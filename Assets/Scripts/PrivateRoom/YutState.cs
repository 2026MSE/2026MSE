using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YutState : MonoBehaviour
{
    private StickSide? previousSide;
    public StickSide? stickSide;
    public GameObject tail_object;
    public GameObject head_object;
    public GameObject back_object;

    private void Start()
    {
        sideUpdate();
    }

    void Update()
    {
        if(previousSide != stickSide)
        {
            sideUpdate();
        }
    }
    void sideUpdate()
    {
        if (stickSide == StickSide.HEAD)
        {
            head_object.SetActive(false);
            tail_object.SetActive(true);
            back_object.SetActive(false);
        }
        else if (stickSide == StickSide.BACK)
        {
            head_object.SetActive(false);
            tail_object.SetActive(false);
            back_object.SetActive(true);
        }
        else
        {
            head_object.SetActive(true);
            tail_object.SetActive(false);
            back_object.SetActive(false);
        }
        previousSide = stickSide;
    }
}
