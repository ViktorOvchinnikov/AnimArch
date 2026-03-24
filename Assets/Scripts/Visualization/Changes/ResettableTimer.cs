using System.Collections;
using UnityEngine;

namespace Visualization.Changes
{
    public class ResettableTimer : MonoBehaviour
    {
        [SerializeField] private float delay = 20f;

        private Coroutine timerCoroutine;
        
        private void Awake()
        {
            SuggestedDiagram.SetTimer(this);
        }

        private void OnEnable()
        {
            SuggestedDiagram.SetTimer(this);
        }

        public void OnUserAction()
        {
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
            }
        
            timerCoroutine = StartCoroutine(TimerRoutine());
            Debug.Log("TimerReset");
        }

        private IEnumerator TimerRoutine()
        {
            yield return new WaitForSeconds(delay);
            timerCoroutine = null;
            DoAfterDelay();
        }

        private void DoAfterDelay()
        {
            Debug.Log("20 seconds passed");
        }
    }
}
