using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PrivateRoom_UIManager : MonoBehaviour
{
    public static PrivateRoom_UIManager instance { get; private set; }

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

    // UI 요소 연결
    public Button throwButton;
    public Button chanceSelectButton;
    public Button exitButton;


    void Start()
    {
        // 버튼 클릭 이벤트 연결
        throwButton.onClick.AddListener(OnThrowButtonClicked);
        chanceSelectButton.onClick.AddListener(OnChanceSelectButtonClicked);
        exitButton.onClick.AddListener(OnExitButtonClicked);
    }

    public void EnterIdle1()
    {
        throwButton.gameObject.SetActive(true);
        chanceSelectButton.gameObject.SetActive(false);
        exitButton.gameObject.SetActive(false);
    }

    public void EnterIdle2()
    {
        throwButton.gameObject.SetActive(false);
        chanceSelectButton.gameObject.SetActive(true);
        exitButton.gameObject.SetActive(true);
    }

    void OnThrowButtonClicked()
    {
        // 던지기 버튼 클릭 시 게임 매니저에 상태 변경 요청
        PrivateRoom_GameManager.instance.SetState(PrivateRoomState.throwing);
    }

    void OnChanceSelectButtonClicked()
    {
        // 찬스 선택 버튼 클릭 시 게임 매니저에 상태 변경 요청
        PrivateRoom_GameManager.instance.SetState(PrivateRoomState.chance_select);
    }

    void OnExitButtonClicked()
    {
        // 나가기 버튼 클릭 시 게임 매니저에 상태 변경 요청
        PrivateRoom_GameManager.instance.SetState(PrivateRoomState.exit);
    }
}
