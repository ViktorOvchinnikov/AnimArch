using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UnityEngine; 

public class Token : MonoBehaviour
{
    private string tokenFilePath = "tokenChat.txt";
    public string TokenValue { get; internal set; }

    Token() { 
        RetrieveToken();
    }

    private void RetrieveToken()
    {
        String line;
        try
        {
            using (StreamReader reader = new StreamReader(tokenFilePath))
            {
                TokenValue = reader.ReadLine();
                Debug.LogError("Your token is: " + TokenValue);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Your tokenFile does not exists! " + ex.Message);
        }
    }

    void Start()
    {
        Token connector = new Token();
    }
}

