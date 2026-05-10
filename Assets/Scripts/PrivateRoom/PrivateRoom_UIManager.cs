using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using TMPro;

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
    public List<GameObject> yut_positions;
    public List<GameObject> yut_objects;
    public GameObject yut_prefab;
    public TextMeshProUGUI yutResultText;

    void Start()
    {
        // 버튼 클릭 이벤트 연결
        throwButton.onClick.AddListener(OnThrowButtonClicked);
        chanceSelectButton.onClick.AddListener(OnChanceSelectButtonClicked);
        exitButton.onClick.AddListener(OnExitButtonClicked);

        // 초기 상태 설정
        throwButton.gameObject.SetActive(false);
        chanceSelectButton.gameObject.SetActive(false);
        exitButton.gameObject.SetActive(false);
        yutResultText.gameObject.SetActive(false);
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

    public void EnterChanceSelect()
    {
        /*
         * 찬스 선택 UI 활성화
         */
    }

    void OnThrowButtonClicked()
    {
        // 던지기 버튼 클릭 시 게임 매니저에 상태 변경 요청
        PrivateRoom_GameManager.instance.SetState(PrivateRoomState.Throwing);
    }

    void OnChanceSelectButtonClicked()
    {
        // 찬스 선택 버튼 클릭 시 게임 매니저에 상태 변경 요청
        PrivateRoom_GameManager.instance.SetState(PrivateRoomState.Chance_select);
    }

    void OnExitButtonClicked()
    {
        // 나가기 버튼 클릭 시 게임 매니저에 상태 변경 요청
        PrivateRoom_GameManager.instance.SetState(PrivateRoomState.Exit);
    }

    public void ShowYut()
    {
        yut_positions.ForEach(pos => yut_objects.Add(Instantiate(yut_prefab, pos.transform.position, Quaternion.identity)));

        for (int i = 0; i < 4; i++)
        {
            yut_objects[i].GetComponent<YutState>().stickSide = MainGameManager.instance.throwResponse.sticks[i];
        }
        yutResultText.gameObject.SetActive(true);
        yutResultText.text = MainGameManager.instance.throwResponse.yutResult.result.ToString();
    }
}
