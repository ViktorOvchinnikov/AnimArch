using System.IO;
using Microsoft.Msagl.Core.DataStructures;
using UnityEngine;
using UnityEngine.UIElements;
using Visualization.Animation;
 

namespace Visualization.UI
{
    public class MediatorMainPanel : Mediator
    {
        [SerializeField] private GameObject MainPanel;
        [SerializeField] private GameObject Toggle;
        [SerializeField] private GameObject PatternCatalogueButton;
        //Only For test purpose
        [SerializeField] public Token token; 

        public MediatorDiagram MediatorDiagram;
        public MediatorMasking MediatorMasking;
        public MediatorAnimation MediatorAnimation;
        public MediatorCreationPanel MediatorCreationPanel;
        public MediatorAnimationPlay MediatorAnimationPlay;
        public MediatorPatternCatalogue MediatorPatternCatalogue;
        public override void OnClicked(GameObject gameObject)
        {
            if (ReferenceEquals(gameObject, Toggle))
            {
                OnToggleValueChanged();
            }
            else if (ReferenceEquals(gameObject, PatternCatalogueButton))
            {
                OnPatternCatalogueButtonClicked();
            }
            else
            {
                OnClickedDefault(gameObject);
            }
        }

        private void OnToggleValueChanged()
        {
            MenuManager.Instance.HideGraphRelations();
        }
        private void OnPatternCatalogueButtonClicked()
        {
            Debug.Log("Pattern Catalogue Button Clicked");
            SetActiveMainPanel(false);
            SetActivePatternCataloguePanel(true);
        }

        public void SetActiveMainPanel(bool active)
        {
            MainPanel.SetActive(active); 
            PatternCatalogueButton.SetActive(active);
        }
        public void SetActiveCreationPanel(bool active)
        {
            MediatorCreationPanel.SetActiveCreationPanel(active);
        }

        public void SetActivePatternCataloguePanel(bool active)
        {
            MediatorPatternCatalogue.SetActivePatternCataloguePanel(active);
            PatternCatalogueButton.SetActive(!active);
        }  
        public void UnshowPatternCatalogue()
        {
            Debug.Log("Unshow Pattern Catalogue");
            SetActiveMainPanel(true);
            SetActivePatternCataloguePanel(false);
        }
    }
}