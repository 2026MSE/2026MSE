using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public bool is_local_player { get; set; } = false;
    public Camera player_camera;

    private void Update()
    {
        if (is_local_player)
        {
            CameraEnable();
        }
    }
    public void CameraEnable()
    {
        if (is_local_player)
        {
            player_camera.enabled = true;
        }
    }
}
