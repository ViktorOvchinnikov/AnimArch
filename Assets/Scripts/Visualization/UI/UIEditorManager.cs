using System;
using AnimArch.Extensions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Visualization.ClassDiagram;
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
                    string buttonName = x.gameObject.name;
                    if (buttonName == "AcceptButton" || 
                        buttonName == "DeclineButton" ||
                        buttonName == "VisualizationAcceptButton" || 
                        buttonName == "VisualizationDeleteButton")
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

        public void StartSelection(string newRelationType)
        {
            var type = newRelationType.Split();
            var relType = type.Length > 1 ? type[1] : type[0];
            relation = new Relation
            {
                ConnectorXmiId = Guid.NewGuid().ToString(),
                PropertiesEaType = relType,
                PropertiesDirection = type.Length > 1 ? "none" : "Source -> Destination"
            };
            state = new SelectFirstState();
            MenuManager.Instance.isSelectingNode = true;
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
            if (relation.SourceModelName != null)
            {
                Animation.Animation.Instance.HighlightClass(relation.SourceModelName, false);
            }
            MenuManager.Instance.isSelectingNode = false;
            relation = null;
            state = null;
            GameObject.Find("SelectionPanel").SetActive(false);
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

        private void Update()
        {
            if (Input.GetKey(KeyCode.Escape) && !exitPopUp.gameObject.activeSelf)
            {
                exitPopUp.ActivateCreation();
            }
        }
    }
}
