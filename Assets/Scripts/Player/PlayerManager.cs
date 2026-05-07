using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance { get; private set; }
    public Player this_player { get; set; }
    public RoomInfo currentRoom { get; set; } = null;
    public List<PlayerInfo> playerList { get; set; }

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
