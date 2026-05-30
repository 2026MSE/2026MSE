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
        player_manager = PlayerManager.instance;
        main_game_manager = MainGameManager.instance;
    }

    private void Update()
    {
        if (main_game_manager.game_stat.roomInfo != null)
        {
            int currentRoomCount = main_game_manager.game_stat.roomInfo.playerIds.Count;
            int currentListCount = main_game_manager.game_stat.players != null ? main_game_manager.game_stat.players.Count : 0;

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
        for (int i = 0; i < playerSlots.Length; i++)
        {
            if (i < main_game_manager.game_stat.roomInfo.playerIds.Count &&
                main_game_manager.game_stat.players != null &&
                i < main_game_manager.game_stat.players.Count)
            {
                playerSlots[i].SetPlayer(main_game_manager.game_stat.players[i].name);
            }
            else
            {
                // 정보가 아직 덜 왔거나 빈 자리라면 일단 Empty 처리
                playerSlots[i].SetEmpty();
            }
        }
    }
}