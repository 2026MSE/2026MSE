using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum LobbyState
{
    NONE,
    PLAYERMAKING,
    SELECT,
    ROOM,
    START
}


public class LobbyManager : MonoBehaviour
{
    [Header("UI Objects")]
    public GameObject playerMakingUI;
    public GameObject roomSelectUI;
    public GameObject inRoomUI;
    public TMP_InputField roomIdInputField;

    bool isInLobby = true;
    ServerManager server_manager;
    PlayerManager player_manager;
    LobbyState lobby_state = LobbyState.NONE;
    
    private void Start()
    {
    }

    private void OnEnable()
    {
        server_manager = ServerManager.instance;
        player_manager = PlayerManager.instance;
    }
    public void Update()
    {
        switch(lobby_state)
        {
            case LobbyState.NONE:
                lobby_state = LobbyState.PLAYERMAKING;
                break;
            case LobbyState.PLAYERMAKING:
                if (player_manager.this_player != null)
                {
                    lobby_state = LobbyState.SELECT;
                }
                else
                {
                    PlayerMakingUI();
                }
                break;
            case LobbyState.SELECT:
                if (player_manager.currentRoom != null)
                {
                    lobby_state = LobbyState.ROOM;
                }
                else
                {
                    SelectUI();
                }
                break;
            case LobbyState.ROOM:
                RoomUI();
                break;
        }

        if (isInLobby && PlayerManager.instance.currentRoom != null)
        {
            if (PlayerManager.instance.currentRoom.started)
            {
                MainGameManager.instance.currentClientScene = ClientScene.IN_GAME;
                ServerManager.instance.GameStart();
                ServerManager.instance.RoomStop();
                isInLobby = false;
            }
        }
    }
    void PlayerMakingUI()
    {
        playerMakingUI.SetActive(true);
        roomSelectUI.SetActive(false);
        inRoomUI.SetActive(false);
    }

    void SelectUI()
    {
        playerMakingUI.SetActive(false);
        roomSelectUI.SetActive(true);
        inRoomUI.SetActive(false);
    }

    void RoomUI()
    {
        playerMakingUI.SetActive(false);
        roomSelectUI.SetActive(false);
        inRoomUI.SetActive(true);
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

    public void RoomJoin()
    {
        string roomId = roomIdInputField.text;

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
        }
    }
}
