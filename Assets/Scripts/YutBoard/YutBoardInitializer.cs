using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class YutBoardInitializer : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject myPiecePrefab;
    public GameObject opponentPiecePrefab;

    [Header("Piece들 묶는 오브젝트")]
    public Transform pieceContainer;

    MainGameManager main_game_manager;
    YutManager yut_manager;

    void Start()
    {
        yut_manager = YutManager.Instance;
        main_game_manager = MainGameManager.instance;
        StartGame();
    }

    private async void StartGame()
    {
        var state = main_game_manager.game_stat.boardStatus;

        while(state != null && state.allPieces != null)
        {
            Debug.LogError("보드 불러오기 에러");
            await UniTask.Delay(1000);
        }

        SpawnPieces(state.allPieces);

        yut_manager.StartGame();
    }

    private void SpawnPieces(Dictionary<string, List<PieceInfo>> allPieces)
    {
        string myPlayerId = PlayerManager.instance.this_player.id;

        foreach (var kvp in allPieces)
        {
            string ownerId = kvp.Key;
            List<PieceInfo> pieces = kvp.Value;

            GameObject prefabToUse = (ownerId == myPlayerId) ? myPiecePrefab : opponentPiecePrefab;

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