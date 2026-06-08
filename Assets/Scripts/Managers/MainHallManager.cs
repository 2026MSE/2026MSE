using System.Collections;
using System.Collections.Generic;
using TMPro;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class MainHallManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject declareUI;
    public GameObject resultUI;

    [Header("Text Components")]
    public TextMeshProUGUI[] declareTexts = new TextMeshProUGUI[2];
    public TextMeshProUGUI[] public_result_texts = new TextMeshProUGUI[3];
    public TextMeshProUGUI all_result_text;
    public TextMeshProUGUI challenge_result_text;

    [Header("Buttons")]
    public Button declareButton2_1;
    public Button declareButton2_2;
    public Button challengeOButton;
    public Button challengeXButton;
    public Button challenge_confirm_button;

    [Header("Misc UI")]
    public Slider challengeTimer;

    [Header("Data")]
    public StickSide[] declareSticks = new StickSide[2];

    private PlayerManager playerManager;
    private MainGameManager main_game_manager;
    private TurnPhase _lastTurnPhase = TurnPhase.WAITING;

    private void Start()
    {
        playerManager = PlayerManager.instance;
        main_game_manager = MainGameManager.instance;

        challengeOButton.onClick.AddListener(ChallengeO);
        challengeXButton.onClick.AddListener(ChallengeX);
        challenge_confirm_button.onClick.AddListener(ChallengeConfirm);
    }

    private void Update()
    {
        if (main_game_manager == null || main_game_manager.game_stat == null) return;

        TurnPhase currentPhase = main_game_manager.game_stat.turnPhase;

        if (_lastTurnPhase != currentPhase)
        {
            OnPhaseChanged(currentPhase);
            _lastTurnPhase = currentPhase;
        }

        if (currentPhase == TurnPhase.MAIN_HALL_CHALLENGE && challengeTimer.gameObject.activeSelf)
        {
            UpdateChallengeTimer();
        }
    }

    private void OnPhaseChanged(TurnPhase phase)
    {
        switch (phase)
        {
            case TurnPhase.PRIVATE_THROW:
                challenge_confirm_button.gameObject.SetActive(false);
                challenge_result_text.gameObject.SetActive(false);
                break;
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

    private void UpdateChallengeTimer()
    {
        float timeLeft = (main_game_manager.game_stat.challengeDeadlineMillis - main_game_manager.game_stat.serverTimeMillis) / 60000f;
        challengeTimer.value = Mathf.Max(0, timeLeft);
    }

    public void ChallengeO()
    {
        ServerManager.instance.ChallengeRequest(true).Forget();
        challengeOButton.gameObject.SetActive(false);
        challengeXButton.gameObject.SetActive(false);
    }

    public void ChallengeX()
    {
        ServerManager.instance.ChallengeRequest(false).Forget();
        challengeOButton.gameObject.SetActive(false);
        challengeXButton.gameObject.SetActive(false);
    }

    public void ChallengeConfirm()
    {
        ServerManager.instance.ChallengeConfirmRequest().Forget();
        challenge_confirm_button.gameObject.SetActive(false);
        challenge_result_text.gameObject.SetActive(false);
    }

    private void CheckDeclareTurn()
    {
        challenge_confirm_button.gameObject.SetActive(false);
        challenge_result_text.gameObject.SetActive(false);

        if (playerManager.isMyTurn())
        {
            declareUI.SetActive(true);
            resultUI.SetActive(true);
            UpdateUI();

            public_result_texts[0].text = main_game_manager.game_stat.publicSticks[0].ToString();
            public_result_texts[1].text = main_game_manager.game_stat.publicSticks[1].ToString();

            if (main_game_manager.game_stat.publicSticks.Length >= 3)
            {
                public_result_texts[2].gameObject.SetActive(true);
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
            declareUI.SetActive(false);
        }
    }

    private void ChallengeTurn()
    {
        SetYutResultText();

        declareUI.SetActive(false);
        resultUI.SetActive(true);
        public_result_texts[2].gameObject.SetActive(true);
        public_result_texts[3].gameObject.SetActive(true);

        var stat = main_game_manager.game_stat;
        public_result_texts[0].text = stat.publicSticks[0].ToString();
        public_result_texts[1].text = stat.publicSticks[1].ToString();

        if (stat.publicSticks.Length >= 3)
        {
            public_result_texts[2].text = stat.publicSticks[2].ToString();
            public_result_texts[3].text = stat.declaredPrivateSticks[0].ToString();
        }
        else
        {
            public_result_texts[2].text = stat.declaredPrivateSticks[1].ToString();
            public_result_texts[3].text = stat.declaredPrivateSticks[0].ToString();
        }

        challengeTimer.gameObject.SetActive(true);
        UpdateChallengeTimer();

        bool isMyTurn = playerManager.isMyTurn();
        challengeOButton.gameObject.SetActive(!isMyTurn || ServerManager.instance.is_debugging);
        challengeXButton.gameObject.SetActive(!isMyTurn);
    }

    private void Result()
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

        challenge_confirm_button.gameObject.SetActive(playerManager.isMyTurn());
    }

    private void UpdateUI()
    {
        declareTexts[0].text = (declareSticks[0] == StickSide.HEAD) ? "Head" : "BackDo";

        if (main_game_manager.game_stat.privateSticks.Length <= 1)
        {
            declareTexts[1].gameObject.SetActive(false);
            declareButton2_1.gameObject.SetActive(false);
            declareButton2_2.gameObject.SetActive(false);
            return;
        }

        declareTexts[1].gameObject.SetActive(true);
        declareButton2_1.gameObject.SetActive(true);
        declareButton2_2.gameObject.SetActive(true);

        declareTexts[1].text = (declareSticks[1] == StickSide.HEAD) ? "Head" : "Tail";
    }

    public void SubmitDeclare()
    {
        ServerManager.instance.DeclareRequest(declareSticks).Forget();
    }

    public void SetDeclareStick01Head() { declareSticks[0] = StickSide.HEAD; UpdateUI(); SetYutResultText(); }
    public void SetDeclareStick01Back() { declareSticks[0] = StickSide.BACK; UpdateUI(); SetYutResultText(); }
    public void SetDeclareStick02Head() { declareSticks[1] = StickSide.HEAD; UpdateUI(); SetYutResultText(); }
    public void SetDeclareStick02Tail() { declareSticks[1] = StickSide.TAIL; UpdateUI(); SetYutResultText(); }

    void SetYutResultText()
    {
        StickSide[] sticks = new StickSide[4];

        if (declareSticks.Length <= 1)
        {
            if (main_game_manager.game_stat.turnPhase == TurnPhase.MAIN_HALL_DECLARE)
                sticks[0] = declareSticks[0];
            else
                sticks[0] = main_game_manager.game_stat.declaredPrivateSticks[0];

            sticks[1] = main_game_manager.game_stat.publicSticks[0];
            sticks[2] = main_game_manager.game_stat.publicSticks[1];
            sticks[3] = main_game_manager.game_stat.publicSticks[2];
        }
        else
        {
            if (main_game_manager.game_stat.turnPhase == TurnPhase.MAIN_HALL_DECLARE)
            {
                sticks[0] = declareSticks[0];
                sticks[1] = declareSticks[1];
            }
            else
            {
                if(main_game_manager.game_stat.declaredPrivateSticks.Length <= 1)
                {
                    sticks[0] = main_game_manager.game_stat.declaredPrivateSticks[0];
                    sticks[1] = main_game_manager.game_stat.publicSticks[1];
                }
                else
                {
                    sticks[0] = main_game_manager.game_stat.declaredPrivateSticks[0];
                    sticks[1] = main_game_manager.game_stat.declaredPrivateSticks[1];
                }
            }

            sticks[2] = main_game_manager.game_stat.publicSticks[0];
            sticks[3] = main_game_manager.game_stat.publicSticks[1];
        }

        int tail_count = 0;

        foreach (var stick in sticks)
        {
            if (stick == StickSide.TAIL || stick == StickSide.BACK)
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

        if (tail_count == 1 && declareSticks[0] == StickSide.BACK)
        {
            all_result_text.text = YutName.BACK_DO.ToString();
        }
    }
}