using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ChanceCards
{
    
}

public enum Scene
{
    NONE,
    TITLE,
    OPTION,
    EXIT,
    MAIN_HALL,
    PRIVATE_ROOM,
    YUT_ROOM,
    CHALLENGE_ROOM
}

public enum ClientScene
{
    NONE,
    TITLE,
    OPTION,
    EXIT,
    ROOM_CREATE,
    IN_GAME
}

[System.Serializable]
public class PlayerInfo
{

    public string playerId;
    public string name;

    public string currentEmoticon = "";
    public string profileUrl;

    public List<string> inventory;
}
[System.Serializable]
public class TurnInfo
{

    public string currentTurnPlayerId;
    public Scene? currentTurnPlayerRoom = Scene.MAIN_HALL;

    public List<string> turnOrder;
    public int currentTurnIndex;

}
[System.Serializable]
public class Player
{
    public string name;
    public string profileUrl;
    public string id;

}
public enum HallState
{
    DECLARE,
    IDLE1,
    CHALLENGE,
    IDLE2,

}
[System.Serializable]
public class ApiResponse<T>
{

    public bool success;
    public string message;
    public T data;
}

[System.Serializable]
public class RoomInfo
{

    public string roomId;
    public List<string> playerIds = new List<string>();
    public string hostId;

    public bool started;

}
[System.Serializable]
public class GameActionRequest
{
    public string roomId;
    public string playerId;
}
public enum YutName
{
    BACK_DO,
    DO,
    GAE,
    GEOL,
    YUT,
    MO
}
public class YutResult
{

    public YutName result;
    public int move;
    public bool extraTurn;

}
public enum StickSide
{
    HEAD,
    TAIL,
    BACK
}
public class ThrowResponse
{

    public StickSide[] sticks;
    public StickSide[] privateSticks;
    public StickSide[] publicSticks;

    public YutResult yutResult;
}