using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UnityEngine; 

public class Token : MonoBehaviour
{
    private static string tokenFilePath = "Assets/Configuration/tokenChat.txt";


    public static string RetrieveToken()
    {
        String line;
        try
        {
            using (StreamReader reader = new StreamReader(tokenFilePath))
            {
                var token = reader.ReadLine();
                //Debug.Log("Your token is: " + token);
                return token;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Your tokenFile does not exists! " + ex.Message);
        }
        return null;
    }

    // void Start()
    // {
    //     RetrieveToken();
    // }
}

