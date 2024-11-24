using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;

public class GPTMessage: GPTBase
{
    private string apiKey;
    private string apiUrl = "https://api.openai.com/v1/chat/completions";
    private string defaultQuestion = "Hello, how are you?";

    public void SendMessageAfterClick()
    {
        apiKey = Token.RetrieveToken();
        SendMessage(defaultQuestion);
        //StartCoroutine(SenGPTMessage(presetMessage));
    }

    private void Awake()
    {
        apiKey = Token.RetrieveToken(); 
    }

    public override async Task<string> SendMessage(string userMessage)
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
            webRequest.SetRequestHeader("Authorization", $"Bearer {apiKey}");

            webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
            webRequest.downloadHandler = new DownloadHandlerBuffer();

            var operation = webRequest.SendWebRequest();
            while (!operation.isDone)
                await Task.Yield();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError($"Connection error: {webRequest.error}");
                return $"Error: {webRequest.error}";
            }
            else if (webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Protocol error: {webRequest.error}");
                Debug.LogError($"Response: {webRequest.downloadHandler.text}");
                return $"Error: {webRequest.error}";
            }
            else
            {
                string jsonResponse = webRequest.downloadHandler.text;
                return ParseResponse(jsonResponse);
            }
        }
    }

    private string ParseResponse(string jsonResponse)
    {
        var response = JsonUtility.FromJson<Response>(jsonResponse);
        string chatbotReply = response.choices[0].message.content;
        Debug.Log($"ChatGPT: {chatbotReply}");
        return chatbotReply;
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
