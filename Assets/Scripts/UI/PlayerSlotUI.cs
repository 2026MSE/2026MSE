using UnityEngine;
using TMPro;

public class PlayerSlotUI : MonoBehaviour
{
    public GameObject emptyVisual;
    public GameObject filledVisual;
    private TMP_Text playerNameText;

    private void Start()
    {
        playerNameText = filledVisual.GetComponentInChildren<TMP_Text>();
    }


    public void SetEmpty()
    {
        emptyVisual.SetActive(true);
        filledVisual.SetActive(false);
    }

    public void SetPlayer(string playerName)
    {
        emptyVisual.SetActive(false);
        filledVisual.SetActive(true);
        playerNameText.text = playerName;
    }
}