using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MainGameUI : MonoBehaviour
{
    private TextMeshProUGUI scene_text;
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        scene_text = GetComponentInChildren<TextMeshProUGUI>();
    }

    void Update()
    {
        GetComponent<Canvas>().worldCamera = Camera.main;
        scene_text.text = "Now Scene : " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
    }
}
