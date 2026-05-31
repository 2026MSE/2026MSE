using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class YutManager : MonoBehaviour
{
    public static YutManager Instance { get; private set; }

    [Header("보드판 노드")]
    public Transform[] boardNodes;

    [Header("대기, 도착 구역")]
    public Transform waitingArea;
    public Transform finishArea;

    [Header("UI 연결")]
    public Button throwButton;
    public Button turnEndButton;
    public TextMeshProUGUI throwResultText;

    [Header("말 배치 수치값")]
    public float spacing = 1.2f;
    public int maxPerRow = 4;
    public int maxRows = 4;
    public float piggybackHeight = 0.5f;
    public float plateYOffset = 0.0f;

    private Dictionary<string, PieceController> allPiecesDict = new Dictionary<string, PieceController>();

    private MainGameManager main_game_manager;
    private ServerManager server_manager;
    private PlayerManager player_manager;

    private bool is_selecting = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        main_game_manager = MainGameManager.instance;
        server_manager = ServerManager.instance;
        player_manager = PlayerManager.instance;

        throwButton.gameObject.SetActive(false);
        throwButton.onClick.AddListener(OnThrowButtonClicked);
        is_selecting = false;
        if (player_manager.isMyTurn())
        {
            MyTurnStart();
        }
    }

    private void Update()
    {
        if (main_game_manager.game_stat.boardStatus.allPieces.Count <= 0) return;

        UpdateBoardUI();
        
        if (main_game_manager.game_stat.turnPhase == TurnPhase.YUT_MOVE_DONE)
        {
            turnEndButton.gameObject.SetActive(true);
            throwButton.gameObject.SetActive(false);
        }
    }

    public void RegisterPiece(PieceController piece)
    {
        if (!allPiecesDict.ContainsKey(piece.pieceId))
        {
            allPiecesDict.Add(piece.pieceId, piece);
        }
    }

    public void StartGame()
    {
        UpdateBoardUI();
    }

    private void MyTurnStart()
    {
        var yut_results = main_game_manager.game_stat.pendingYutResults;
        
        if (throwResultText != null)
        {
            string tmp_string = "남은 이동 : ";
            foreach(var result in yut_results)
            {
                tmp_string += TranslateYutResult(result.result) + " ";
            }
            throwResultText.text = tmp_string;
        }

        CheckMovablePieces();
    }

    private void CheckMovablePieces()
    {
        List<PieceInfo> movablePieces = new List<PieceInfo>();
        

        foreach (var moveOption in movablePieces)
        {
            if (allPiecesDict.TryGetValue(moveOption.pieceId, out PieceController pieceObj))
            {
                pieceObj.SetClickable(true);
            }
        }
    }

    public async void OnPieceSelected(string pieceId)
    {
        is_selecting = true;

        foreach (var piece in allPiecesDict.Values)
            piece.SetClickable(false);

        await ServerManager.instance.MovePieceRequest(pieceId, target_move);

        // ServerManager의 폴링 대기
        await Task.Delay(1000);

        var state = MainGameManager.instance.boardStatusResponse;
        UpdateBoardUI(state);

        Debug.Log("이동 연출 대기 중...");
        await Task.Delay(1500);

        if (MainGameManager.instance.boardStatusResponse.extraTurn)
        {
            Debug.Log("한 번 더 던집니다! 버튼 활성화.");
            throwButton.gameObject.SetActive(true);
            throwButton.interactable = true;
        }
        else
        {
            await ServerManager.instance.EndTurnRequest();
        }
    }

    // =========================================================
    // [추가 턴 시작] ThrowResponse 기반
    // =========================================================
    private async void OnThrowButtonClicked()
    {
        throwButton.interactable = false;

        await ServerManager.instance.ThrowYutRequest();

        await Task.Delay(1000);

        var state = MainGameManager.instance.boardStatusResponse;

        string throwResultStr = TranslateYutResult(state.throwResult.yutResult.ToString());
        Debug.Log($"추가 던지기 결과: {throwResultStr}");

        if (throwResultText != null)
        {
            throwResultText.text = $"결과: {throwResultStr}";
        }

        throwButton.gameObject.SetActive(false);
        CheckMovablePieces();
    }

    // =========================================================
    // 8. 보드 UI 갱신 로직 (그리드 중앙 정렬 및 업기 처리)
    // =========================================================
    private void UpdateBoardUI()
    {
        BoardStatusResponse state = main_game_manager.game_stat.boardStatus;

        if (state.allPieces == null) return;

        int waitingCount = 0;
        int finishCount = 0;
        Dictionary<int, int> nodePieceCount = new Dictionary<int, int>();

        float startX = -(maxPerRow - 1) * spacing / 2f;
        float startZ = -(maxRows - 1) * spacing / 2f;

        foreach (var kvp in state.allPieces)
        {
            foreach (var pieceData in kvp.Value)
            {
                if (allPiecesDict.TryGetValue(pieceData.id, out PieceController pieceObj))
                {
                    Vector3 targetPosition = Vector3.zero;
                    int pos = pieceData.currentPosition;

                    if (pos == -1) // 대기석
                    {
                        float offsetX = startX + (waitingCount % maxPerRow) * spacing;
                        float offsetZ = startZ + (waitingCount / maxPerRow) * spacing;

                        if (waitingArea != null)
                        {
                            targetPosition = waitingArea.position + new Vector3(offsetX, plateYOffset, offsetZ);
                        }
                        waitingCount++;
                    }
                    else if (pos == 99) // 완주석
                    {
                        float offsetX = startX + (finishCount % maxPerRow) * spacing;
                        float offsetZ = startZ + (finishCount / maxPerRow) * spacing;

                        if (finishArea != null)
                        {
                            targetPosition = finishArea.position + new Vector3(offsetX, plateYOffset, offsetZ);
                        }
                        finishCount++;
                    }
                    else // 보드판 위
                    {
                        if (!nodePieceCount.ContainsKey(pos)) nodePieceCount[pos] = 0;

                        float offsetY = plateYOffset + (nodePieceCount[pos] * piggybackHeight);
                        targetPosition = boardNodes[pos].position + new Vector3(0, offsetY, 0);

                        nodePieceCount[pos]++;
                    }

                    pieceObj.transform.DOMove(targetPosition, 0.5f).SetEase(Ease.OutQuad);
                }
            }
        }
    }

    
    private string TranslateYutResult(YutName name)
    {
        switch (name.ToString().ToUpper())
        {
            case "DO": return "도";
            case "GAE": return "개";
            case "GEOL": return "걸";
            case "YUT": return "윷";
            case "MO": return "모";
            case "BACK_DO":
            case "BACKDO": return "빽도";
            default: return name.ToString();
        }
    }
}