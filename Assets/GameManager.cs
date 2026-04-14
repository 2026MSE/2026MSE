using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }
    void Start()
    {
        SceneManager.LoadScene("MainGameUI", LoadSceneMode.Additive);
    }

    void Update()
    {
        
    }
}
