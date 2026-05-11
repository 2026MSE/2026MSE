using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class YutManager : MonoBehaviour
{
    public static YutManager Instance { get; private set; }

    [Header("보드판 노드 (인덱스 = 노드 ID)")]
    public Transform[] boardNodes;

    [Header("특수 구역 위치")]
    public Transform waitingArea;
    public Transform finishArea;

    [Header("UI 연결")]
    public Button throwButton;
    public TextMeshProUGUI throwResultText;

    [Header("말 배치 상세 설정 (인스펙터 조절용)")]
    public float spacing = 1.2f;
    public int maxPerRow = 4;
    public int maxRows = 4;
    public float piggybackHeight = 0.5f;
    public float plateYOffset = 0.0f;

    private Dictionary<string, PieceController> allPiecesDict = new Dictionary<string, PieceController>();

    // 턴 전환 감지 및 동기화용 변수
    private bool wasMyTurnLastFrame = false;
    private float syncTimer = 0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        throwButton.gameObject.SetActive(false);
        throwButton.onClick.AddListener(OnThrowButtonClicked);
    }

    private void Update()
    {
        if (MainGameManager.instance.boardStatusResponse == null) return;

        // 1초 단위로 UI 갱신 (상대방 이동 동기화)
        syncTimer += Time.deltaTime;
        if (syncTimer >= 1f)
        {
            syncTimer = 0f;
            UpdateBoardUI(MainGameManager.instance.boardStatusResponse);
        }

        // 자동 턴 전환 감지
        bool isMyTurnNow = PlayerManager.instance.isMyTurn();

        if (!wasMyTurnLastFrame && isMyTurnNow)
        {
            OnMyTurnStarted();
        }
        else if (wasMyTurnLastFrame && !isMyTurnNow)
        {
            Debug.Log("내 턴이 종료되었습니다. 상대방을 기다립니다.");
            throwButton.gameObject.SetActive(false);
        }

        wasMyTurnLastFrame = isMyTurnNow;
    }

    public void RegisterPiece(PieceController piece)
    {
        if (!allPiecesDict.ContainsKey(piece.pieceId))
        {
            allPiecesDict.Add(piece.pieceId, piece);
        }
    }

    public void StartGameAfterInit(BoardStatusResponse initialState)
    {
        UpdateBoardUI(initialState);
    }

    // =========================================================
    // [최초 턴 시작] HallInfoResponse 기반
    // =========================================================
    private void OnMyTurnStarted()
    {
        Debug.Log("내 턴 시작! (Hall에서 정해진 윷 결과 확인)");

        var hallInfo = MainGameManager.instance.hallInfoResponse;
        string resultStr = CalculateYutResult(hallInfo.publicSticks, hallInfo.declaredPrivateSticks);

        if (throwResultText != null)
            throwResultText.text = $"현재 결과: {resultStr}";

        CheckMovablePieces();
    }

    private async void CheckMovablePieces()
    {
        await ServerManager.instance.MoveListRequest();
        var movablePieces = MainGameManager.instance.moveListResponse.movablePieces;

        if (movablePieces == null || movablePieces.Count == 0)
        {
            Debug.Log("움직일 수 있는 말이 없습니다. 턴을 종료합니다.");
            await ServerManager.instance.EndTurnRequest();
            return;
        }

        Debug.Log("움직일 말을 선택해주세요.");
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
        foreach (var piece in allPiecesDict.Values)
            piece.SetClickable(false);

        await ServerManager.instance.MovePieceRequest(pieceId);

        // ServerManager의 폴링 대기
        await Task.Delay(1000);

        var state = MainGameManager.instance.boardStatusResponse;
        UpdateBoardUI(state);

        Debug.Log("이동 연출 대기 중...");
        await Task.Delay(1500);

        if (state.extraTurn)
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
    private void UpdateBoardUI(BoardStatusResponse state)
    {
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

    // =======================================================
    // 헬퍼: 막대기로 도개걸윷모 계산 (TAIL = 평평한 면 기준)
    // =======================================================
    private string CalculateYutResult(StickSide?[] publicSticks, StickSide?[] privateSticks)
    {
        int flatCount = 0; // 평평한 면(TAIL, BACK)의 개수
        bool hasBackDo = false;

        List<StickSide?> allSticks = new List<StickSide?>();
        if (publicSticks != null) allSticks.AddRange(publicSticks);
        if (privateSticks != null) allSticks.AddRange(privateSticks);

        foreach (var stick in allSticks)
        {
            // TAIL과 BACK을 평평한 면(배)으로 취급합니다.
            if (stick == StickSide.TAIL) flatCount++;
            else if (stick == StickSide.BACK)
            {
                flatCount++;
                hasBackDo = true;
            }
        }

        if (flatCount == 1 && hasBackDo) return "빽도";
        if (flatCount == 1) return "도";
        if (flatCount == 2) return "개";
        if (flatCount == 3) return "걸";
        if (flatCount == 4) return "윷";
        if (flatCount == 0) return "모";

        return "결과 오류";
    }

    private string TranslateYutResult(string enumName)
    {
        switch (enumName.ToUpper())
        {
            case "DO": return "도";
            case "GAE": return "개";
            case "GEOL": return "걸";
            case "YUT": return "윷";
            case "MO": return "모";
            case "BACK_DO":
            case "BACKDO": return "빽도";
            default: return enumName;
        }
    }
}