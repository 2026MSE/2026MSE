using TMPro;
using UnityEngine;

public class PlayerSlotUI : MonoBehaviour
{
    [Header("Slot Visual")]
    public GameObject emptyVisual;
    public GameObject filledVisual;

    [Header("Text")]
    public TMP_Text playerNameText;

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        ResolveReferences();
    }

    private void ResolveReferences()
    {
        if (playerNameText == null && filledVisual != null)
        {
            playerNameText = filledVisual.GetComponentInChildren<TMP_Text>(true);
        }
    }

    public void SetEmpty()
    {
        if (emptyVisual != null)
            emptyVisual.SetActive(true);

        if (filledVisual != null)
            filledVisual.SetActive(false);

        if (playerNameText != null)
            playerNameText.text = "";
    }

    public void SetPlayer(string name)
    {
        if (emptyVisual != null)
            emptyVisual.SetActive(false);

        if (filledVisual != null)
            filledVisual.SetActive(true);

        if (playerNameText != null)
            playerNameText.text = string.IsNullOrEmpty(name) ? "Unknown" : name;
    }
}