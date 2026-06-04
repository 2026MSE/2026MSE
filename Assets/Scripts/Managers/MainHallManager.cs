using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MainHallManager : MonoBehaviour
{
    public GameObject declareUI;
    public GameObject resultUI;

    PlayerManager playerManager;
    MainGameManager main_game_manager;
    public TextMeshProUGUI[] declareTexts = new TextMeshProUGUI[2];
    public TextMeshProUGUI[] public_result_texts = new TextMeshProUGUI[3];
    public TextMeshProUGUI all_result_text;
    public Button declareButton2_1;
    public Button declareButton2_2;
    public Button challengeOButton;
    public Button challengeXButton;
    public Slider challengeTimer;
    public TextMeshProUGUI challenge_result_text;
    public Button challenge_confirm_button;

    public StickSide[] declareSticks = new StickSide[2];

    private void Start()
    {
        playerManager = PlayerManager.instance;
        main_game_manager = MainGameManager.instance;

        challengeOButton.onClick.AddListener(challengeO);
        challengeXButton.onClick.AddListener(challengeX);
        challenge_confirm_button.onClick.AddListener(challengeConfirm);
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
                CheckDeclareTurn();
                break;
            case TurnPhase.MAIN_HALL_CHALLENGE:
                ChallengeTurn();
                break;
            case TurnPhase.CHALLENGE_RESULT:
                Result();
                break;
        }

    }

    public void challengeO()
    {
        ServerManager.instance.ChallengeRequest(true).Forget();
        challengeOButton.gameObject.SetActive(false);
        challengeXButton.gameObject.SetActive(false);
    }

    public void challengeX()
    {
        ServerManager.instance.ChallengeRequest(false).Forget();
        challengeOButton.gameObject.SetActive(false);
        challengeXButton.gameObject.SetActive(false);
    }

    public void challengeConfirm()
    {
        ServerManager.instance.ChallengeConfirmRequest().Forget();
        challenge_confirm_button.gameObject.SetActive(false);
        challenge_result_text.gameObject.SetActive(false);
    }

    void CheckDeclareTurn()
    {
        challenge_confirm_button.gameObject.SetActive(false);
        challenge_result_text.gameObject.SetActive(false);

        if (playerManager.isMyTurn())
        {
            //내 턴
            declareUI.SetActive(true); resultUI.SetActive(true);

            public_result_texts[0].text = main_game_manager.game_stat.publicSticks[0].ToString();
            public_result_texts[1].text = main_game_manager.game_stat.publicSticks[1].ToString();
            if (main_game_manager.game_stat.publicSticks.Length >= 3)
            {
                public_result_texts[2].text = main_game_manager.game_stat.publicSticks[2].ToString();
            }
            else
            {
                public_result_texts[2].gameObject.SetActive(false);
            }
            public_result_texts[3].gameObject.SetActive(false);
        }
        else
        {
            //남의 턴
            declareUI.SetActive(false);
        }
    }
    
    void ChallengeTurn()
    {
        declareUI.SetActive(false); resultUI.SetActive(true);
        public_result_texts[2].gameObject.SetActive(true);
        public_result_texts[3].gameObject.SetActive(true);

        public_result_texts[0].text = main_game_manager.game_stat.publicSticks[0].ToString();
        public_result_texts[1].text = main_game_manager.game_stat.publicSticks[1].ToString();
        if (main_game_manager.game_stat.publicSticks.Length >= 3)
        {
            public_result_texts[2].text = main_game_manager.game_stat.publicSticks[2].ToString();
            public_result_texts[3].text = main_game_manager.game_stat.declaredPrivateSticks[0].ToString();
        }
        else
        {
            public_result_texts[2].text = main_game_manager.game_stat.declaredPrivateSticks[1].ToString();
            public_result_texts[3].text = main_game_manager.game_stat.declaredPrivateSticks[0].ToString();
        }

        challengeTimer.gameObject.SetActive(true);
        challengeTimer.value = (main_game_manager.game_stat.challengeDeadlineMillis - main_game_manager.game_stat.serverTimeMillis) / 60000f;
        
        if (playerManager.isMyTurn())
        {
            challengeOButton.gameObject.SetActive(false);
            challengeXButton.gameObject.SetActive(false);
            if (ServerManager.instance.is_debugging)
            {
                challengeOButton.gameObject.SetActive(true);
            }
        }
        else
        {
            challengeTimer.gameObject.SetActive(true);
            challengeTimer.value = (main_game_manager.game_stat.challengeDeadlineMillis - main_game_manager.game_stat.serverTimeMillis) / 60000f;
            challengeOButton.gameObject.SetActive(true);
            challengeXButton.gameObject.SetActive(true);
        }
    }

    void Result()
    {
        challengeTimer.gameObject.SetActive(false);
        challengeOButton.gameObject.SetActive(false);
        challengeXButton.gameObject.SetActive(false);
        resultUI.SetActive(false);

        challenge_result_text.gameObject.SetActive(true);

        if (main_game_manager.game_stat.lastJudgeResponse == null)
        {
            challenge_result_text.text = "No Challenge";
            
        }
        else
        {
            challenge_result_text.text = main_game_manager.game_stat.lastJudgeResponse.judgeResult.ToString();
        }

        if (playerManager.isMyTurn())
        {
            challenge_confirm_button.gameObject.SetActive(true);
        }
        else
        {
            challenge_confirm_button.gameObject.SetActive(false);
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

        if (main_game_manager.game_stat.privateSticks.Length <= 1)
        {
            declareTexts[1].gameObject.SetActive(false);
            declareButton2_1.gameObject.SetActive(false);
            declareButton2_2.gameObject.SetActive(false);
            return;
        }
        else
        {
            declareTexts[1].gameObject.SetActive(true);
            declareButton2_1.gameObject.SetActive(true);
            declareButton2_2.gameObject.SetActive(true);
        }

        if (declareSticks[1] == StickSide.HEAD)
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
        SetYutResultText();
    }
    public void SetDeclareStick01Back()
    {
        declareSticks[0] = StickSide.BACK;
        SetYutResultText();
    }
    public void SetDeclareStick02Head()
    {
        declareSticks[1] = StickSide.HEAD;
        SetYutResultText();
    }
    public void SetDeclareStick02Tail()
    {
        declareSticks[1] = StickSide.TAIL;
        SetYutResultText();
    }

    void SetYutResultText()
    {
        StickSide[] sticks = new StickSide[4];
        if(declareSticks.Length <= 1)
        {
            sticks[0] = declareSticks[0];
            sticks[1] = main_game_manager.game_stat.publicSticks[0];
            sticks[2] = main_game_manager.game_stat.publicSticks[1];
            sticks[3] = main_game_manager.game_stat.publicSticks[2];
        }
        else
        {
            sticks[0] = declareSticks[0];
            sticks[1] = declareSticks[1];
            sticks[2] = main_game_manager.game_stat.publicSticks[0];
            sticks[3] = main_game_manager.game_stat.publicSticks[1];
        }
        int tail_count = 0;
        foreach(var stick in sticks)
        {
            if(stick == StickSide.TAIL || stick == StickSide.BACK)
            {   
                tail_count++;
            }
        }

        switch (tail_count)
        {
            case 0:
                all_result_text.text = YutName.MO.ToString();
                break;
            case 1:
                all_result_text.text = YutName.DO.ToString();
                break;
            case 2:
                all_result_text.text = YutName.GAE.ToString();
                break;
            case 3:
                all_result_text.text = YutName.GEOL.ToString();
                break;
            case 4:
                all_result_text.text = YutName.YUT.ToString();
                break;
        }
        if(tail_count == 1 && declareSticks[0] == StickSide.BACK)
        {
            all_result_text.text = YutName.BACK_DO.ToString();
        }
    }
}
