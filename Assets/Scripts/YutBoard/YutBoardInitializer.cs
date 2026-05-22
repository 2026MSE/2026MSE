using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YutBoardInitializer : MonoBehaviour
{
    [Header("윷말 프리팹 설정")]
    public GameObject myPiecePrefab;
    public GameObject opponentPiecePrefab;

    [Header("생성될 말들을 묶어둘 부모 폴더")]
    public Transform pieceContainer;

    [Header("초기화 설정")]
    public float waitInterval = 0.2f;
    public float maxWaitTime = 10f;

    private bool initialized = false;

    private IEnumerator Start()
    {
        Debug.Log("[YutBoardInitializer] 윷놀이 보드 초기화 대기 시작");

        float elapsed = 0f;

        while (elapsed < maxWaitTime)
        {
            if (CanInitialize())
            {
                InitializeBoard();
                yield break;
            }

            elapsed += waitInterval;
            yield return new WaitForSeconds(waitInterval);
        }

        Debug.LogError("[YutBoardInitializer] 보드 상태를 불러오지 못했습니다. /game/state polling과 MainGameManager 상태를 확인하세요.");
    }

    private bool CanInitialize()
    {
        if (initialized) return false;

        if (MainGameManager.instance == null) return false;
        if (PlayerManager.instance == null) return false;
        if (PlayerManager.instance.this_player == null) return false;

        BoardStatusResponse state = MainGameManager.instance.boardStatusResponse;

        if (state == null) return false;
        if (state.allPieces == null) return false;

        return true;
    }

    private void InitializeBoard()
    {
        initialized = true;

        BoardStatusResponse state = MainGameManager.instance.boardStatusResponse;

        SpawnPieces(state.allPieces);

        if (YutManager.Instance != null)
        {
            YutManager.Instance.StartGameAfterInit(state);
        }
        else
        {
            Debug.LogWarning("[YutBoardInitializer] YutManager.Instance가 없습니다.");
        }

        Debug.Log("[YutBoardInitializer] 윷말 생성 및 보드 초기화 완료");
    }

    private void SpawnPieces(Dictionary<string, List<Piece>> allPieces)
    {
        string myPlayerId = PlayerManager.instance.this_player.id;

        foreach (var kvp in allPieces)
        {
            string ownerId = kvp.Key;
            List<Piece> pieces = kvp.Value;

            if (pieces == null) continue;

            GameObject prefabToUse = ownerId == myPlayerId
                ? myPiecePrefab
                : opponentPiecePrefab;

            if (prefabToUse == null)
            {
                Debug.LogError($"[YutBoardInitializer] 사용할 말 프리팹이 없습니다. ownerId={ownerId}");
                continue;
            }

            foreach (Piece pieceData in pieces)
            {
                if (pieceData == null) continue;

                GameObject newPiece = Instantiate(
                    prefabToUse,
                    Vector3.zero,
                    Quaternion.identity,
                    pieceContainer
                );

                newPiece.name = $"Piece_{pieceData.id}";

                PieceController controller = newPiece.GetComponent<PieceController>();

                if (controller == null)
                {
                    Debug.LogWarning("[YutBoardInitializer] 윷말 프리팹에 PieceController가 없습니다.");
                    continue;
                }

                controller.pieceId = pieceData.id;

                if (YutManager.Instance != null)
                {
                    YutManager.Instance.RegisterPiece(controller);
                }
            }
        }
    }
}