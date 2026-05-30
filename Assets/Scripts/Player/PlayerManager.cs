using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance { get; private set; }
    public Player this_player { get; set; } = null;

    public List<Player> debug_players = new List<Player>();

    public bool isMyTurn()
    {
        if (MainGameManager.instance.game_stat.turnInfo.currentTurnPlayerId == this_player.id)
        {
            return true;
        }
        return false;
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
        ServerManager.instance.PlayerRequest(name, style).Forget();
    }

}
