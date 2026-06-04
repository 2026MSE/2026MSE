using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSelect : MonoBehaviour
{
    public MoveGroup this_move;
    public string pieceId;

    private void OnMouseDown()
    {
        YutManager.Instance.OnClickMove(this_move, pieceId);
    }
}
