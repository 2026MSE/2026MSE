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
[System.Serializable]
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
    public List<string> playerIds;
    public string hostId;

    public bool started;

}
[System.Serializable]
public class GameActionRequest
{
    public string roomId;
    public string playerId;
}
[System.Serializable]
public enum YutName
{
    BACK_DO,
    DO,
    GAE,
    GEOL,
    YUT,
    MO
}
[System.Serializable]
public class YutResult
{

    public YutName result;
    public int move;
    public bool extraTurn;

}

[System.Serializable]
public class ThrowResponse
{

    public StickSide?[] sticks;
    public StickSide?[] privateSticks;
    public StickSide?[] publicSticks;

    public YutResult yutResult;
}

[System.Serializable]
public class BoardStatusResponse
{
    public Dictionary<string, List<Piece>> allPieces;

    public bool extraTurn;
    public ThrowResponse throwResult;

    public string currentTurnPlayerId;
    public Scene currentRoom;

    public bool alreadyThrown;
    public bool alreadyMoved;

    public HallState hallState;
}

[System.Serializable]
public class MoveRequest
{
    public string roomId;
    public string playerId;
    public string pieceId;
}

[System.Serializable]
public class MoveListResponse
{
    public List<MoveOption> movablePieces;
}
[System.Serializable]
public class Piece
{
    public string id;
    public string ownerId;
    public int currentPosition;

    public List<Piece> carriedPieces;
}
[System.Serializable]
public class MoveOption
{
    public string pieceId;
    public int currentPosition;
    public int targetPosition;
    public bool finished;
}
[System.Serializable]
public class HallInfoResponse
{
    public HallState state;

    public StickSide?[] publicSticks;
    public StickSide?[] declaredPrivateSticks;

    public string firstChallenger;
    public List<string> queue;
}
[System.Serializable]
public enum StickSide
{
    HEAD,
    TAIL,
    BACK
}
[System.Serializable]
public class DeclareRequest
{

    public string roomId;
    public string playerId;

    public StickSide s1;
    public StickSide s2;
}
[System.Serializable]
public class DeclareResponse
{
    public string message;

    public StickSide?[] declaredPrivateSticks;
    public StickSide?[] publicSticks;

    public HallState state;
}