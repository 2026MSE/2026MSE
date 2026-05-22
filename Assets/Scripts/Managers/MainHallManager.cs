using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class MainHallManager : MonoBehaviour
{
    [Header("Declare UI")]
    public GameObject declareUI;
    public TextMeshProUGUI[] declareTexts = new TextMeshProUGUI[2];

    [Header("Challenge UI")]
    public GameObject challengeUI;
    public Button challengeOButton;
    public Button challengeXButton;
    public TextMeshProUGUI challengeTimerText;
    public TextMeshProUGUI challengeInfoText;

    private PlayerManager playerManager;
    private StickSide[] declareSticks = new StickSide[2];

    private void Start()
    {
        playerManager = PlayerManager.instance;

        // 기본 선언값
        declareSticks[0] = StickSide.HEAD;
        declareSticks[1] = StickSide.HEAD;

        if (declareUI != null)
            declareUI.SetActive(false);

        if (challengeUI != null)
            challengeUI.SetActive(false);

        if (challengeOButton != null)
        {
            challengeOButton.onClick.RemoveAllListeners();
            challengeOButton.onClick.AddListener(OnClickChallengeO);
        }

        if (challengeXButton != null)
        {
            challengeXButton.onClick.RemoveAllListeners();
            challengeXButton.onClick.AddListener(OnClickChallengeX);
        }

        UpdateDeclareUI();
    }

    private void Update()
    {
        if (MainGameManager.instance == null)
            return;

        if (playerManager == null)
            playerManager = PlayerManager.instance;

        UpdateMainHallUI();
    }

    private void UpdateMainHallUI()
    {
        MainGameManager gm = MainGameManager.instance;

        bool isMainHallView =
            gm.currentGameViewScene == GameViewScene.MAIN_HALL;

        if (!isMainHallView)
        {
            HideAllUI();
            return;
        }

        UpdateDeclarePanel();
        UpdateChallengePanel();
    }

    private void UpdateDeclarePanel()
    {
        bool canDeclare =
            MainGameManager.instance.CanDeclare();

        if (declareUI != null)
            declareUI.SetActive(canDeclare);

        if (canDeclare)
            UpdateDeclareUI();
    }

    private void UpdateChallengePanel()
    {
        bool canChallenge =
            MainGameManager.instance.CanChallengeVote();

        if (challengeUI != null)
            challengeUI.SetActive(canChallenge);

        if (!canChallenge)
            return;

        UpdateChallengeTimer();
        UpdateChallengeInfo();
    }

    private void HideAllUI()
    {
        if (declareUI != null)
            declareUI.SetActive(false);

        if (challengeUI != null)
            challengeUI.SetActive(false);
    }

    private void UpdateDeclareUI()
    {
        if (declareTexts == null || declareTexts.Length < 2)
            return;

        if (declareTexts[0] != null)
            declareTexts[0].text = StickSideToText(declareSticks[0]);

        if (declareTexts[1] != null)
            declareTexts[1].text = StickSideToText(declareSticks[1]);
    }

    private void UpdateChallengeTimer()
    {
        GameStateResponse state = MainGameManager.instance.gameState;

        if (state == null || challengeTimerText == null)
            return;

        long remainMillis = state.challengeDeadlineMillis - state.serverTimeMillis;
        float remainSeconds = Mathf.Max(0f, remainMillis / 1000f);

        challengeTimerText.text = $"남은 시간: {remainSeconds:F0}초";
    }

    private void UpdateChallengeInfo()
    {
        if (challengeInfoText == null)
            return;

        TurnInfo turnInfo = MainGameManager.instance.turnInfo;

        if (turnInfo == null)
        {
            challengeInfoText.text = "챌린지 대기 중";
            return;
        }

        challengeInfoText.text = $"현재 턴 플레이어의 선언을 믿을까요?";
    }

    public void SubmitDeclare()
    {
        if (!MainGameManager.instance.CanDeclare())
        {
            Debug.LogWarning("[MainHallManager] 현재는 선언할 수 있는 단계가 아닙니다.");
            return;
        }

        ServerManager.instance.DeclareRequest(declareSticks).Forget();

        if (declareUI != null)
            declareUI.SetActive(false);
    }

    private void OnClickChallengeO()
    {
        if (!MainGameManager.instance.CanChallengeVote())
        {
            Debug.LogWarning("[MainHallManager] 현재는 챌린지를 할 수 있는 단계가 아닙니다.");
            return;
        }

        ServerManager.instance.ChallengeVoteRequest(true).Forget();

        if (challengeUI != null)
            challengeUI.SetActive(false);
    }

    private void OnClickChallengeX()
    {
        if (!MainGameManager.instance.CanChallengeVote())
        {
            Debug.LogWarning("[MainHallManager] 현재는 챌린지 투표를 할 수 있는 단계가 아닙니다.");
            return;
        }

        ServerManager.instance.ChallengeVoteRequest(false).Forget();

        if (challengeUI != null)
            challengeUI.SetActive(false);
    }

    public void SetDeclareStick01Head()
    {
        declareSticks[0] = StickSide.HEAD;
        UpdateDeclareUI();
    }

    public void SetDeclareStick01Back()
    {
        declareSticks[0] = StickSide.BACK;
        UpdateDeclareUI();
    }

    public void SetDeclareStick02Head()
    {
        declareSticks[1] = StickSide.HEAD;
        UpdateDeclareUI();
    }

    public void SetDeclareStick02Tail()
    {
        declareSticks[1] = StickSide.TAIL;
        UpdateDeclareUI();
    }

    private string StickSideToText(StickSide side)
    {
        switch (side)
        {
            case StickSide.HEAD:
                return "Head";

            case StickSide.TAIL:
                return "Tail";

            case StickSide.BACK:
                return "BackDo";

            default:
                return side.ToString();
        }
    }
}