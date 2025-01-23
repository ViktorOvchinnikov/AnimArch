using UnityEngine;
using System.Threading.Tasks;
using System.Collections;

public class GPTManager : MonoBehaviour
{
    private GPTMessage gptMessage;

    private void Start()
    {
        gptMessage = new GPTMessage();
    }

    public void SendMessageAfterClick()
    {
        StartCoroutine(CallSendMessage());
    }

    private IEnumerator CallSendMessage()
    {
        Task task = gptMessage.SendMessageAsync();
        while (!task.IsCompleted)
        {
            yield return null; 
        }

        if (task.Exception != null)
        {
            Debug.LogError("Error occurred while sending message: " + task.Exception.Message);
        }
    }
}
