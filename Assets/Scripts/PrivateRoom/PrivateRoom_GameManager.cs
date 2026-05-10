using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public enum PrivateRoomState
{
    None,
    Init,
    Idle1,
    Throwing,
    Idle2,
    Chance_select,
    Exit
}



public class PrivateRoom_GameManager : MonoBehaviour
{
    public static PrivateRoom_GameManager instance { get; private set; }
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public StickSide[] yutResult = new StickSide[4];

    [SerializeField]
    PrivateRoomState state;

    void Start()
    {
        state = PrivateRoomState.Init;
    }

    void Update()
    {
        // state에 따라 코루틴이든 일반 함수로든 실행
        switch (state)
        {
            case PrivateRoomState.Init:
                Init();
                break;
            case PrivateRoomState.Idle1:
                Idle1();
                break;
            case PrivateRoomState.Throwing:
                Throwing();
                break;
            case PrivateRoomState.Idle2:
                Idle2();
                break;
            case PrivateRoomState.Chance_select:
                ChanceSelect();
                break;
            case PrivateRoomState.Exit:
                Exit();
                break;
            default:
                break;
        }
    }

    public void SetState(PrivateRoomState newState)
    {
        state = newState;
    }

    void Init()
    {
        Debug.Log("PrivateRoom_GameManager: Init");
        /*
         * 룸 진입 애니메이션 실행시킨 뒤
         * state 변경
         */
        InitAnimation().Forget();
        state = PrivateRoomState.None;
    }

    void Idle1()
    {
        Debug.Log("PrivateRoom_GameManager: Idle1");
        PrivateRoom_UIManager.instance.EnterIdle1();
        state = PrivateRoomState.None;
    }

    void Throwing()
    {
        Debug.Log("PrivateRoom_GameManager: Throwing");
        GetYutResult().Forget();
        /*
         * 던지기 애니메이션 실행시키고
         * state 변경
         */
        state = PrivateRoomState.None;
    }

    void Idle2()
    {
        Debug.Log("PrivateRoom_GameManager: Idle2");
        /*
         * 찬스카드와 나가기 버튼 띄우기
         * 나가기 버튼을 누르면 Exit state로 변경
         * 찬스카드를 누르면 chance_select state로 변경
         */
        PrivateRoom_UIManager.instance.EnterIdle2();
        state = PrivateRoomState.None;
    }

    void ChanceSelect()
    {
        Debug.Log("PrivateRoom_GameManager: ChanceSelect");
        /*
         * 현재 가지고 있는 찬스카드 보여주기
         * 뒤로가기 버튼을 누르면 idle2 state로 변경
         */
        PrivateRoom_UIManager.instance.EnterChanceSelect();
        state = PrivateRoomState.None;
    }
    void Exit()
    {
        Debug.Log("PrivateRoom_GameManager: Exit");
        /*
         * 룸 나가기 애니메이션 실행시키고
         * 씬 전환
         */

        if (ServerManager.instance.isUsingServer)
            ServerManager.instance.PrivateExitRequest().Forget();
        else
            MainGameManager.instance.turnInfo = new TurnInfo { currentTurnPlayerRoom = Scene.MAIN_HALL }; // 테스트용 더미 데이터
        state = PrivateRoomState.None;
    }
    public async UniTaskVoid GetYutResult()
    {
        if(ServerManager.instance.isUsingServer)
            await ServerManager.instance.YutRequest();
        else
            yutResult = new StickSide[] { StickSide.HEAD, StickSide.TAIL, StickSide.HEAD, StickSide.TAIL }; // 테스트용 더미 데이터

        await YutAnimation();
    }

    public async UniTaskVoid InitAnimation()
    {
        /*
         * 룸 진입 애니메이션 실행
         * 애니메이션이 끝나면 state 변경
         */
        await UniTask.Delay(2000); // 테스트용 딜레이

        state = PrivateRoomState.Idle1;
    }

    public async UniTask YutAnimation()
    {
        await UniTask.Delay(2000); // 테스트용 딜레이

        await UniTask.WaitUntil(() => MainGameManager.instance.throwResponse != null);
        PrivateRoom_UIManager.instance.ShowYut();
        state = PrivateRoomState.Idle2;
    }
}

