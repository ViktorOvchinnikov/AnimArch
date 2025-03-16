using OALProgramControl;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using Visualization.Animation;
using Visualization.ClassDiagram;

namespace Visualization.UI
{
    public class MediatorDiagram : Mediator
    {
        [SerializeField] private GameObject ExpandButton;
        [SerializeField] private GameObject CollapseButton;
        [SerializeField] private GameObject Separator;
        [SerializeField] private GameObject Buttons;
        [SerializeField] private GameObject CreateButton;
        [SerializeField] private GameObject LoadButton;
        [SerializeField] private GameObject EditButton;
        [SerializeField] private GameObject Label;
        [SerializeField] private TextMeshProUGUI LabelText;
        [SerializeField] private UnityEngine.UI.Button RemoveButton;
        public MediatorMainPanel MediatorMainPanel;
        
        public override void OnClicked(GameObject gameObject)
        {
            if (ReferenceEquals(gameObject, ExpandButton))
            {
                OnExpandButtonClicked();
            }
            else if (ReferenceEquals(gameObject, CollapseButton))
            {
                OnCollapseButtonClicked();
            }
            else if (ReferenceEquals(gameObject, CreateButton))
            {
                OnCreateButtonClicked();
            }
            else if (ReferenceEquals(gameObject, LoadButton))
            {
                OnLoadButtonClicked();
            }
            else if (ReferenceEquals(gameObject, EditButton))
            {
                OnEditButtonClicked();
            }
            else if (ReferenceEquals(gameObject, RemoveButton))
            {
                OnRemoveButtonClicked();
            }
            else
            {
                OnClickedDefault(gameObject);
            }
        }

        private void OnExpandButtonClicked()
        {
            TooltipManager.Instance.HideTooltip();
            Separator.SetActive(true);
            Buttons.SetActive(true);
            Label.SetActive(true);
            ExpandButton.SetActive(false);
            CollapseButton.SetActive(true);
        }
        private void OnCollapseButtonClicked()
        {
            TooltipManager.Instance.HideTooltip();
            Separator.SetActive(false);
            Buttons.SetActive(false);
            Label.SetActive(false);
            ExpandButton.SetActive(true);
            CollapseButton.SetActive(false);
        }
        private void OnCreateButtonClicked()
        {
            TooltipManager.Instance.HideTooltip();
            UIEditorManager.Instance.CreateNewDiagram();
            MediatorMainPanel.SetActiveMainPanel(false);
            MediatorMainPanel.SetActiveCreationPanel(true);
        }
        private void OnLoadButtonClicked()
        {
            TooltipManager.Instance.HideTooltip();
            ClassDiagramUtils.ReloadClassDiagram();
            MenuManager.Instance.UnshowAnimation();
            MediatorMainPanel.SetActiveMainPanel(true);
            MediatorMainPanel.SetActiveCreationPanel(false);
        }
        //TODO sprav z toho template method, zopakovat onloadbuttonclicked,
        //Fileloader metoda vynechas tri riadky a das tam cestu z leafu
        //sekvencia krokov sa bude opakovagt viackrat
        // pri nacitani diagramu: vsetko okrem troch riadkov z fileloadera
        // nahlad pri hover: nebudeme schovavat tooltip ani panely, ani unshow animation a z fileloadera iba classdiagrambuilder.loaddiagram
        //
        private void OnEditButtonClicked()
        {
            TooltipManager.Instance.HideTooltip();
            MaskingHandler.Instance.RemoveMasking();
            UIEditorManager.Instance.StartEditing();
            MenuManager.Instance.UnshowAnimation();
            MediatorMainPanel.SetActiveMainPanel(false);
            MediatorMainPanel.SetActiveMainPanel(false);
            MediatorMainPanel.SetActiveCreationPanel(true);
        }
        private void OnRemoveButtonClicked()
        {
            LabelText.text = "";
            RemoveButton.interactable = false;
            TooltipManager.Instance.HideTooltip();
            MenuManager.Instance.RemoveDiagram();
        }

    }
}