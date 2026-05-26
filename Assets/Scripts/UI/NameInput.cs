using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NameInput : MonoBehaviour
{
    public Button button;
    public TMP_InputField inputField;

    private void OnEnable()
    {
        button.onClick.AddListener(OnClick);
    }

    void Update()
    {
        if(PlayerManager.instance.this_player != null)
        {
            gameObject.SetActive(false);
        }
    }
    
    public void OnClick()
    {
        string name = inputField.text;
        PlayerManager.instance.createPlayer(name);
    }
}
