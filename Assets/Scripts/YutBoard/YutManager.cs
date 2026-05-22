using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class YutManager : MonoBehaviour
{
    public static YutManager Instance { get; private set; }

    [Header("보드판 노드 (인덱스 = 서버 currentPosition)")]
    public Transform[] boardNodes;

    [Header("특수 구역 위치")]
    public Transform waitingArea;
    public Transform finishArea;

    [Header("UI 연결")]
    public Button throwButton;
    public Button endTurnButton;
    public TextMeshProUGUI throwResultText;

    [Header("말 배치 상세 설정")]
    public float spacing = 1.2f;
    public int maxPerRow = 4;
    public int maxRows = 4;
    public float piggybackHeight = 0.5f;
    public float plateYOffset = 0.0f;

    private readonly Dictionary<string, PieceController> allPiecesDict =
        new Dictionary<string, PieceController>();

    // pieceId -> yutResultIndex
    // 어떤 말을 클릭했을 때 pendingYutResults 중 몇 번째 결과를 사용할지 저장
    private readonly Dictionary<string, int> pieceMoveIndexDict =
        new Dictionary<string, int>();

    private bool wasMyMovePhaseLastFrame = false;
    private float syncTimer = 0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (throwButton != null)
        {
            throwButton.gameObject.SetActive(false);
            throwButton.onClick.AddListener(OnThrowButtonClicked);
        }

        if (endTurnButton != null)
        {
            endTurnButton.gameObject.SetActive(false);
            endTurnButton.onClick.AddListener(OnEndTurnButtonClicked);
        }
    }

    private void Update()
    {
        if (MainGameManager.instance == null) return;

        BoardStatusResponse boardState = MainGameManager.instance.boardStatusResponse;
        if (boardState == null || boardState.allPieces == null) return;

        // 보드 화면에 들어온 상태에서만 보드 동기화
        if (MainGameManager.instance.currentGameViewScene != GameViewScene.YUT_ROOM)
        {
            HideBoardControls();
            wasMyMovePhaseLastFrame = false;
            return;
        }

        // 서버 polling으로 갱신된 boardStatusResponse를 1초마다 보드에 반영
        syncTimer += Time.deltaTime;
        if (syncTimer >= 1f)
        {
            syncTimer = 0f;
            UpdateBoardUI(boardState);
        }

        bool canMove = MainGameManager.instance.CanMovePiece();
        bool canThrow = MainGameManager.instance.CanThrowYut();
        bool canEndTurn = MainGameManager.instance.CanEndTurn();

        // YUT_MOVE 단계에 처음 진입했을 때 이동 가능한 말 조회
        if (!wasMyMovePhaseLastFrame && canMove)
        {
            OnMyMovePhaseStarted();
        }

        // 이동 단계가 아니면 클릭 가능 상태 제거
        if (wasMyMovePhaseLastFrame && !canMove)
        {
            SetAllPiecesClickable(false);
            pieceMoveIndexDict.Clear();
        }

        wasMyMovePhaseLastFrame = canMove;

        // 버튼 표시
        if (throwButton != null)
        {
            throwButton.gameObject.SetActive(canThrow);
            throwButton.interactable = canThrow;
        }

        if (endTurnButton != null)
        {
            endTurnButton.gameObject.SetActive(canEndTurn);
            endTurnButton.interactable = canEndTurn;
        }
    }

    public void RegisterPiece(PieceController piece)
    {
        if (piece == null || string.IsNullOrEmpty(piece.pieceId)) return;

        if (!allPiecesDict.ContainsKey(piece.pieceId))
        {
            allPiecesDict.Add(piece.pieceId, piece);
        }
    }

    public void StartGameAfterInit(BoardStatusResponse initialState)
    {
        UpdateBoardUI(initialState);
    }

    private async void OnMyMovePhaseStarted()
    {
        Debug.Log("[YutManager] 내 말 이동 단계 시작");

        ShowCurrentPendingResults();

        await CheckMovablePieces();
    }

    private async Task CheckMovablePieces()
    {
        if (!MainGameManager.instance.CanMovePiece())
        {
            SetAllPiecesClickable(false);
            return;
        }

        await ServerManager.instance.MoveListRequest();

        MoveListResponse moveList = MainGameManager.instance.moveListResponse;

        if (moveList == null || moveList.moveGroups == null || moveList.moveGroups.Count == 0)
        {
            Debug.Log("[YutManager] 이동 가능한 말이 없습니다. 턴 종료 요청");
            await ServerManager.instance.EndTurnRequest();
            return;
        }

        SetAllPiecesClickable(false);
        pieceMoveIndexDict.Clear();

        foreach (MoveGroup group in moveList.moveGroups)
        {
            if (group == null || group.movablePieces == null) continue;

            foreach (MoveOption option in group.movablePieces)
            {
                if (option == null || string.IsNullOrEmpty(option.pieceId)) continue;

                // 같은 말이 여러 윷 결과로 이동 가능하면 일단 첫 번째 결과를 사용
                // 나중에 UI에서 윷 결과 선택 기능을 만들면 여기 구조를 확장하면 됨
                if (!pieceMoveIndexDict.ContainsKey(option.pieceId))
                {
                    pieceMoveIndexDict.Add(option.pieceId, group.yutResultIndex);
                }

                if (allPiecesDict.TryGetValue(option.pieceId, out PieceController pieceObj))
                {
                    pieceObj.SetClickable(true);
                }
            }
        }

        Debug.Log("[YutManager] 이동할 말을 선택하세요.");
    }

    public async void OnPieceSelected(string pieceId)
    {
        if (!MainGameManager.instance.CanMovePiece())
        {
            Debug.LogWarning("[YutManager] 현재는 말을 이동할 수 있는 단계가 아닙니다.");
            return;
        }

        if (!pieceMoveIndexDict.TryGetValue(pieceId, out int yutResultIndex))
        {
            Debug.LogWarning($"[YutManager] 선택한 말에 해당하는 yutResultIndex를 찾지 못했습니다. pieceId={pieceId}");
            return;
        }

        SetAllPiecesClickable(false);

        await ServerManager.instance.MovePieceRequest(pieceId, yutResultIndex);

        // 서버 polling이 boardStatusResponse를 갱신할 시간을 약간 둠
        await Task.Delay(500);

        UpdateBoardUI(MainGameManager.instance.boardStatusResponse);

        // 서버가 이동 후 상태를 YUT_MOVE, YUT_MOVE_DONE, CATCH_BONUS_THROW 등으로 바꿔줄 것
        await Task.Delay(700);

        if (MainGameManager.instance.CanMovePiece())
        {
            // pendingYutResults가 남아 있으면 계속 이동 가능
            await CheckMovablePieces();
        }
        else if (MainGameManager.instance.CanEndTurn())
        {
            if (endTurnButton != null)
            {
                endTurnButton.gameObject.SetActive(true);
                endTurnButton.interactable = true;
            }
        }
        else if (MainGameManager.instance.CanThrowYut())
        {
            if (throwButton != null)
            {
                throwButton.gameObject.SetActive(true);
                throwButton.interactable = true;
            }
        }
    }

    private async void OnThrowButtonClicked()
    {
        if (!MainGameManager.instance.CanThrowYut())
        {
            Debug.LogWarning("[YutManager] 현재는 윷을 던질 수 있는 단계가 아닙니다.");
            return;
        }

        if (throwButton != null)
            throwButton.interactable = false;

        await ServerManager.instance.ThrowYutRequest();

        await Task.Delay(500);

        YutResult result = null;

        if (MainGameManager.instance.throwResponse != null)
        {
            result = MainGameManager.instance.throwResponse.yutResult;
        }
        else if (MainGameManager.instance.gameState != null)
        {
            result = MainGameManager.instance.gameState.currentYutResult;
        }

        if (throwResultText != null && result != null)
        {
            throwResultText.text = $"결과: {TranslateYutResult(result.result)}";
        }

        if (throwButton != null)
            throwButton.gameObject.SetActive(false);
    }

    private async void OnEndTurnButtonClicked()
    {
        if (!MainGameManager.instance.CanEndTurn())
        {
            Debug.LogWarning("[YutManager] 현재는 턴 종료 단계가 아닙니다.");
            return;
        }

        if (endTurnButton != null)
            endTurnButton.interactable = false;

        await ServerManager.instance.EndTurnRequest();

        HideBoardControls();
    }

    private void HideBoardControls()
    {
        SetAllPiecesClickable(false);
        pieceMoveIndexDict.Clear();

        if (throwButton != null)
            throwButton.gameObject.SetActive(false);

        if (endTurnButton != null)
            endTurnButton.gameObject.SetActive(false);
    }

    private void SetAllPiecesClickable(bool clickable)
    {
        foreach (PieceController piece in allPiecesDict.Values)
        {
            if (piece != null)
                piece.SetClickable(clickable);
        }
    }

    private void UpdateBoardUI(BoardStatusResponse state)
    {
        if (state == null || state.allPieces == null) return;

        int waitingCount = 0;
        int finishCount = 0;

        Dictionary<int, int> nodePieceCount = new Dictionary<int, int>();

        float startX = -(maxPerRow - 1) * spacing / 2f;
        float startZ = -(maxRows - 1) * spacing / 2f;

        foreach (var kvp in state.allPieces)
        {
            List<Piece> pieces = kvp.Value;
            if (pieces == null) continue;

            foreach (Piece pieceData in pieces)
            {
                if (pieceData == null) continue;

                if (!allPiecesDict.TryGetValue(pieceData.id, out PieceController pieceObj))
                    continue;

                Vector3 targetPosition = Vector3.zero;
                int pos = pieceData.currentPosition;

                if (pos == -1)
                {
                    float offsetX = startX + (waitingCount % maxPerRow) * spacing;
                    float offsetZ = startZ + (waitingCount / maxPerRow) * spacing;

                    if (waitingArea != null)
                    {
                        targetPosition = waitingArea.position + new Vector3(offsetX, plateYOffset, offsetZ);
                    }

                    waitingCount++;
                }
                else if (pos == 99)
                {
                    float offsetX = startX + (finishCount % maxPerRow) * spacing;
                    float offsetZ = startZ + (finishCount / maxPerRow) * spacing;

                    if (finishArea != null)
                    {
                        targetPosition = finishArea.position + new Vector3(offsetX, plateYOffset, offsetZ);
                    }

                    finishCount++;
                }
                else
                {
                    if (pos < 0 || pos >= boardNodes.Length || boardNodes[pos] == null)
                    {
                        Debug.LogWarning($"[YutManager] 유효하지 않은 보드 위치입니다. pieceId={pieceData.id}, pos={pos}");
                        continue;
                    }

                    if (!nodePieceCount.ContainsKey(pos))
                        nodePieceCount[pos] = 0;

                    float offsetY = plateYOffset + nodePieceCount[pos] * piggybackHeight;
                    targetPosition = boardNodes[pos].position + new Vector3(0, offsetY, 0);

                    nodePieceCount[pos]++;
                }

                pieceObj.transform.DOMove(targetPosition, 0.5f).SetEase(Ease.OutQuad);
            }
        }
    }

    private void ShowCurrentPendingResults()
    {
        if (throwResultText == null) return;

        List<YutResult> pending = MainGameManager.instance.gameState?.pendingYutResults;

        if (pending == null || pending.Count == 0)
        {
            throwResultText.text = "이동할 윷 결과 없음";
            return;
        }

        List<string> names = new List<string>();

        foreach (YutResult result in pending)
        {
            if (result != null)
                names.Add(TranslateYutResult(result.result));
        }

        throwResultText.text = "이동 가능 결과: " + string.Join(", ", names);
    }

    private string TranslateYutResult(YutName yutName)
    {
        switch (yutName)
        {
            case YutName.BACK_DO:
                return "빽도";
            case YutName.DO:
                return "도";
            case YutName.GAE:
                return "개";
            case YutName.GEOL:
                return "걸";
            case YutName.YUT:
                return "윷";
            case YutName.MO:
                return "모";
            default:
                return yutName.ToString();
        }
    }
}