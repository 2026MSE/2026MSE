using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ChanceCard
{
    EXTRA_THROW,
    MOVE_PLUS_ONE,
    SHIELD,
    FORCE_BACK
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

    public List<string> turnOrder;
    public int currentTurnIndex;

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
public class MoveRequest
{
    public string roomId;
    public string playerId;
    public string pieceId;
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


// 새로운 DTO

[System.Serializable]
public class BoardStatusResponse
{
    public Dictionary<string, List<PieceInfo>> allPieces;
}

[System.Serializable]
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

[System.Serializable]
public class GameLog
{

    public string type;
    public string message;
    public long timeMillis;
}
public enum TurnPhase
{

    WAITING,

    PRIVATE_THROW,

    MAIN_HALL_DECLARE,
    MAIN_HALL_CHALLENGE,

    CHALLENGE_RESULT,

    CATCH_BONUS_THROW,

    YUT_MOVE,
    YUT_MOVE_DONE,

    TURN_END,

    GAME_OVER
}
public enum JudgeResult
{
    CHALLENGE_SUCCESS,
    CHALLENGE_FAIL
}

[System.Serializable]
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

[System.Serializable]
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

[System.Serializable]
public class PieceInfo
{
    public string pieceId;
    public string ownerId;
    public int currentPosition;
    public string carriedByPieceId;
    public List<string> carriedPieceIds;
}

[System.Serializable]
public class MoveGroup
{
    public int yutResultIndex;
    public YutName yutName;
    public int move;
    public List<MoveOption> movablePieces;
}

[System.Serializable]
public class MoveListResponse
{
    public List<MoveGroup> moveGroups; // 변경됨
}

public enum MoveType
{
    NORMAL,    // 일반 이동 (빈 칸으로 이동)
    PIGGYBACK, // 업기 (내 말이 있는 칸으로 이동하여 합쳐짐)
    CATCH,     // 잡기 (상대방 말이 있는 칸으로 이동하여 상대 말을 대기석으로 보냄)
    FINISH     // 완주 (골인 지점을 통과함)
}

[System.Serializable]
public class MoveOption
{
    public string pieceId;
    public int currentPosition;
    public int targetPosition;
    public bool finished;

    public MoveType moveType;
}

[System.Serializable]
public class Player
{

    public string name;
    public string profileUrl;
    public string id;

    public List<ChanceCard> inventory;
}