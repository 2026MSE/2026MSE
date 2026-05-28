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

/*
 * 새로운 DTO
 */


public class GameStateResponse
{

    public List<GameLog> logs;

    public RoomInfo roomInfo;
    public TurnInfo turnInfo;
    public TurnPhase turnPhase;
    public BoardStatusResponse boardStatus;
    public List<PlayerInfo> players;

    public StickSide[] privateSticks;
    public StickSide[] publicSticks;
    public StickSide[] declaredPrivateSticks;
    public string firstChallenger;
    public List<string> challengeQueue;
    public Dictionary<string, bool> challengeVotes;

    public YutResult currentYutResult;
    public List<YutResult> pendingYutResults;

    public JudgeResponse lastJudgeResponse;

    public long challengeDeadlineMillis;
    public long serverTimeMillis;

    public string winnerId;
    public List<PlayerEffectInfo> activeEffects;
}

public class GameLog
{

    public string type;
    public string message;
    public long timeMillis;
}
public enum TurnPhase
{

    WAITING, // 방 대기 중

    PRIVATE_THROW, // 턴 플레이어가 Private Room에서 윷 던지는 단계

    MAIN_HALL_DECLARE, // Main Hall에서 결과를 선언하는 단계
    MAIN_HALL_CHALLENGE, // 다른 플레이어들이 챌린지할 수 있는 단계

    CHALLENGE_RESULT, // 챌린지 결과 공개 및 처리 단계

    CATCH_BONUS_THROW,

    YUT_MOVE, // 윷판에서 말을 이동하는 단계
    YUT_MOVE_DONE, // 말을 이동한 후 추가 행동이 필요한지 판단하는 단계

    TURN_END, // 턴 종료 처리 단계

    GAME_OVER // 게임 종료

}


// 챌린지 결과 enum
public enum JudgeResult
{
    CHALLENGE_SUCCESS,
    CHALLENGE_FAIL
}
public class JudgeResponse
{

    public JudgeResult judgeResult;

    public string challengerId;
    public string turnPlayerId;

    public StickSide[] actualPrivateSticks;
    public StickSide[] declaredPrivateSticks;
    public StickSide[] publicSticks;

    public YutResult actualResult;


    public bool rewardChanceCard;
    public string rewardCard;


    public string penaltyType;


    public bool penaltyApplied;


    public string penaltyPieceId;
}

public class PlayerEffectInfo
{

    public EffectType type;
    public string targetPlayerId;
    public string sourcePlayerId;
    public int remainingTurns;
    public int value;
}
public enum EffectType
{

    // 패널티 계열
    ONE_PRIVATE_STICK,   // 다음 턴에 private stick 1개만 사용
    SKIP_TURN,           // 다음 턴 스킵
    MOVE_MINUS_ONE,      // 다음 이동 칸 수 -1

    // 보상/버프 계열
    EXTRA_THROW,         // 윷 한 번 더 던지기
    MOVE_PLUS_ONE,       // 다음 이동 칸 수 +1
    SHIELD,              // 패널티 1회 방어

    // 제한 계열
    NO_CHALLENGE,        // 다음 턴 챌린지 불가
    NO_CHANCE_CARD       // 다음 보상 획득 불가
}