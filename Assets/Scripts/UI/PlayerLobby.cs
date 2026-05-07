using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLobby : MonoBehaviour
{
    [Header("UI References")]
    public PlayerSlotUI[] playerSlots;

    public int maxPlayers = 4;
    private int playerCount = 0;

    private PlayerManager player_manager;

    private void Start()
    {
        player_manager = PlayerManager.instance;
        UpdateRoomUI();
    }

    private void Update()
    {
        if(player_manager.currentRoom != null)
        {
            if (player_manager.currentRoom.playerIds.Count != playerCount)
            {
                UpdateRoomUI();
                playerCount = player_manager.currentRoom.playerIds.Count;
            }
        }
    }

    private void UpdateRoomUI()
    {
        for (int i = 0; i < playerSlots.Length; i++)
        {
            if (i < player_manager.playerList.Count)
            {
                playerSlots[i].SetPlayer(player_manager.playerList[i]);
            }
            else
            {
                playerSlots[i].SetEmpty();
            }
        }
    }

}
