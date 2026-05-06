using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NameInput : MonoBehaviour
{
    
    void Start()
    {
        GetComponentInParent<Button>().onClick.AddListener(OnClick);
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
        string name = GetComponentInParent<TMP_InputField>().text;
        PlayerManager.instance.createPlayer(name);
    }
}
