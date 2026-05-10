using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerInstantiate : MonoBehaviour
{
    PlayerManager playerManager;

    public List<GameObject> playerObjects { get; set; } = new List<GameObject>();
    public GameObject playerPrefab;
    public List<GameObject> playerSpawnPoints;

    void Start()
    {
        playerManager = PlayerManager.instance;
        for(int i = 0; i < playerManager.playerList.Count; i++) 
        {
            playerObjects[i] = Instantiate(playerPrefab, playerSpawnPoints[i].transform);
            if (playerManager.playerList[i].playerId == playerManager.this_player.id)
            {
                playerObjects[i].GetComponent<PlayerController>().is_local_player = true;
            }
        }
    }

    
    void Update()
    {
        
    }
}
