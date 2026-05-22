using System.Collections.Generic;
using UnityEngine;

public class MainGameManager : MonoBehaviour
{
    public static MainGameManager instance;

    [Header("Server State")]
    public GameStateResponse gameState;

    [Header("Scene Load")]
    public string nextSceneName = "";
    public ClientSceneLoadMode nextSceneLoadMode = ClientSceneLoadMode.Single;

    public TurnInfo turnInfo;
    public TurnPhase turnPhase;
    public BoardStatusResponse boardStatusResponse;

    public ThrowResponse throwResponse;
    public MoveListResponse moveListResponse;
    public JudgeResponse lastJudgeResponse;

    [Header("Client Scene State")]
    public ClientScene currentClientScene = ClientScene.NONE;
    public GameViewScene currentGameViewScene = GameViewScene.NONE;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[MainGameManager] instance created and marked DontDestroyOnLoad.");
        }
        else
        {
            Debug.LogWarning("[MainGameManager] Duplicate instance destroyed.");
            Destroy(gameObject);
        }
    }

    public void ApplyGameState(GameStateResponse state)
    {
        if (state == null)
        {
            Debug.LogWarning("[MainGameManager] GameStateResponse is null");
            return;
        }

        // 1. 서버 상태 저장
        gameState = state;

        turnInfo = state.turnInfo;
        turnPhase = state.turnPhase;
        boardStatusResponse = state.boardStatus;
        lastJudgeResponse = state.lastJudgeResponse;

        // 2. PlayerManager 쪽 정보도 갱신
        if (PlayerManager.instance != null)
        {
            PlayerManager.instance.currentRoom = state.roomInfo;
            PlayerManager.instance.playerList = state.players;
        }

        // 3. TurnPhase 기준으로 Unity 내부 화면 상태 변경
        ApplySceneByTurnPhase();
    }

    public void ApplySceneByTurnPhase()
    {
        if (gameState == null || turnInfo == null)
        {
            Debug.LogWarning("[MainGameManager] Cannot apply scene. gameState or turnInfo is null");
            return;
        }

        bool isMyTurn = IsMyTurn();

        switch (turnPhase)
        {
            case TurnPhase.WAITING:
                currentClientScene = ClientScene.ROOM_CREATE;
                currentGameViewScene = GameViewScene.NONE;
                break;

            case TurnPhase.PRIVATE_THROW:
                currentClientScene = ClientScene.IN_GAME;

                if (isMyTurn)
                {
                    currentGameViewScene = GameViewScene.PRIVATE_ROOM;
                }
                else
                {
                    currentGameViewScene = GameViewScene.MAIN_HALL;
                }
                break;

            case TurnPhase.MAIN_HALL_DECLARE:
                currentClientScene = ClientScene.IN_GAME;
                currentGameViewScene = GameViewScene.MAIN_HALL;
                break;

            case TurnPhase.MAIN_HALL_CHALLENGE:
                currentClientScene = ClientScene.IN_GAME;
                currentGameViewScene = GameViewScene.MAIN_HALL;
                break;

            case TurnPhase.CATCH_BONUS_THROW:
                currentClientScene = ClientScene.IN_GAME;

                if (isMyTurn)
                {
                    currentGameViewScene = GameViewScene.PRIVATE_ROOM;
                }
                else
                {
                    currentGameViewScene = GameViewScene.MAIN_HALL;
                }
                break;

            case TurnPhase.YUT_MOVE:
                currentClientScene = ClientScene.IN_GAME;

                if (isMyTurn)
                {
                    currentGameViewScene = GameViewScene.YUT_ROOM;
                }
                else
                {
                    currentGameViewScene = GameViewScene.MAIN_HALL;
                }
                break;

            case TurnPhase.YUT_MOVE_DONE:
                currentClientScene = ClientScene.IN_GAME;

                if (isMyTurn)
                {
                    currentGameViewScene = GameViewScene.YUT_ROOM;
                }
                else
                {
                    currentGameViewScene = GameViewScene.MAIN_HALL;
                }
                break;

            case TurnPhase.TURN_END:
                currentClientScene = ClientScene.IN_GAME;
                currentGameViewScene = GameViewScene.MAIN_HALL;
                break;

            case TurnPhase.GAME_OVER:
                currentClientScene = ClientScene.IN_GAME;
                currentGameViewScene = GameViewScene.GAME_RESULT;
                break;
        }
    }

    public bool IsMyTurn()
    {
        if (turnInfo == null)
            return false;

        if (PlayerManager.instance == null)
            return false;

        if (PlayerManager.instance.this_player == null)
            return false;

        return turnInfo.currentTurnPlayerId == PlayerManager.instance.this_player.id;
    }

    public bool IsPhase(TurnPhase phase)
    {
        return turnPhase == phase;
    }

    public bool IsMyTurnAndPhase(TurnPhase phase)
    {
        return IsMyTurn() && IsPhase(phase);
    }

    public bool CanThrowYut()
    {
        return IsMyTurn() &&
               (turnPhase == TurnPhase.PRIVATE_THROW ||
                turnPhase == TurnPhase.CATCH_BONUS_THROW);
    }

    public bool CanDeclare()
    {
        return IsMyTurn() &&
               turnPhase == TurnPhase.MAIN_HALL_DECLARE;
    }

    public bool CanChallengeVote()
    {
        return !IsMyTurn() &&
               turnPhase == TurnPhase.MAIN_HALL_CHALLENGE;
    }

    public bool CanMovePiece()
    {
        return IsMyTurn() &&
               turnPhase == TurnPhase.YUT_MOVE;
    }

    public bool CanEndTurn()
    {
        return IsMyTurn() &&
               turnPhase == TurnPhase.YUT_MOVE_DONE;
    }

    public bool IsGameOver()
    {
        return turnPhase == TurnPhase.GAME_OVER;
    }
    public string GetGotoSceneName()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            string sceneName = nextSceneName;
            nextSceneName = "";
            return sceneName;
        }

        switch (currentClientScene)
        {
            case ClientScene.TITLE:
                return "Title";

            case ClientScene.OPTION:
                return "Option";

            case ClientScene.ROOM_CREATE:
                return "RoomCreate";

            case ClientScene.IN_GAME:
                return GetGameViewSceneName();

            default:
                return "MainHall";
        }
    }

    private string GetGameViewSceneName()
    {
        switch (currentGameViewScene)
        {
            case GameViewScene.MAIN_HALL:
                return "MainHall";

            case GameViewScene.PRIVATE_ROOM:
                return "PrivateRoom";

            case GameViewScene.YUT_ROOM:
                return "YutBoard";

            case GameViewScene.GAME_RESULT:
                return "GameResult";

            default:
                return "MainHall";
        }
    }
}