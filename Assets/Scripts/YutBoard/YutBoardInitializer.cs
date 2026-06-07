using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class YutBoardInitializer : MonoBehaviour
{
    [Header("Prefabs")]
    [Tooltip("최대 4명의 플레이어를 위한 4가지 색상의 프리팹을 순서대로 넣어주세요.")]
    public GameObject[] piecePrefabs;

    [Header("Piece들 묶는 오브젝트")]
    public Transform pieceContainer;

    MainGameManager main_game_manager;
    YutManager yut_manager;

    void Start()
    {
        yut_manager = YutManager.Instance;
        main_game_manager = MainGameManager.instance;
        StartGame().Forget();
    }

    private async UniTaskVoid StartGame()
    {
        while (main_game_manager == null ||
               main_game_manager.game_stat == null ||
               main_game_manager.game_stat.boardStatus == null ||
               main_game_manager.game_stat.boardStatus.allPieces == null ||
               main_game_manager.game_stat.roomInfo == null ||
               main_game_manager.game_stat.roomInfo.playerIds == null)
        {
            Debug.Log("보드 초기화 데이터 대기 중...");
            await UniTask.Delay(1000, cancellationToken: this.GetCancellationTokenOnDestroy());
        }

        SpawnPieces(main_game_manager.game_stat.boardStatus.allPieces);

        yut_manager.StartGame();
    }

    private void SpawnPieces(Dictionary<string, List<PieceInfo>> allPieces)
    {
        List<string> roomPlayerIds = main_game_manager.game_stat.roomInfo.playerIds;

        foreach (var kvp in allPieces)
        {
            string ownerId = kvp.Key;
            List<PieceInfo> pieces = kvp.Value;

            int playerIndex = roomPlayerIds.IndexOf(ownerId);

            if (playerIndex < 0 || playerIndex >= piecePrefabs.Length)
            {
                Debug.LogWarning($"플레이어 {ownerId}의 인덱스를 찾을 수 없거나 프리팹 개수를 초과했습니다. 0번 프리팹을 사용합니다.");
                playerIndex = 0;
            }

            GameObject prefabToUse = piecePrefabs[playerIndex];

            foreach (var pieceData in pieces)
            {
                GameObject newPiece = Instantiate(prefabToUse, Vector3.zero, Quaternion.identity, pieceContainer);
                newPiece.name = $"Piece_{pieceData.pieceId}";

                PieceController controller = newPiece.GetComponent<PieceController>();
                if (controller != null)
                {
                    controller.pieceId = pieceData.pieceId;
                    yut_manager.RegisterPiece(controller);
                }
                else
                {
                    Debug.LogWarning("PieceController 스크립트 null");
                }
            }
        }
    }
}