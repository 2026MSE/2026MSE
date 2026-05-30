using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MainHallManager : MonoBehaviour
{
    public GameObject declareUI;


    PlayerManager playerManager;
    MainGameManager main_game_manager;
    public TextMeshProUGUI[] declareTexts = new TextMeshProUGUI[2];

    StickSide[] declareSticks = new StickSide[2];

    private void Start()
    {
        playerManager = PlayerManager.instance;
        main_game_manager = MainGameManager.instance;
    }

    private void Update()
    {
        if(declareUI.activeSelf)
        {
            UpdateUI();
        }
        switch (main_game_manager.game_stat.turnPhase)
        {
            case TurnPhase.MAIN_HALL_DECLARE:
                CheckTurn();
                break;
            case TurnPhase.MAIN_HALL_CHALLENGE:
                // fix after challenge system is implemented
                break;
        }
    }

    void CheckTurn()
    {
        if (playerManager.isMyTurn())
        {
            //내 턴
            declareUI.SetActive(true);
        }
        else
        {
            //남의 턴
            declareUI.SetActive(false);
        }
    }

    void UpdateUI()
    {
        if (declareSticks[0] == StickSide.HEAD)
        {
            declareTexts[0].text = "Head";
        }
        else if(declareSticks[0] == StickSide.BACK)
        {
            declareTexts[0].text = "BackDo";
        }
        if(declareSticks[1] == StickSide.HEAD)
        {
            declareTexts[1].text = "Head";
        }
        else if (declareSticks[1] == StickSide.TAIL)
        {
            declareTexts[1].text = "Tail";
        }
    }

    public void SubmitDeclare()
    {
        ServerManager.instance.DeclareRequest(declareSticks).Forget();
    }

    public void SetDeclareStick01Head()
    {
        declareSticks[0] = StickSide.HEAD;
    }
    public void SetDeclareStick01Back()
    {
        declareSticks[0] = StickSide.BACK;
    }
    public void SetDeclareStick02Head()
    {
        declareSticks[1] = StickSide.HEAD;
    }
    public void SetDeclareStick02Tail()
    {
        declareSticks[1] = StickSide.TAIL;
    }

    
}
