using UnityEngine;
using TMPro;

public class PlayerSlotUI : MonoBehaviour
{
    public GameObject emptyVisual;
    public GameObject filledVisual;
    private TMP_Text playerNameText;

    private void OnEnable()
    {
        playerNameText = filledVisual.GetComponentInChildren<TMP_Text>();
    }

    public void SetEmpty()
    {
        emptyVisual.SetActive(true);
        filledVisual.SetActive(false);
    }

    public void SetPlayer(string name)
    {
        emptyVisual.SetActive(false);
        filledVisual.SetActive(true);
        playerNameText.text = name;
    }
}