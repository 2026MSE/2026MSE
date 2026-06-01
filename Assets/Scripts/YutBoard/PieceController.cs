//using UnityEngine;

//public class PieceController : MonoBehaviour
//{
//    public string pieceId;
//    private bool isClickable = false;

//    public GameObject highlightEffect;

//    private void Start()
//    {
//        YutManager.Instance.RegisterPiece(this);
//    }

//    public void SetClickable(bool clickable)
//    {
//        isClickable = clickable;

//        if (highlightEffect != null)
//        {
//            highlightEffect.SetActive(clickable);
//        }
//    }

//    private void OnMouseDown()
//    {
//        if (!isClickable) return;

//        YutManager.Instance.OnPieceSelected(pieceId);
//    }
//}