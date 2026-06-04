using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeclareUIOnActive : MonoBehaviour
{
    public MainHallManager mainHallManager;

    private void OnEnable()
    {
        mainHallManager.declareSticks = MainGameManager.instance.game_stat.privateSticks;
        mainHallManager.all_result_text.text = MainGameManager.instance.game_stat.currentYutResult.result.ToString();
    }
}
