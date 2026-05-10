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

        int i = 1;
        foreach(PlayerInfo player in playerManager.playerList)
        {
            if(player.playerId == MainGameManager.instance.turnInfo.currentTurnPlayerId)
            {
                playerObjects.Add(Instantiate(playerPrefab, playerSpawnPoints[0].transform));
            }
            else
            {
                playerObjects.Add(Instantiate(playerPrefab, playerSpawnPoints[i++].transform));
            }
            if(player.playerId == playerManager.this_player.id)
            {
                playerObjects[playerObjects.Count - 1].GetComponent<PlayerController>().is_local_player = true;
            }
        }
    }

    
    void Update()
    {
        
    }
}
