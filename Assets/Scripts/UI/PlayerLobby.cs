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

    private void OnEnable()
    {
        Debug.Log("PlayerLobby Start");
        player_manager = PlayerManager.instance;
    }

    private void Update()
    {
        if (player_manager.currentRoom != null)
        {
            //Debug.Log("Current player count: " + player_manager.currentRoom.playerIds.Count);
            if (player_manager.currentRoom.playerIds.Count != playerCount)
            {
                UpdateRoomUI();
                playerCount = player_manager.currentRoom.playerIds.Count;
            }
        }
    }

    private void UpdateRoomUI()
    {
        Debug.Log("UpdateRoomUI");
        //if(player_manager.playerList == null)
        //{
        //    Debug.LogWarning("No room info available to update UI.");
        //    return;
        //}
        for (int i = 0; i < playerSlots.Length; i++)
        {
            //Debug.Log(i + ": " + player_manager.playerList[i].name);
            if (i < player_manager.currentRoom.playerIds.Count)
            {
                playerSlots[i].SetPlayer(player_manager.currentRoom.playerIds[i]);
            }
            else
            {
                playerSlots[i].SetEmpty();
            }
        }
    }

}
