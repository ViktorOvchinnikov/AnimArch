using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class GPTMessage: MonoBehaviour
{
    private string key;
    private string apiUrl = "https://api.openai.com/v1/chat/completions";
    private string presetMessage = "Hello, how are you?";

    public void SendMessage()
    {
        key = Token.RetrieveToken();
        StartCoroutine(SenGPTMessage(presetMessage));
    }

    private IEnumerator SenGPTMessage(string userMessage)
    {

        var requestData = new RequestData
        {
            model = "gpt-4",
            messages = new List<Message>
            {
                //new Message { role = "system", content = "You are a helpful assistant." },
                new Message { role = "user", content = userMessage }
            },
            max_tokens = 100,
            temperature = 0.7f
        };

        string jsonRequest = JsonUtility.ToJson(requestData);
        byte[] jsonToSend = Encoding.UTF8.GetBytes(jsonRequest);

        using (UnityWebRequest webRequest = new UnityWebRequest(apiUrl, "POST"))
        {
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Authorization", $"Bearer {key}");

            webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
            webRequest.downloadHandler = new DownloadHandlerBuffer();

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError($"Connection error: {webRequest.error}");
            }
            else if (webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Protocol error: {webRequest.error}");
                Debug.LogError($"Response: {webRequest.downloadHandler.text}");
            }
            else
            {
                string jsonResponse = webRequest.downloadHandler.text;
                //Debug.Log($"ChatGPT Response: {jsonResponse}");
                ParseResponse(jsonResponse);
            }
        }
    }

    private void ParseResponse(string jsonResponse)
    {
        var response = JsonUtility.FromJson<Response>(jsonResponse);
        string chatbotReply = response.choices[0].message.content;
        Debug.Log($"ChatGPT: {chatbotReply}");
    }

    [System.Serializable]
    private class RequestData
    {
        public string model;
        public List<Message> messages;
        public int max_tokens;
        public float temperature;
    }

    [System.Serializable]
    private class Message
    {
        public string role;
        public string content;
    }

    [System.Serializable]
    private class Response
    {
        public List<Choice> choices;

        [System.Serializable]
        public class Choice
        {
            public Message message;
        }
    }
}
