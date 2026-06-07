using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerSlotUI : MonoBehaviour
{
    public GameObject emptyVisual;
    public GameObject filledVisual;
    public Image player_icon;
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

    public async void SetPlayer(string name, string url)
    {
        emptyVisual.SetActive(false);
        filledVisual.SetActive(true);
        playerNameText.text = name;
        Texture2D profile = await ServerManager.instance.TextureRequest(url);
        player_icon.sprite = Sprite.Create(
                profile,
                new Rect(0, 0, profile.width, profile.height),
                new Vector2(0.5f, 0.5f)
            );
    }
}