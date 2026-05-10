using UnityEngine;

public class PieceController : MonoBehaviour
{
    public string pieceId; // "player1_piece_1" 등 서버와 동일한 ID 매칭 필요
    private bool isClickable = false;

    // 시각적 피드백을 위한 외곽선이나 파티클 등을 연결
    public GameObject highlightEffect;

    private void Start()
    {
        // 씬이 시작되면 YutManager의 딕셔너리에 자신을 등록합니다.
        YutManager.Instance.RegisterPiece(this);
    }

    public void SetClickable(bool clickable)
    {
        isClickable = clickable;

        // 클릭 가능 상태에 따라 하이라이트 이펙트 켜기/끄기
        if (highlightEffect != null)
        {
            highlightEffect.SetActive(clickable);
        }
    }

    // 3D 오브젝트에 Collider가 붙어있어야 작동합니다.
    // 2D UI(Image)라면 OnMouseDown 대신 IPointerClickHandler 등을 사용해야 합니다.
    private void OnMouseDown()
    {
        if (!isClickable) return;

        // 클릭 시 YutManager로 자신의 ID를 넘기며 선택되었음을 알림
        YutManager.Instance.OnPieceSelected(pieceId);
    }
}