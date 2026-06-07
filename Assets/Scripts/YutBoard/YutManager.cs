using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class YutManager : MonoBehaviour
{
    public static YutManager Instance { get; private set; }

    [Header("프리팹")]
    public GameObject selectMove_prefab;

    [Header("보드판 노드")]
    public Transform[] boardNodes;

    [Header("대기, 도착 구역")]
    public Transform waitingArea;
    public Transform finishArea;

    [Header("UI 연결")]
    public Button throwButton;
    public Button turnEndButton;
    public TextMeshProUGUI throwResultText;
    public Button throw_exit_button;

    [Header("말 배치 수치값")]
    public float spacing = 1.2f;
    public int maxPerRow = 4;
    public int maxRows = 4;
    public float piggybackHeight = 0.5f;
    public float plateYOffset = 0.0f;

    [Header("보드판 말 정렬 설정")]
    public float spreadRadius = 0.3f;
    [Tooltip("체크 시 대기석과 보드판 위 말들이 모두 세로(Z축)로 통일되어 정렬됩니다. 해제 시 가로(X축)로 정렬됩니다.")]
    public bool isVerticalAlignment = true;

    private Dictionary<string, PieceController> allPiecesDict = new Dictionary<string, PieceController>();
    private List<GameObject> select_moves = new List<GameObject>();

    private MainGameManager main_game_manager;
    private ServerManager server_manager;
    private PlayerManager player_manager;

    private bool is_selecting = false;
    private TurnPhase _lastTurnPhase;

    private long _lastProcessedSequence = -1;
    private bool _isPlayingMoveAnimation = false;

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

        // UI 초기화
        throwButton.gameObject.SetActive(false);
        throw_exit_button.gameObject.SetActive(false);
        turnEndButton.gameObject.SetActive(false);

        throwButton.onClick.AddListener(OnThrowButtonClicked);
        throw_exit_button.onClick.AddListener(() => { server_manager.PrivateExitRequest().Forget(); throw_exit_button.gameObject.SetActive(false); });
        turnEndButton.onClick.AddListener(OnTurnEndButtonClicked);

        InitializeBoardState();
    }

    private void Update()
    {
        if (main_game_manager == null || main_game_manager.game_stat == null) return;

        if (!_isPlayingMoveAnimation && main_game_manager.game_stat.moveHistory != null)
        {
            var history = main_game_manager.game_stat.moveHistory;
            if (history.Count > 0 && history[history.Count - 1].moveSequence > _lastProcessedSequence)
            {
                ProcessMoveHistoryAsync().Forget();
                return;
            }
        }

        if (!_isPlayingMoveAnimation)
        {
            RefreshObserverUI();
            CheckPhaseTransitions();
        }
    }
    private void CheckPhaseTransitions()
    {
        TurnPhase currentPhase = main_game_manager.game_stat.turnPhase;
        if (_lastTurnPhase == currentPhase) return;

        bool isMyTurn = player_manager.isMyTurn();

        if (currentPhase == TurnPhase.YUT_MOVE || currentPhase == TurnPhase.YUT_MOVE_DONE)
        {
            if (isMyTurn) FetchAndRefreshMoveUIAsync().Forget();
            turnEndButton.gameObject.SetActive(isMyTurn && currentPhase == TurnPhase.YUT_MOVE_DONE);
            throwButton.gameObject.SetActive(false);
            throw_exit_button.gameObject.SetActive(false);
        }
        else if (currentPhase == TurnPhase.CATCH_BONUS_THROW)
        {
            CheckMovablePieces(false);
            throwButton.gameObject.SetActive(isMyTurn);
            turnEndButton.gameObject.SetActive(false);
            throw_exit_button.gameObject.SetActive(false);
        }
        _lastTurnPhase = currentPhase;
    }
    private void RefreshObserverUI()
    {
        if (player_manager.isMyTurn()) return;

        var state = main_game_manager.game_stat;
        if (state == null) return;

        if (throwResultText != null)
        {
            string tmp_string = "남은 이동 : ";

            if (state.pendingYutResults == null || state.pendingYutResults.Count == 0)
            {
                tmp_string += "없음";
            }
            else
            {
                foreach (var yut in state.pendingYutResults)
                {
                    tmp_string += TranslateYutResult(yut.result) + " ";
                }
            }
            throwResultText.text = tmp_string;
        }
    }
    private async UniTaskVoid ProcessMoveHistoryAsync()
    {
        _isPlayingMoveAnimation = true;
        var token = this.GetCancellationTokenOnDestroy();
        var history = main_game_manager.game_stat.moveHistory;

        foreach (var move in history)
        {
            if (move.moveSequence > _lastProcessedSequence)
            {
                await PlaySingleMoveAnimation(move, token);
                _lastProcessedSequence = move.moveSequence;
            }
        }

        RefreshBoardLayout();

        if (player_manager.isMyTurn() && main_game_manager.game_stat.turnPhase != TurnPhase.CATCH_BONUS_THROW)
        {
            await FetchAndRefreshMoveUIAsync();
        }

        _isPlayingMoveAnimation = false;
    }

    private async UniTask PlaySingleMoveAnimation(MoveResultResponse move, CancellationToken token)
    {
        List<UniTask> tasks = new List<UniTask>();

        if (move.movedPieceIds != null)
        {
            foreach (var pId in move.movedPieceIds)
            {
                if (allPiecesDict.TryGetValue(pId, out var piece))
                {
                    Vector3 targetPos;
                    if (move.moveType == MoveType.FINISH || move.toPosition == 99)
                        targetPos = finishArea.position;
                    else if (move.toPosition == -1)
                        targetPos = waitingArea.position;
                    else
                        targetPos = boardNodes[move.toPosition].position; // 노드 중앙으로 1차 이동

                    tasks.Add(piece.transform.DOMove(targetPos, 0.4f).SetEase(Ease.OutQuad).ToUniTask(cancellationToken: token));
                }
            }
        }

        await UniTask.WhenAll(tasks);
        tasks.Clear();

        if (move.moveType == MoveType.CATCH && move.caughtPieceIds != null)
        {
            foreach (var cId in move.caughtPieceIds)
            {
                if (allPiecesDict.TryGetValue(cId, out var piece))
                {
                    tasks.Add(piece.transform.DOMove(waitingArea.position, 0.4f).SetEase(Ease.OutBounce).ToUniTask(cancellationToken: token));
                }
            }
            await UniTask.WhenAll(tasks);
        }

        await UniTask.Delay(100, cancellationToken: token);
    }

    private void InitializeBoardState()
    {
        if (main_game_manager?.game_stat?.moveHistory != null)
        {
            var history = main_game_manager.game_stat.moveHistory;
            if (history.Count > 0) _lastProcessedSequence = history[history.Count - 1].moveSequence;
        }
        RefreshBoardLayout(true);
    }

    private void RefreshBoardLayout(bool isInstant = false)
    {
        var state = main_game_manager.game_stat.boardStatus;
        if (state?.allPieces == null) return;

        List<string> roomPlayerIds = main_game_manager.game_stat.roomInfo.playerIds;
        Dictionary<string, int> pWaitCnt = new Dictionary<string, int>();
        Dictionary<string, int> pFinCnt = new Dictionary<string, int>();
        Dictionary<int, List<PieceController>> nodePieces = new Dictionary<int, List<PieceController>>();

        float startX = -1.8f; float startZ = -1.8f;

        foreach (var kvp in state.allPieces)
        {
            string ownerId = kvp.Key;
            int pIdx = roomPlayerIds.IndexOf(ownerId);
            if (pIdx < 0) pIdx = 0;

            foreach (var pieceData in kvp.Value)
            {
                if (allPiecesDict.TryGetValue(pieceData.pieceId, out var pieceObj))
                {
                    int pos = pieceData.currentPosition;
                    if (pos == -1 || pos == 99)
                    {
                        var dict = (pos == -1) ? pWaitCnt : pFinCnt;
                        if (!dict.ContainsKey(ownerId)) dict[ownerId] = 0;
                        float offX = startX + (isVerticalAlignment ? pIdx * spacing : dict[ownerId] * spacing);
                        float offZ = startZ + (isVerticalAlignment ? dict[ownerId] * spacing : pIdx * spacing);
                        Vector3 target = (pos == -1 ? waitingArea.position : finishArea.position) + new Vector3(offX, plateYOffset, offZ);
                        MovePiece(pieceObj, target, isInstant);
                        dict[ownerId]++;
                    }
                    else
                    {
                        if (!nodePieces.ContainsKey(pos)) nodePieces[pos] = new List<PieceController>();
                        nodePieces[pos].Add(pieceObj);
                    }
                }
            }
        }

        foreach (var kvp in nodePieces)
        {
            int total = kvp.Value.Count;
            Vector3 center = boardNodes[kvp.Key].position + new Vector3(0, plateYOffset, 0);
            for (int i = 0; i < total; i++)
            {
                Vector3 off = Vector3.zero;
                if (total == 2) off = isVerticalAlignment ? (i == 0 ? new Vector3(0, 0, -spreadRadius) : new Vector3(0, 0, spreadRadius)) : (i == 0 ? new Vector3(-spreadRadius, 0, 0) : new Vector3(spreadRadius, 0, 0));
                else if (total == 3) off = isVerticalAlignment ? (i == 0 ? new Vector3(0, 0, spreadRadius) : (i == 1 ? new Vector3(-spreadRadius, 0, -spreadRadius) : new Vector3(spreadRadius, 0, -spreadRadius))) : (i == 0 ? new Vector3(spreadRadius, 0, 0) : (i == 1 ? new Vector3(-spreadRadius, 0, -spreadRadius) : new Vector3(-spreadRadius, 0, spreadRadius)));
                else if (total >= 4) off = new Vector3(i < 2 ? -spreadRadius : spreadRadius, 0, (i % 2 == 0) ? spreadRadius : -spreadRadius);
                MovePiece(kvp.Value[i], center + off, isInstant);
            }
        }
    }

    private void MovePiece(PieceController pieceObj, Vector3 targetPosition, bool isInstant)
    {
        if (isInstant) pieceObj.transform.position = targetPosition;
        else pieceObj.transform.DOMove(targetPosition, 0.2f).SetEase(Ease.OutQuad);
    }
    public void RegisterPiece(PieceController piece) => allPiecesDict[piece.pieceId] = piece;

    public void StartGame() => RefreshBoardLayout(true);

    private async UniTask FetchAndRefreshMoveUIAsync()
    {
        await server_manager.MoveListRequest();

        RefreshMoveListUI();

        CheckMovablePieces(true);
    }

    // 텍스트 UI만 갱신하는 함수
    private void RefreshMoveListUI()
    {
        var move_list = main_game_manager.moveListResponse;

        if (throwResultText != null)
        {
            string tmp_string = "남은 이동 : ";
            if (move_list == null || move_list.moveGroups == null || move_list.moveGroups.Count == 0)
            {
                tmp_string += "없음";
            }
            else
            {
                foreach (var move in move_list.moveGroups)
                {
                    tmp_string += TranslateYutResult(move.yutName) + " ";
                }
            }
            throwResultText.text = tmp_string;
        }
    }
    private void CheckMovablePieces(bool switch_on = true)
    {
        is_selecting = false;
        select_moves.ForEach(move => Destroy(move));
        select_moves.Clear();

        foreach (var kvp in allPiecesDict)
        {
            kvp.Value.transform.localScale = Vector3.one * 1.5f;
            kvp.Value.SetClickable(false);
        }

        if (!switch_on) return;

        var move_list = main_game_manager.moveListResponse;
        if (move_list == null || move_list.moveGroups == null || move_list.moveGroups.Count <= 0) return;

        var targetGroup = move_list.moveGroups[0];
        if (targetGroup == null || targetGroup.movablePieces == null) return;

        foreach (var moveOption in targetGroup.movablePieces)
        {
            if (allPiecesDict.TryGetValue(moveOption.pieceId, out PieceController pieceObj))
            {
                pieceObj.SetClickable(true);
            }
        }
    }

    public void OnPieceSelected(string pieceId)
    {
        if (is_selecting)
        {
            if (select_moves.Count >= 1 && select_moves[0].GetComponent<MoveSelect>().pieceId == pieceId)
            {
                select_moves.ForEach(move => Destroy(move));
                select_moves.Clear();
                is_selecting = false;

                var tmp1 = allPiecesDict.TryGetValue(pieceId, out PieceController piece1);
                piece1.transform.localScale = Vector3.one * 1.5f;
                return;
            }
        }
        else
        {
            is_selecting = true;
        }

        select_moves.ForEach(move => Destroy(move));
        select_moves.Clear();

        foreach (var kvp in allPiecesDict)
        {
            kvp.Value.transform.localScale = Vector3.one * 1.5f;
        }

        var tmp = allPiecesDict.TryGetValue(pieceId, out PieceController piece);
        piece.transform.localScale = Vector3.one * 2f;

        List<MoveGroup> moveGroups = main_game_manager.moveListResponse.moveGroups;
        foreach (var moveGroup in moveGroups)
        {
            foreach (var move in moveGroup.movablePieces)
            {
                if (move.pieceId == pieceId)
                {
                    var tmp_obj = Instantiate(selectMove_prefab, boardNodes[move.targetPosition]);
                    select_moves.Add(tmp_obj);

                    tmp_obj.GetComponent<MoveSelect>().this_move = moveGroup;
                    tmp_obj.GetComponent<MoveSelect>().pieceId = pieceId;
                }
            }
        }
    }

    public async void OnClickMove(MoveGroup move, string pieceId)
    {
        foreach (var kvp in allPiecesDict)
        {
            kvp.Value.transform.localScale = Vector3.one * 1.5f;
            kvp.Value.SetClickable(false);
        }

        is_selecting = false;
        select_moves.ForEach(m => Destroy(m));
        select_moves.Clear();

        await server_manager.MovePieceRequest(pieceId, move.yutResultIndex);
    }
    private async void OnThrowButtonClicked()
    {
        throwButton.interactable = false;

        ServerManager.instance.YutRequest().Forget();

        var token = this.GetCancellationTokenOnDestroy();
        await UniTask.WaitUntil(() => main_game_manager.throwResponse != null, cancellationToken: token);

        string throwResultStr = TranslateYutResult(main_game_manager.throwResponse.yutResult.result);

        if (throwResultText != null)
        {
            throwResultText.text = $"결과: {throwResultStr}";
        }

        throwButton.gameObject.SetActive(false);

        if (player_manager.isMyTurn())
        {
            throw_exit_button.gameObject.SetActive(true);
        }

        throwButton.interactable = true;
        CheckMovablePieces();
    }

    private void OnTurnEndButtonClicked()
    {
        turnEndButton.gameObject.SetActive(false);
        ServerManager.instance.EndTurnRequest().Forget();
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