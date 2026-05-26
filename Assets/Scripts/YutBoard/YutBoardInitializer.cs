using System.Collections.Generic;
using UnityEngine;

public class YutBoardInitializer : MonoBehaviour
{
    [Header("윷말 프리팹 설정")]
    public GameObject myPiecePrefab;       // 내 윷말 프리팹 (파란색 등)
    public GameObject opponentPiecePrefab; // 상대방 윷말 프리팹 (빨간색 등)

    [Header("생성될 말들을 묶어둘 부모 폴더 (선택사항)")]
    public Transform pieceContainer;

    private async void Start()
    {
        Debug.Log("윷놀이 씬 초기화 시작...");
        await ServerManager.instance.BoardStateRequest();
        var state = MainGameManager.instance.boardStatusResponse;

        if (state == null || state.allPieces == null)
        {
            Debug.LogError("보드 상태를 불러오지 못했습니다. 서버 상태를 확인하세요.");
            return;
        }

        // 2. 보드 상태를 바탕으로 윷말들을 맵에 생성합니다.
        SpawnPieces(state.allPieces);

        // 3. 생성이 끝났으면 YutManager에게 "이제 턴 확인하고 게임을 시작해!" 라고 알립니다.
        YutManager.Instance.StartGameAfterInit(state);
    }

    private void SpawnPieces(Dictionary<string, List<Piece>> allPieces)
    {
        string myPlayerId = PlayerManager.instance.this_player.id;

        foreach (var kvp in allPieces)
        {
            string ownerId = kvp.Key;
            List<Piece> pieces = kvp.Value;

            // 내 말인지 상대 말인지에 따라 프리팹 다르게 선택
            GameObject prefabToUse = (ownerId == myPlayerId) ? myPiecePrefab : opponentPiecePrefab;

            foreach (var pieceData in pieces)
            {
                // 말 프리팹 생성
                GameObject newPiece = Instantiate(prefabToUse, Vector3.zero, Quaternion.identity, pieceContainer);
                newPiece.name = $"Piece_{pieceData.id}"; // 하이어라키에서 보기 좋게 이름 변경

                // PieceController에 서버 ID 부여
                PieceController controller = newPiece.GetComponent<PieceController>();
                if (controller != null)
                {
                    controller.pieceId = pieceData.id;

                    // 생성된 말을 YutManager의 딕셔너리에 수동 등록
                    YutManager.Instance.RegisterPiece(controller);
                }
                else
                {
                    Debug.LogWarning("윷말 프리팹에 PieceController 스크립트가 없습니다!");
                }
            }
        }
        Debug.Log("윷말 생성 완료!");
    }
}