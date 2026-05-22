using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NameInput : MonoBehaviour
{
    [Header("UI")]
    public Button button;
    public TMP_InputField inputField;

    [Header("Option")]
    public string defaultStyle = "adventurer";

    private bool isRequesting = false;

    private void OnEnable()
    {
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }

        isRequesting = false;

        if (button != null)
            button.interactable = true;
    }

    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(OnClick);
    }

    private void Update()
    {
        if (PlayerManager.instance != null &&
            PlayerManager.instance.this_player != null &&
            !string.IsNullOrEmpty(PlayerManager.instance.this_player.id))
        {
            gameObject.SetActive(false);
        }
    }

    public void OnClick()
    {
        if (isRequesting)
            return;

        if (inputField == null)
        {
            Debug.LogWarning("[NameInput] inputField가 연결되어 있지 않습니다.");
            return;
        }

        string name = inputField.text.Trim();

        if (string.IsNullOrEmpty(name))
        {
            Debug.LogWarning("[NameInput] 이름을 입력하세요.");
            return;
        }

        if (PlayerManager.instance == null)
        {
            Debug.LogWarning("[NameInput] PlayerManager.instance가 없습니다.");
            return;
        }

        isRequesting = true;

        if (button != null)
            button.interactable = false;

        PlayerManager.instance.createPlayer(name, defaultStyle);
    }
}