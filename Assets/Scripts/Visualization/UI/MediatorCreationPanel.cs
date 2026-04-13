using UnityEngine;
using UnityEngine.UIElements;
using TMPro;
using Visualization.Animation;
using Visualization.ABTesting;
using Visualization.UI.PopUps;

namespace Visualization.UI
{
    public class MediatorCreationPanel : Mediator 
    {
        [SerializeField] private GameObject CreationPanel;
        [SerializeField] private GameObject BackButton;
        [SerializeField] private GameObject SaveButton;
        [SerializeField] private GameObject AddClass;
        [SerializeField] private GameObject AddRelation;
        [SerializeField] private GameObject SuggestionsButton;
        private const string CompleteTaskButtonText = "Complete Task";
        private const string SidebarSectionTitle = "A/B Test";
        public MediatorRightMenu MediatorRightMenu;
        public MediatorMainPanel MediatorMainPanel;
        public MediatorAddClassPopUp MediatorAddClassPopUp;
        public MediatorSelectionPopUp MediatorSelectionPopUp;

        private void Start()
        {
            ApplyABTestButtonLabels();
        }

        public override void OnClicked(GameObject gameObject)
        {
            if (ReferenceEquals(gameObject, BackButton))
            {
                OnBackButtonClicked();
            }
            else if (ReferenceEquals(gameObject, SaveButton))
            {
                OnSaveButtonClicked();
            }
            else if (ReferenceEquals(gameObject, AddClass))
            {
                OnAddClassClicked();
            }
            else if (ReferenceEquals(gameObject, AddRelation))
            {
                OnAddRelationClicked();
            }
            else if (ReferenceEquals(gameObject, SuggestionsButton))
            {
                OnSuggestionsButtonClicked();
            }
            else if (gameObject != null && gameObject.name == "SuggestionsAcceptAllButton")
            {
                AcceptChanges.SaveAllSuggestions();
            }
            else if (gameObject != null && gameObject.name == "SuggestionsRejectAllButton")
            {
                DeclineChanges.DeclineAllSuggestions();
            }
            else
            {
                OnClickedDefault(gameObject);
            }
        }

        private void OnBackButtonClicked()
        {
            MediatorRightMenu.SetActiveRightMenu(true);
            CreationPanel.SetActive(false);
            MediatorMainPanel.SetActiveMainPanel(true);
            UIEditorManager.Instance.EndEditing();
        }
        private void OnSaveButtonClicked()
        {
            FileLoader.Instance.SaveDiagram();
        }
        private void OnAddClassClicked()
        {
            MediatorAddClassPopUp.SetActiveAddClassPopUp(true); 
            MediatorAddClassPopUp.ActivateCreation();
        }
        private void OnAddRelationClicked()
        {
            MediatorSelectionPopUp.SetActiveSelectionPopUp(true);
            MediatorSelectionPopUp.ActivateCreation();
        }
        
        private void OnSuggestionsButtonClicked()
        {
            TooltipManager.Instance.HideTooltip();
            if (!ABTestManager.TryOpenCompleteTaskConfirmation(out string message))
            {
                Debug.LogWarning(message);
                return;
            }

            Debug.Log(message);
        }

        public void SetActiveCreationPanel(bool active)
        {
            CreationPanel.SetActive(active);
        }

        private void ApplyABTestButtonLabels()
        {
            if (SuggestionsButton != null)
            {
                TMP_Text buttonText = SuggestionsButton.GetComponentInChildren<TMP_Text>(includeInactive: true);
                if (buttonText != null)
                {
                    buttonText.text = CompleteTaskButtonText;
                }
            }

            GameObject sectionTitleObject = GameObject.Find("SuggestionsMenuTxt");
            if (sectionTitleObject != null)
            {
                TMP_Text sectionTitleText = sectionTitleObject.GetComponent<TMP_Text>();
                if (sectionTitleText != null)
                {
                    sectionTitleText.text = SidebarSectionTitle;
                }
            }
        }
        
    }
}
