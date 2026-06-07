using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

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
            GameObject spawnedPlayer;

            if (player.playerId == main_game_manager.game_stat.turnInfo.currentTurnPlayerId)
            {
                spawnedPlayer = Instantiate(playerPrefab, playerSpawnPoints[0].transform);
            }
            else
            {
                spawnedPlayer = Instantiate(playerPrefab, playerSpawnPoints[i++].transform);
            }

            spawnedPlayer.name = player.name;

            playerObjects.Add(spawnedPlayer);

            PlayerController player_script;
            player_script = spawnedPlayer.GetComponent<PlayerController>();
            player_script.this_player = player;

            if (player.playerId == playerManager.this_player.id)
            {
                player_script.is_local_player = true;
            }
        }
    }
    
}
