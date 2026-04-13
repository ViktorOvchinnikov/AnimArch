using System;
using AnimArch.Extensions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Visualization.ClassDiagram;
using Visualization.ClassDiagram.ComponentsInDiagram;
using Visualization.ClassDiagram.Editors;
using Visualization.ClassDiagram.Relations;
using Visualization.UI.PopUps;
using EditorChangesHistory;

namespace Visualization.UI
{
    public class UIEditorManager : Singleton<UIEditorManager>
    {
        public bool active;
        private IClassDiagramBuilder _classDiagramBuilder;
        public MainEditor mainEditor;

        public AbstractMethodPopUp methodPopUp;

        public string ParameterPopUpCallee = "";

        [SerializeField]
        public bool NetworkEnabled;

        public AddAttributePopUp addAttributePopUp;
        public RenameAttributePopUp renameAttributePopUp;

        public AddMethodPopUp addMethodPopUp;
        public EditMethodPopUp editMethodPopUp;

        public AddParameterPopUp addParameterPopUp;
        public EditParameterPopUp editParameterPopUp;

        public AddClassPopUp addClassPopUp;
        public RenameClassPopUp renameClassPopUp;

        public ConfirmPopUp confirmPopUp;
        public ErrorPopUp errorPopUp;
        public ExitPopUp exitPopUp;

        public State state;
        public Relation relation;
        private RelationInDiagram _relationBeingEdited;

        public bool isNetworkDisabledOrIsServer()
        {
            return (Instance.NetworkEnabled && NetworkManager.Singleton.IsServer) || !Instance.NetworkEnabled;
        }

        public void InitializeCreation()
        {
            if (DiagramPool.Instance.ClassDiagram.graph != null)
            {
                return;
            }

            _classDiagramBuilder.CreateGraph();
            _classDiagramBuilder.MakeNetworkedGraph();
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            _classDiagramBuilder = ClassDiagramBuilderFactory.Create();
            mainEditor = MainEditorFactory.Create(_classDiagramBuilder.visualEditor);
        }

        public void CreateNewDiagram()
        {
            mainEditor.ClearDiagram();
            StartEditing();
        }
        
        public void StartEditing()
        {
            if (DiagramPool.Instance.ClassDiagram.graph == null)
                InitializeCreation();
            Debug.Assert(DiagramPool.Instance.ClassDiagram.graph);
            
            // Activate all buttons except suggestions buttons
            DiagramPool.Instance.ClassDiagram.graph.GetComponentsInChildren<Button>(includeInactive: true)
                .ForEach(x =>
                {
                    if (!ShouldAutoActivateButton(x))
                    {
                        return;
                    }
                    x.gameObject.SetActive(true);
                });
            
            active = true;
        }

        public void EndEditing()
        {
            active = false;
            MenuManager.Instance.isSelectingNode = false;

            DiagramPool.Instance.ClassDiagram.graph.GetComponentsInChildren<Button>()
                .ForEach(x => x.gameObject.SetActive(false));
        }

        public static void SetDiagramButtonsActive(bool enable)
        {
            if (DiagramPool.Instance.ClassDiagram.graph != null)
                DiagramPool.Instance.ClassDiagram.graph.GetComponentsInChildren<GraphicRaycaster>()
                    .ForEach(x => x.enabled = enable);
        }

        public static bool ShouldAutoActivateButton(string buttonName)
        {
            return buttonName != "AcceptButton" &&
                   buttonName != "DeclineButton" &&
                   buttonName != "AcceptSuggestionButton" &&
                   buttonName != "DeclineSuggestionButton" &&
                   buttonName != "VisualizationAcceptButton" &&
                   buttonName != "VisualizationDeleteButton";
        }

        public static bool ShouldAutoActivateButton(Button button)
        {
            if (button == null)
                return false;

            if (!ShouldAutoActivateButton(button.gameObject.name))
                return false;

            Transform current = button.transform;
            while (current != null)
            {
                if (current.name == "ChangesVisualization")
                    return false;
                current = current.parent;
            }

            return true;
        }

        public void StartSelection(string newRelationType)
        {
            ParseRelationSelection(newRelationType, out string relType, out string direction);

            relation = new Relation
            {
                ConnectorXmiId = Guid.NewGuid().ToString(),
                PropertiesEaType = relType,
                PropertiesDirection = direction
            };

            _relationBeingEdited = null;
            state = new SelectFirstState();
            MenuManager.Instance.isSelectingNode = true;
        }

        public void BeginRelationTypeEdit(GameObject relationObject)
        {
            if (relationObject == null)
            {
                return;
            }

            _relationBeingEdited = DiagramPool.Instance.ClassDiagram.Relations
                .Find(item => item?.VisualObject != null && item.VisualObject.Equals(relationObject));

            if (_relationBeingEdited == null)
            {
                return;
            }

            SelectionPopUp selectionPopUp = FindObjectOfType<SelectionPopUp>(true);
            if (selectionPopUp == null)
            {
                Debug.LogError("SelectionPopUp is not found. Cannot edit relation type.");
                _relationBeingEdited = null;
                return;
            }

            Relation parsedRelation = _relationBeingEdited.ParsedRelation;
            selectionPopUp.SelectRelationOption(parsedRelation?.PropertiesEaType, parsedRelation?.PropertiesDirection);
            selectionPopUp.SetRelationEditMode(true);
            selectionPopUp.ActivateCreation();
        }

        public bool TryApplyRelationTypeEdit(string selectedRelationType)
        {
            if (_relationBeingEdited == null)
            {
                return false;
            }

            ParseRelationSelection(selectedRelationType, out string relationType, out string relationDirection);
            bool updated = mainEditor.UpdateRelationType(_relationBeingEdited.VisualObject, relationType, relationDirection);

            if (!updated)
            {
                errorPopUp.ActivateCreation();
            }

            _relationBeingEdited = null;
            return true;
        }

        public void CancelPendingRelationTypeEdit()
        {
            _relationBeingEdited = null;
        }

        public void SelectNode(GameObject selected)
        {
            if (!active || state == null)
            {
                return;
            }

            state.Select(selected);
        }

        public void EndSelection()
        {
            SetDiagramButtonsActive(true);
            if (relation != null &&
                !string.IsNullOrWhiteSpace(relation.SourceModelName) &&
                Animation.Animation.Instance != null)
            {
                Animation.Animation.Instance.HighlightClass(relation.SourceModelName, false);
            }

            if (MenuManager.Instance != null)
            {
                MenuManager.Instance.isSelectingNode = false;
            }

            relation = null;
            state = null;

            GameObject selectionPanel = ResolveSelectionPanel();
            if (selectionPanel != null)
            {
                selectionPanel.SetActive(false);
            }
        }

        public void ShowSelectionPanel(bool active)
        {
            GameObject selectionPanel = ResolveSelectionPanel();
            if (selectionPanel != null)
            {
                selectionPanel.SetActive(active);
            }
        }

        private static GameObject ResolveSelectionPanel()
        {
            MediatorSelectionPanel mediatorSelectionPanel = FindObjectOfType<MediatorSelectionPanel>(true);
            return mediatorSelectionPanel != null ? mediatorSelectionPanel.gameObject : null;
        }

        public void AddRelation()
        {
            if (relation == null)
                return;

            if(DiagramPool.Instance.ClassDiagram.FindRelation(relation.SourceModelName, relation.TargetModelName, relation.PropertiesEaType) != null)
            {
                errorPopUp.ActivateCreation();
                return;
            }

            string serializedData = DiagramChangeSerializer.SerializeAddRelation(relation.SourceModelName, relation.TargetModelName);
            DiagramChangeEvent changeEvent = new DiagramChangeEvent(ChangeType.AddRelation, serializedData);
            DiagramChangeTracker.Instance.TrackChange(changeEvent);
            
            mainEditor.CreateRelation(relation);
            EndSelection();
        }

        private static void ParseRelationSelection(string relationSelection, out string relationType, out string relationDirection)
        {
            string selectionValue = (relationSelection ?? string.Empty).Trim();
            string[] tokens = selectionValue.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            relationType = tokens.Length > 1 ? tokens[^1] : selectionValue;
            relationDirection = tokens.Length > 1 ? "none" : "Source -> Destination";

            if (string.IsNullOrWhiteSpace(relationType))
            {
                relationType = "Association";
            }
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.Escape) && !exitPopUp.gameObject.activeSelf)
            {
                exitPopUp.ActivateCreation();
            }
        }
    }
}
