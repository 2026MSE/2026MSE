using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainGameManager : MonoBehaviour
{
    public static MainGameManager instance { get; private set; }
    public TurnInfo turnInfo { get; set; } = new TurnInfo();
    private Scene? currentScene = Scene.NONE;

    public ClientScene currentClientScene = ClientScene.NONE;
    private ClientScene previousClientScene = ClientScene.NONE;

    public ThrowResponse throwResponse { get; set; } = new ThrowResponse();
    private PlayerManager playerManager;


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

        if (currentClientScene != ClientScene.IN_GAME)
            return;

        if (currentScene != turnInfo.currentTurnPlayerRoom)
        {
            // 현재 턴 플레이어가 아니면서 private room에 있는 경우 씬 이동 X
            if(turnInfo.currentTurnPlayerId != playerManager.this_player.id && turnInfo.currentTurnPlayerRoom == Scene.PRIVATE_ROOM)
            {
                return;
            }

            currentScene = turnInfo.currentTurnPlayerRoom;
            switch (currentScene)
            {
                case Scene.MAIN_HALL:
                    MainHall();
                    break;
                case Scene.PRIVATE_ROOM:
                    PrivateRoom();
                    break;
                case Scene.YUT_ROOM:
                    YutRoom();
                    break;
                case Scene.CHALLENGE_ROOM:
                    ChallengeRoom();
                    break;
                default:
                    break;
            }
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
        if(is_additive)
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
