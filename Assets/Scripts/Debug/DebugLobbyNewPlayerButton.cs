using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugLobbyButton : MonoBehaviour
{

    private void OnEnable()
    {
        GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
        {
            PlayerManager.instance.createPlayer($"DebugPlayer{PlayerManager.instance.debug_players.Count + 1}");
        });
    }
    void Update()
    {
        
    }
}
