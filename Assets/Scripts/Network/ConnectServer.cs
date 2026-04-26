using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NewBehaviourScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void GetRandomPerson()
    {
        // start a coroutine to make a GET request
        StartCoroutine(RandomRequest());
    }
    IEnumerator RandomRequest()
    {
        // UnityWebRequest can do GET, POST, etc web requests.
        UnityWebRequest webRequest = UnityWebRequest.Get("http://localhost:8080/random");
        // set the request header: tell the server that we accept json as a return value
        webRequest.SetRequestHeader("Accept", "application/json");
        // Make the request and wait for it to complete.
        yield return webRequest.SendWebRequest();

        // check if the request was successful
        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            string json = webRequest.downloadHandler.text;
            // do something with the json string
        }
    }
}
