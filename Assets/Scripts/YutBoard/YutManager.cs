using System.Collections.Generic;
using System.Threading.Tasks;
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

    private Dictionary<string, PieceController> allPiecesDict = new Dictionary<string, PieceController>();
    private List<GameObject> select_moves = new List<GameObject>();

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
        throw_exit_button.onClick.AddListener(() => { server_manager.PrivateExitRequest().Forget(); throw_exit_button.gameObject.SetActive(false); });
        turnEndButton.onClick.AddListener(OnTurnEndButtonClicked);
        is_selecting = false;
        MoveListUIUpdate().Forget();
        if (player_manager.isMyTurn())
        {
            MyTurnStart();
        }
    }

    private void Update()
    {
        if (main_game_manager.game_stat.boardStatus.allPieces.Count <= 0) return;

        if(main_game_manager.game_stat.turnPhase == TurnPhase.YUT_MOVE)
        {
            UpdateBoardUI();
        }
        if (main_game_manager.game_stat.turnPhase == TurnPhase.YUT_MOVE_DONE)
        {
            UpdateBoardUI();
            CheckMovablePieces();
            turnEndButton.gameObject.SetActive(true);
            throwButton.gameObject.SetActive(false);
        }
        else if(main_game_manager.game_stat.turnPhase == TurnPhase.CATCH_BONUS_THROW)
        {
            CheckMovablePieces(false);
            throwButton.gameObject.SetActive(true);
            turnEndButton.gameObject.SetActive(false);
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
        CheckMovablePieces();
    }

    private async void CheckMovablePieces(bool switch_on = true)
    {
        if(!switch_on)
        {
            foreach (var kvp in allPiecesDict)
            {
                kvp.Value.SetClickable(false);
            }
            return;
        }

        await UniTask.WaitUntil(() => main_game_manager != null && main_game_manager.moveListResponse != null,
            cancellationToken: this.GetCancellationTokenOnDestroy());

        if (main_game_manager.moveListResponse == null)
        {
            Debug.Log("이동 가능한 말이 없습니다. (Response Null)");
            return;
        }

        await UniTask.WaitUntil(() => main_game_manager.moveListResponse != null && main_game_manager.moveListResponse.moveGroups != null,
            cancellationToken: this.GetCancellationTokenOnDestroy());

        if (main_game_manager.moveListResponse == null ||
            main_game_manager.moveListResponse.moveGroups == null ||
            main_game_manager.moveListResponse.moveGroups.Count <= 0)
        {
            Debug.Log("이동 가능한 말이 없습니다. (Groups Empty)");
            return;
        }

        var targetGroup = main_game_manager.moveListResponse.moveGroups[0];
        if (targetGroup == null || targetGroup.movablePieces == null) return;

        var movablePieces = targetGroup.movablePieces;

        foreach (var moveOption in movablePieces)
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

        foreach(var kvp in allPiecesDict)
        {
            kvp.Value.transform.localScale = Vector3.one * 1.5f;
        }

        var tmp = allPiecesDict.TryGetValue(pieceId, out PieceController piece);
        piece.transform.localScale = Vector3.one * 2f;

        List<MoveGroup> moveGroups = main_game_manager.moveListResponse.moveGroups;
        foreach (var moveGroup in moveGroups)
        {
            foreach(var move in moveGroup.movablePieces)
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
    // 이동 명령을 했을 때
    public async void OnClickMove(MoveGroup move, string pieceId)
    {
        foreach (var kvp in allPiecesDict)
        {
            kvp.Value.transform.localScale = Vector3.one * 1.5f;
            kvp.Value.SetClickable(false);
        }

        is_selecting = false;
        select_moves.ForEach(move => Destroy(move));
        select_moves.Clear();
        await server_manager.MovePieceRequest(pieceId, move.yutResultIndex);

        CheckMovablePieces();
    }

    // 추가턴 시에 던지는 버튼 눌렀을 때
    private async void OnThrowButtonClicked()
    {
        throwButton.interactable = false;

        ServerManager.instance.YutRequest().Forget();

        await UniTask.WaitUntil(() => main_game_manager.throwResponse != null);

        string throwResultStr = TranslateYutResult(main_game_manager.throwResponse.yutResult.result);
        
        // movelist 수정 후 고쳐야함.
        //throwResultText.text += $" {throwResultStr}";

        if (throwResultText != null)
        {
            throwResultText.text = $"결과: {throwResultStr}";
        }

        throwButton.gameObject.SetActive(false);
        throw_exit_button.gameObject.SetActive(true);
        CheckMovablePieces();
    }

    private void OnTurnEndButtonClicked()
    {
        turnEndButton.gameObject.SetActive(false);
        ServerManager.instance.EndTurnRequest().Forget();
    }

    private async UniTaskVoid MoveListUIUpdate()
    {
        while(main_game_manager.game_stat.turnPhase != TurnPhase.YUT_MOVE_DONE)
        {
            await server_manager.MoveListRequest();

            var move_list = main_game_manager.moveListResponse;

            if (throwResultText != null)
            {
                string tmp_string = "남은 이동 : ";
                if(move_list == null)
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
    }

    // 보드 UI 업데이트
    private async void UpdateBoardUI()
    {
        await UniTask.WaitUntil(() => main_game_manager.game_stat.boardStatus != null);

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
                if (allPiecesDict.TryGetValue(pieceData.pieceId, out PieceController pieceObj))
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