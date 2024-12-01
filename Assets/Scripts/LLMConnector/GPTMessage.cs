using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;

public class GPTMessage: BaseModel
{
    private string apiKey;
    private string apiUrl = "https://api.openai.com/v1/chat/completions";
    private string defaultQuestion = "Hello, how are you?";
    private string lastResponse = "No response";
    private string _chatResponse = "Default response";

    public void SendMessageAfterClick()
    {
        apiKey = Token.RetrieveToken();
        var res = SendMessage(defaultQuestion);
        Debug.LogError($"Response: {res}");
    }
    public override string SendMessage(string message)
    {
        // Start the coroutine to handle the async logic
        StartCoroutine(SendMessageCoroutineFirst(message));

        // Wait a frame for the response to be processed before returning
        Debug.LogError($"_chatResponse 1: {_chatResponse}");
        return _chatResponse;
    }

    // This coroutine handles the asynchronous request
    private IEnumerator SendMessageCoroutineFirst(string userMessage)
    {
        Task<string> task = FetchAndStoreResponse(userMessage);

        // Wait for the task to complete
        yield return new WaitUntil(() => task.IsCompleted);

        // Set the response once the task has finished
        _chatResponse = task.Result; // Assign the result to _chatResponse
        Debug.LogError($"_chatResponse 2: {_chatResponse}");
    }

    // This method handles the async logic for fetching and storing the response
    public async Task<string> FetchAndStoreResponse(string userMessage)
    {
        try
        {
            // Wait for the completion of SendMessageCoroutine
            string response = await SendMessageCoroutine(userMessage);
            lastResponse = response;  // Store the response
            return response;
        }
        catch (Exception ex)
        {
            Debug.LogError("An error occurred: " + ex.Message);
            return $"Error: {ex.Message}";
        }
    }

    public async void SendMessageAsync(string userMessage)
    {
        await FetchAndStoreResponse(userMessage);
        //Debug.Log("Last Response last last:" + lastResponse);
        _chatResponse = lastResponse;
    }

    

    private async Task<string> SendMessageCoroutine(string userMessage)
    {
        var requestData = new RequestData
        {
            model = "gpt-4",
            messages = new List<Message>
        {
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

            // Create a TaskCompletionSource to handle awaiting
            var tcs = new TaskCompletionSource<bool>();
            webRequest.SendWebRequest().completed += operation => tcs.SetResult(true);
            await tcs.Task;

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
                string parsedResponse = ParseResponse(jsonResponse);
                return parsedResponse;
            }
        }
    }



    private string ParseResponse(string jsonResponse)
    {
        var response = JsonUtility.FromJson<Response>(jsonResponse);
        string chatbotReply = response.choices[0].message.content;
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
