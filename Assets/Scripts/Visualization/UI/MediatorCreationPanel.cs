using UnityEngine;
using UnityEngine.UIElements;
using Visualization.Animation;
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
        public MediatorRightMenu MediatorRightMenu;
        public MediatorMainPanel MediatorMainPanel;
        public MediatorAddClassPopUp MediatorAddClassPopUp;
        public MediatorSelectionPopUp MediatorSelectionPopUp;

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
            SuggestedDiagram.ToggleSuggestions();
        }

        public void SetActiveCreationPanel(bool active)
        {
            CreationPanel.SetActive(active);
        }
        
    }
}