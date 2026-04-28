using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ChanceCards
{
    None,
    OneMoreTurn,
    MoveForward,
    MoveBackward,
    ChangePrivateYut,
    ChangePublicYut,
    OneMoreThrow
}

public enum Rooms
{
    None,
    Title,
    Option,
    Exit,
    MainHall,
    PrivateRoom,
    YutRoom,
    ChallengeRoom
}

public class PlayerInfo
{
    public int playerIcon;
    public string playerName;
    public List<ChanceCards> inventory;
    public int using_emoji;
    public bool is_alive;
}

public class TurnInfo
{
    public int turnNumber;
    public int currentPlayer;
    public Rooms currentRoom;
}
