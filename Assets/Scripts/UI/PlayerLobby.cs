using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLobby : MonoBehaviour
{
    [Header("UI References")]
    public PlayerSlotUI[] playerSlots;

    private List<string> currentPlayers = new List<string>();
    private int maxPlayers = 4;

    private void Start()
    {
        UpdateRoomUI();
    }

    public void AddPlayer(string newPlayerName)
    {
        currentPlayers.Add(newPlayerName);
        UpdateRoomUI();
    }

    public void RemovePlayer(string playerName)
    {
        if (currentPlayers.Contains(playerName))
        {
            currentPlayers.Remove(playerName);
            UpdateRoomUI();
        }
    }

    private void UpdateRoomUI()
    {
        for (int i = 0; i < playerSlots.Length; i++)
        {
            if (i < currentPlayers.Count)
            {
                playerSlots[i].SetPlayer(currentPlayers[i]);
            }
            else
            {
                playerSlots[i].SetEmpty();
            }
        }
    }

}
