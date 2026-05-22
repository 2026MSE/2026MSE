using System.Collections.Generic;
using UnityEngine;

public class PlayerLobby : MonoBehaviour
{
    [Header("UI References")]
    public PlayerSlotUI[] playerSlots;

    [Header("Option")]
    public int maxPlayers = 4;

    private PlayerManager playerManager;

    private int lastPlayerListCount = -1;
    private int lastRoomIdCount = -1;
    private string lastRoomId = "";

    private void OnEnable()
    {
        Debug.Log("[PlayerLobby] OnEnable");

        playerManager = PlayerManager.instance;

        ClearSlots();
        ForceUpdateRoomUI();
    }

    private void Update()
    {
        if (playerManager == null)
            playerManager = PlayerManager.instance;

        if (playerManager == null)
        {
            ClearSlots();
            return;
        }

        int currentPlayerListCount =
            playerManager.playerList != null ? playerManager.playerList.Count : 0;

        int currentRoomIdCount =
            playerManager.currentRoom != null &&
            playerManager.currentRoom.playerIds != null
                ? playerManager.currentRoom.playerIds.Count
                : 0;

        string currentRoomId =
            playerManager.currentRoom != null
                ? playerManager.currentRoom.roomId
                : "";

        if (currentPlayerListCount != lastPlayerListCount ||
            currentRoomIdCount != lastRoomIdCount ||
            currentRoomId != lastRoomId)
        {
            ForceUpdateRoomUI();

            lastPlayerListCount = currentPlayerListCount;
            lastRoomIdCount = currentRoomIdCount;
            lastRoomId = currentRoomId;
        }
    }

    public void ForceUpdateRoomUI()
    {
        if (playerSlots == null || playerSlots.Length == 0)
            return;

        List<PlayerInfo> playerInfos =
            playerManager != null ? playerManager.playerList : null;

        RoomInfo roomInfo =
            playerManager != null ? playerManager.currentRoom : null;

        for (int i = 0; i < playerSlots.Length; i++)
        {
            if (playerSlots[i] == null)
                continue;

            // 1순위: 서버에서 받은 플레이어 상세 정보
            if (playerInfos != null && i < playerInfos.Count && playerInfos[i] != null)
            {
                string displayName = playerInfos[i].name;

                if (string.IsNullOrEmpty(displayName))
                    displayName = playerInfos[i].playerId;

                playerSlots[i].SetPlayer(displayName);
                continue;
            }

            // 2순위: 아직 상세 정보가 안 왔으면 playerId라도 표시
            if (roomInfo != null &&
                roomInfo.playerIds != null &&
                i < roomInfo.playerIds.Count)
            {
                string playerId = roomInfo.playerIds[i];

                if (!string.IsNullOrEmpty(playerId))
                {
                    playerSlots[i].SetPlayer("Player " + (i + 1));
                    continue;
                }
            }

            playerSlots[i].SetEmpty();
        }
    }

    private void ClearSlots()
    {
        if (playerSlots == null)
            return;

        foreach (PlayerSlotUI slot in playerSlots)
        {
            if (slot != null)
                slot.SetEmpty();
        }
    }
}