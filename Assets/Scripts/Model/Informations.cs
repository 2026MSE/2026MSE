using System;
using System.Collections.Generic;
using UnityEngine;

//
// ===============================
// Client-only Scene State
// ===============================
// 서버에서 오는 상태가 아니라, Unity 클라이언트 화면 전환용으로만 사용
//

[Serializable]
public enum ClientScene
{
    NONE,
    TITLE,
    OPTION,
    EXIT,
    ROOM_CREATE,
    IN_GAME
}

[Serializable]
public enum ClientSceneLoadMode
{
    Single,
    Additive
}

// 필요하면 Unity 내부 화면 구분용으로 유지 가능
// 단, 서버 상태 판단에는 사용하지 않는 것을 권장
[Serializable]
public enum GameViewScene
{
    NONE,
    MAIN_HALL,
    PRIVATE_ROOM,
    YUT_ROOM,
    CHALLENGE_RESULT,
    GAME_RESULT
}

//
// ===============================
// Common API Wrapper
// ===============================
//

[Serializable]
public class ApiResponse<T>
{
    public bool success;
    public string message;
    public string errorCode;
    public T data;
}

//
// ===============================
// Player / Room
// ===============================
//

[Serializable]
public class Player
{
    public string name;
    public string profileUrl;
    public string id;
}

[Serializable]
public class PlayerInfo
{
    public string playerId;
    public string name;
    public string currentEmoticon = "";
    public string profileUrl = "";
    public List<string> inventory = new List<string>();
}

[Serializable]
public class RoomInfo
{
    public string roomId;
    public List<string> playerIds;
    public string hostId;
    public bool started;
}

//
// ===============================
// Turn / Phase
// ===============================
//

[Serializable]
public class TurnInfo
{
    public string currentTurnPlayerId;
    public List<string> turnOrder;
    public int currentTurnIndex;
}

[Serializable]
public enum TurnPhase
{
    WAITING,
    PRIVATE_THROW,
    MAIN_HALL_DECLARE,
    MAIN_HALL_CHALLENGE,
    CATCH_BONUS_THROW,
    YUT_MOVE,
    YUT_MOVE_DONE,
    TURN_END,
    GAME_OVER
}

//
// ===============================
// Yut / Stick
// ===============================
//

[Serializable]
public enum StickSide
{
    HEAD,
    TAIL,
    BACK
}

[Serializable]
public enum YutName
{
    BACK_DO,
    DO,
    GAE,
    GEOL,
    YUT,
    MO
}

[Serializable]
public class YutResult
{
    public YutName result;
    public int move;
    public bool extraTurn;
}

[Serializable]
public class ThrowResponse
{
    public StickSide?[] sticks;
    public StickSide?[] privateSticks;
    public StickSide?[] publicSticks;
    public YutResult yutResult;
}

//
// ===============================
// Game State Polling
// ===============================
// /game/state 응답용
//

[Serializable]
public class GameStateResponse
{
    public List<GameLog> logs;

    public RoomInfo roomInfo;
    public TurnInfo turnInfo;
    public TurnPhase turnPhase;

    public BoardStatusResponse boardStatus;
    public List<PlayerInfo> players;

    public StickSide?[] privateSticks;
    public StickSide?[] publicSticks;
    public StickSide?[] declaredPrivateSticks;

    public string firstChallenger;
    public List<string> challengeQueue;
    public Dictionary<string, bool> challengeVotes;

    public YutResult currentYutResult;
    public List<YutResult> pendingYutResults;

    public JudgeResponse lastJudgeResponse;

    public long challengeDeadlineMillis;
    public long serverTimeMillis;

    public string winnerId;
}

[Serializable]
public class GameLog
{
    public string type;
    public string message;
    public long timeMillis;
}

//
// ===============================
// Board / Piece
// ===============================
//

[Serializable]
public class BoardStatusResponse
{
    public Dictionary<string, List<Piece>> allPieces;
}

[Serializable]
public class Piece
{
    public string id;
    public string ownerId;
    public int currentPosition;
    public List<Piece> carriedPieces = new List<Piece>();
    public string carriedByPieceId;
}

//
// ===============================
// Move
// ===============================
//

[Serializable]
public class MoveRequest
{
    public string roomId;
    public string playerId;
    public string pieceId;

    // 서버 신버전에서 추가됨
    // pendingYutResults 중 몇 번째 윷 결과를 사용할지 선택
    public int yutResultIndex;
}

[Serializable]
public class MoveListResponse
{
    public List<MoveGroup> moveGroups;
}

[Serializable]
public class MoveGroup
{
    public int yutResultIndex;
    public YutName yutName;
    public int move;
    public List<MoveOption> movablePieces;
}

[Serializable]
public class MoveOption
{
    public string pieceId;
    public int currentPosition;
    public int targetPosition;
    public bool finished;
    public MoveType moveType;
}

[Serializable]
public enum MoveType
{
    NORMAL,
    CATCH,
    FINISH
}

[Serializable]
public class MoveResultResponse
{
    public string pieceId;
    public int fromPosition;
    public int toPosition;
    public MoveType moveType;
    public bool extraTurn;
    public bool gameOver;
    public string winnerId;
}

//
// ===============================
// Hall / Declare / Challenge
// ===============================
//

[Serializable]
public class DeclareRequest
{
    public string roomId;
    public string playerId;
    public StickSide s1;
    public StickSide s2;
}

[Serializable]
public class ChallengeVoteRequest
{
    public string roomId;
    public string playerId;
    public bool challenge;
}

[Serializable]
public class JudgeResponse
{
    public JudgeResult judgeResult;

    public string challengerId;
    public string turnPlayerId;

    public StickSide?[] actualPrivateSticks;
    public StickSide?[] declaredPrivateSticks;
    public StickSide?[] publicSticks;

    public YutResult actualResult;

    public bool rewardChanceCard;
    public string rewardCard;

    public string penaltyType;
    public bool penaltyApplied;
    public string penaltyPieceId;
}

[Serializable]
public enum JudgeResult
{
    CHALLENGE_SUCCESS,
    CHALLENGE_FAIL
}

//
// ===============================
// Request DTO
// ===============================
//

[Serializable]
public class GameActionRequest
{
    public string roomId;
    public string playerId;
}