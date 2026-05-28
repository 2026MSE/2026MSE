using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLobby : MonoBehaviour
{
    [Header("UI References")]
    public PlayerSlotUI[] playerSlots;

    public int maxPlayers = 4;

    // 방 인원수와 리스트 인원수를 각각 따로 추적합니다.
    private int lastRoomIdCount = 0;
    private int lastPlayerListCount = 0;

    private PlayerManager player_manager;
    private MainGameManager main_game_manager;

    private void OnEnable()
    {
        Debug.Log("PlayerLobby Start");
        player_manager = PlayerManager.instance;
        main_game_manager = MainGameManager.instance;
    }

    private void Update()
    {
        if (main_game_manager.game_stat.roomInfo != null)
        {
            int currentRoomCount = main_game_manager.game_stat.roomInfo.playerIds.Count;
            int currentListCount = player_manager.playerList != null ? player_manager.playerList.Count : 0;

            // 방의 ID 개수가 바뀌었거나, 폴링을 통해 실제 플레이어 정보 리스트 개수가 바뀌었을 때 UI 갱신
            if (currentRoomCount != lastRoomIdCount || currentListCount != lastPlayerListCount)
            {
                UpdateRoomUI();

                // 마지막으로 UI를 갱신했던 시점의 개수를 저장
                lastRoomIdCount = currentRoomCount;
                lastPlayerListCount = currentListCount;
            }
        }
    }

    private void UpdateRoomUI()
    {
        Debug.Log("UpdateRoomUI");

        for (int i = 0; i < playerSlots.Length; i++)
        {
            // [핵심 해결 부분] 
            // i가 방 인원수보다 작고, '동시에' 아직 서버에서 받아온 playerList의 개수보다도 작을 때만 접근합니다.
            if (i < player_manager.currentRoom.playerIds.Count &&
                player_manager.playerList != null &&
                i < player_manager.playerList.Count)
            {
                playerSlots[i].SetPlayer(player_manager.playerList[i].name);
            }
            else
            {
                // 정보가 아직 덜 왔거나 빈 자리라면 일단 Empty 처리
                playerSlots[i].SetEmpty();
            }
        }
    }
}