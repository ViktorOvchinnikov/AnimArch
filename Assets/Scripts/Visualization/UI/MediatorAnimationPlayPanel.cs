using UnityEngine;
using UnityEngine.UIElements;
using Visualization.Animation;
using Visualization.TODO;

namespace Visualization.UI
{
    public class MediatorAnimationPlayPanel : Mediator 
    {
        [SerializeField] private GameObject PlayPanel;
        [SerializeField] private GameObject ButtonPlay;
        [SerializeField] private GameObject ButtonStop;
        [SerializeField] private GameObject ButtonPause;        
        [SerializeField] private GameObject StepButton;
        MediatorColorSelectionPanel MediatorColorSelectionPanel;

        public override void OnClicked(GameObject gameObject)
        {
            if (ReferenceEquals(gameObject, ButtonPlay))
            {
                MediatorColorSelectionPanel.RandomPallete();
                Debug.Log("RANDOM COLORS WERE SET");
            }
            else if (ReferenceEquals(gameObject, ButtonStop))
            {
                OnStopBtnClicked();
            }
            else if (ReferenceEquals(gameObject, ButtonPause))
            {
                OnPauseBtnClicked();
            }
            else if (ReferenceEquals(gameObject, StepButton))
            {
                OnStepBtnClicked();
            }
            else
            {
                OnClickedDefault(gameObject);
            }
        }

        private void OnPlayBtnClicked()
        {
            Animation.Animation.Instance.StartAnimation();
        }
        private void OnStopBtnClicked()
        {
            Animation.Animation.Instance.UnhighlightAll();
            MenuManager.Instance.HideErrorPanelOnStopButton();
        }
        private void OnPauseBtnClicked()
        {
            Animation.Animation.Instance.Pause();
        }
        private void OnStepBtnClicked()
        {
            MenuManager.Instance.ChangeMode();
        }

    }
}