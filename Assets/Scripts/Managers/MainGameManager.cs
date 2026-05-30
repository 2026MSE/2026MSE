using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainGameManager : MonoBehaviour
{
    public static MainGameManager instance { get; private set; }
    public ClientScene currentClientScene = ClientScene.NONE;
    private ClientScene previousClientScene = ClientScene.NONE;

    public GameStateResponse game_stat = new GameStateResponse();
    public ThrowResponse throwResponse { get; set; } = new ThrowResponse();
    public BoardStatusResponse boardStatusResponse { get; set; } = new BoardStatusResponse();
    public MoveListResponse moveListResponse { get; set; } = new MoveListResponse();
    private PlayerManager playerManager;

    private TurnPhase now_pos_phase;

    public string gotoSceneName = "MainHall";

    private void Awake()
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
    void Start()
    {
        playerManager = PlayerManager.instance;
        SceneManager.LoadScene("MainGameUI", LoadSceneMode.Additive);
        //디버깅용
        //ServerManager.instance.TextureRequest().Forget();
    }

    void Update()
    {
        if(previousClientScene != currentClientScene)
        {
            previousClientScene = currentClientScene;
            switch (currentClientScene)
            {
                case ClientScene.TITLE:
                    Title();
                    return;
                case ClientScene.OPTION:
                    Option();
                    return;
                case ClientScene.EXIT:
                    Exit();
                    return;
                case ClientScene.ROOM_CREATE:
                    RoomCreate();
                    break;
                case ClientScene.IN_GAME:
                    break;
            }
        }

        
        // 예외처리
        if (currentClientScene != ClientScene.IN_GAME
            || (!playerManager.isMyTurn() && game_stat.turnPhase == TurnPhase.PRIVATE_THROW)
            || game_stat.turnPhase == now_pos_phase)
            return;


        now_pos_phase = game_stat.turnPhase;
        switch (now_pos_phase)
        {
            case TurnPhase.PRIVATE_THROW:
                PrivateRoom();
                break;
            case TurnPhase.MAIN_HALL_DECLARE:
                MainHall();
                break;
            case TurnPhase.MAIN_HALL_CHALLENGE:
            case TurnPhase.CHALLENGE_RESULT:
                ChallengeRoom();
                break;
            case TurnPhase.CATCH_BONUS_THROW:
            case TurnPhase.YUT_MOVE:
            case TurnPhase.YUT_MOVE_DONE:
                YutRoom();
                break;
            case TurnPhase.TURN_END:
                break;
            case TurnPhase.GAME_OVER:
                Exit();
                break;
            default:
                break;
        }

        
    }
    public string GetGotoSceneName()
    {
        string tmp = gotoSceneName;
        gotoSceneName = null;
        return tmp;
    }

    public void LoadingScene(bool is_additive = false)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (is_additive)
            SceneManager.LoadScene("LoadingScene", LoadSceneMode.Additive);
        else
            SceneManager.LoadScene("LoadingScene");
    }

    void Title()
    {
        gotoSceneName = "MainTitle";
    }
    void Option()
    {
        gotoSceneName = "Option";
        LoadingScene();
    }
    void RoomCreate()
    {
        SceneManager.LoadScene("RoomCreate", LoadSceneMode.Additive);
    }
    void MainHall()
    {
        throwResponse = null;
        gotoSceneName = "MainHall";
        LoadingScene();
    }
    void PrivateRoom()
    {
        gotoSceneName = "PrivateRoom";
        LoadingScene();
    }
    void YutRoom()
    {
        gotoSceneName = "YutRoom";
        LoadingScene();
    }
    void ChallengeRoom()
    {
        gotoSceneName = "ChallengeRoom";
        LoadingScene();
    }

    void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // 에디터 모드 종료
#else
    Application.Quit(); // 빌드된 앱 종료
#endif
    }
}
