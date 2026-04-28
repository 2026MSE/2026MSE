using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainGameManager : MonoBehaviour
{
    public static MainGameManager instance { get; private set; }
    private TurnInfo turnInfo;

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
        turnInfo = new TurnInfo();
        SceneManager.LoadScene("MainGameUI", LoadSceneMode.Additive);
    }

    void Update()
    {
        // 턴이 바뀌었을 때만 실행함. 평소에는 None으로 유지
        switch(turnInfo.currentRoom)
        {
            case Rooms.Title:
                Title();
                break;
            case Rooms.Option:
                Option();
                break;
            case Rooms.Exit:
                Exit();
                break;
            case Rooms.MainHall:
                MainHall();
                break;
            case Rooms.PrivateRoom:
                PrivateRoom();
                break;
            case Rooms.YutRoom:
                YutRoom();
                break;
            case Rooms.ChallengeRoom:
                ChallengeRoom();
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

    public void SetTurnInfo(TurnInfo newTurnInfo)
    {
        turnInfo = newTurnInfo;
    }

    public void LoadingScene()
    {
        turnInfo.currentRoom = Rooms.None; // 씬 전환이 완료되면 None으로 초기화
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
    void MainHall()
    {
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
