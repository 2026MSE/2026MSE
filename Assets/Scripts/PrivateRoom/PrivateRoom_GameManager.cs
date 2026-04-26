using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PrivateRoomState
{
    init,
    idle1,
    throwing,
    idle2,
    chance_select,
    exit
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


    [SerializeField]
    PrivateRoomState state;

    bool isStateChanged = false;
    void Start()
    {
        state = PrivateRoomState.init;
    }

    void Update()
    {
        if (isStateChanged)
        {
            // state에 따라 코루틴이든 일반 함수로든 실행
            switch (state)
            {
                case PrivateRoomState.init:
                    break;
                case PrivateRoomState.idle1:
                    break;
                case PrivateRoomState.throwing:
                    break;
                case PrivateRoomState.idle2:
                    break;
                case PrivateRoomState.exit:
                    break;
            }
        }
    }

    public void SetState(PrivateRoomState newState)
    {
        state = newState;
        isStateChanged = true;
    }

    void Init()
    {
        /*
         * 룸 진입 애니메이션 실행시킨 뒤
         * state 변경
         */
    }

    void Idle1()
    {
        PrivateRoom_UIManager.instance.EnterIdle1();
    }

    void Throwing()
    {
        /*
         * 던지기 애니메이션 실행시키고
         * state 변경
         */
    }

    void Idle2()
    {
        /*
         * 찬스카드와 나가기 버튼 띄우기
         * 나가기 버튼을 누르면 Exit state로 변경
         * 찬스카드를 누르면 chance_select state로 변경
         */
    }

    void ChanceSelect()
    {
        /*
         * 현재 가지고 있는 찬스카드 보여주기
         * 뒤로가기 버튼을 누르면 idle2 state로 변경
         */
    }
    void Exit()
    {
        /*
         * 룸 나가기 애니메이션 실행시키고
         * 씬 전환
         */
    }
}

