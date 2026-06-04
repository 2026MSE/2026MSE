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
        //SceneManager.LoadScene("emoticon", LoadSceneMode.Additive);
        Instantiate();
    }

    private void Instantiate()
    {
        int i = 1;
        foreach (PlayerInfo player in main_game_manager.game_stat.players)
        {
            //6/3 영준 기존 조건문 이모지관련 수정
            // 1. Instantiate 한 결과를 먼저 변수(spawnedPlayer)에 담습니다.
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

            // 2. 리스트에 담기
            playerObjects.Add(spawnedPlayer);

            // 3. [추가된 부분] 방금 스폰된 캐릭터에게 "너의 ID는 이거야!" 라고 알려줍니다.
            //spawnedPlayer.GetComponent<PlayerEmoticonDisplay>().ownerId = player.playerId;

            if (player.playerId == playerManager.this_player.id)
            {
                spawnedPlayer.GetComponent<PlayerController>().is_local_player = true;
            }
        }
    }
    
    void Update()
    {
        
    }
}
