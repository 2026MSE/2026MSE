using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

public class ServerManager : MonoBehaviour
{
    public static ServerManager instance { get; private set; }

    public bool isUsingServer = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public async UniTask YutRequest()
    {
        UnityWebRequest webRequest = UnityWebRequest.Get("http://localhost:8080/private/result");
        webRequest.SetRequestHeader("Accept", "application/json");
        
        await webRequest.SendWebRequest().ToUniTask();

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            string json = webRequest.downloadHandler.text;
            JsonUtility.FromJsonOverwrite(json, PrivateRoom_GameManager.instance.yutResult);
        }
    }

    // ¹Ì¿Ï¼º
    public async UniTaskVoid PrivateExitRequest()
    {
        UnityWebRequest webRequest = new UnityWebRequest("http://localhost:8080/private/exit", "POST");
        webRequest.SetRequestHeader("Content-Type", "application/json");

        await webRequest.SendWebRequest().ToUniTask();
        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            
        }
    }
}
