using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    bool isInLobby = true;
    ServerManager server_manager;
    PlayerManager player_manager;
    private void Start()
    {
        server_manager = ServerManager.instance;
        player_manager = PlayerManager.instance;
    }
    public void Update()
    {
        if (isInLobby && PlayerManager.instance.currentRoom != null)
        {
            if (PlayerManager.instance.currentRoom.started)
            {
                MainGameManager.instance.currentClientScene = ClientScene.IN_GAME;
                isInLobby = false;
            }
        }
    }
    public void RoomCreate()
    {
        if(player_manager.this_player != null)
        {
            GameActionRequest request = new GameActionRequest()
            {
                playerId = player_manager.this_player.id,
                roomId = null
            };
            server_manager.RoomRequest(request, ServerManager.RoomActionType.Create).Forget();
        }
    }

    public void RoomJoin(string roomId)
    {
        if (player_manager.this_player != null)
        {
            GameActionRequest request = new GameActionRequest()
            {
                playerId = player_manager.this_player.id,
                roomId = roomId
            };
            server_manager.RoomRequest(request, ServerManager.RoomActionType.Join).Forget();
        }
    }

    public void GameStart()
    {
        if (player_manager.this_player != null)
        {
            GameActionRequest request = new GameActionRequest()
            {
                playerId = player_manager.this_player.id,
                roomId = player_manager.currentRoom.roomId
            };
            server_manager.RoomRequest(request, ServerManager.RoomActionType.Start).Forget();
            server_manager.GameStart();
        }
    }
}
