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

    public String playerId;
    public String name;

    public bool alive = true;

    public String currentEmoticon = "";

    public List<String> inventory;
}
[System.Serializable]
public class TurnInfo
{

    public String currentTurnPlayerId;
    public Scene currentTurnPlayerRoom;

    public List<String> turnOrder;
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
public class GameRoom
{

    public String roomId;
    public List<String> playerIds;
    public bool started = false;

    public TurnInfo turnInfo = new TurnInfo();

    public HallState hallState = HallState.DECLARE;
    public YutName declaredYut;

    public YutResult currentYutResult;
    public bool alreadyThrown = false;

    public StickSide[] sticks = new StickSide[4];
    public StickSide[] privateSticks = new StickSide[2];
    public StickSide[] publicSticks = new StickSide[2];

    public StickSide[] declaredPrivateSticks = new StickSide[2];

    public String firstChallengerId;
    public List<String> challengeQueue;

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

    private YutName result;
    private int move;
    private bool extraTurn;

}
public enum StickSide
{
    HEAD,
    TAIL,
    BACK
}
