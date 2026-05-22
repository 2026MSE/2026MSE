using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance { get; private set; }
    public Player this_player { get; set; } = null;
    public RoomInfo currentRoom { get; set; } = null;
    public List<PlayerInfo> playerList { get; set; } = new List<PlayerInfo>();

    public List<Player> debug_players = new List<Player>();

    public bool isMyTurn()
    {
        if (MainGameManager.instance == null)
            return false;

        if (MainGameManager.instance.turnInfo == null)
            return false;

        if (this_player == null)
            return false;

        if (string.IsNullOrEmpty(this_player.id))
            return false;

        return MainGameManager.instance.turnInfo.currentTurnPlayerId == this_player.id;
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public void createPlayer(string name, string style = "adventurer")
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            Debug.LogWarning("[PlayerManager] player name is empty.");
            return;
        }

        if (ServerManager.instance == null)
        {
            Debug.LogWarning("[PlayerManager] ServerManager.instance is null.");
            return;
        }

        ServerManager.instance.PlayerRequest(name.Trim(), style).Forget();
    }

}
