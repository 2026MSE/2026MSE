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
    MainGameManager main_game_manager;

    IEnumerator Start()
    {
        playerManager = PlayerManager.instance;
        main_game_manager = MainGameManager.instance;

        yield return new WaitUntil(() => main_game_manager.game_stat.turnInfo.currentTurnPlayerId != null);
        Instantiate();
    }

    private void Instantiate()
    {
        int i = 1;
        foreach (PlayerInfo player in main_game_manager.game_stat.players)
        {
            if (player.playerId == main_game_manager.game_stat.turnInfo.currentTurnPlayerId)
            {
                playerObjects.Add(Instantiate(playerPrefab, playerSpawnPoints[0].transform));
            }
            else
            {
                playerObjects.Add(Instantiate(playerPrefab, playerSpawnPoints[i++].transform));
            }
            if (player.playerId == playerManager.this_player.id)
            {
                playerObjects[playerObjects.Count - 1].GetComponent<PlayerController>().is_local_player = true;
            }
            playerObjects[playerObjects.Count - 1].name = player.name;
        }
    }
    
    void Update()
    {
        
    }
}
