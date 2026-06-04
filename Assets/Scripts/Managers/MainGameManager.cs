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
        //SceneManager.LoadScene("MainGameUI", LoadSceneMode.Additive);
        //디버깅용
        //ServerManager.instance.TextureRequest().Forget();
    }

    void Update()
    {
        if (SceneManager.GetSceneByName("LoadingScene").IsValid())
        {
            return;
        }

        if (previousClientScene != currentClientScene)
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
                    return;
                case ClientScene.IN_GAME:
                    break;
                default:
                    return;
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
            case TurnPhase.YUT_MOVE:
                YutRoom();
                break;
            case TurnPhase.TURN_END:
                MainHall();
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
        if(SceneManager.GetSceneByName(gotoSceneName).IsValid())
        {
            return;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (is_additive)
            SceneManager.LoadScene(gotoSceneName, LoadSceneMode.Additive);
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
        LoadingScene(true);
    }
    void RoomCreate()
    {
        gotoSceneName = "RoomCreate";
        LoadingScene(true);
    }
    public void MainHall()
    {
        gotoSceneName = "MainHall";
        LoadingScene();
    }
    void PrivateRoom()
    {
        if(!SceneManager.GetSceneByName("MainHall").IsValid())
        {
            gotoSceneName = "MainHall";
            LoadingScene();
            return;
        }
        gotoSceneName = "PrivateRoom";
        LoadingScene(true);
    }
    void YutRoom()
    {
        gotoSceneName = "YutRoom";
        LoadingScene();
    }
    //void ChallengeRoom()
    //{
    //    gotoSceneName = "ChallengeRoom";
    //    LoadingScene(true);
    //}

    void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // 에디터 모드 종료
#else
    Application.Quit(); // 빌드된 앱 종료
#endif
    }
}
