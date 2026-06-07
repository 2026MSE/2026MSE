using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("회전 감도")]
    public float mouseSensitivity = 300f;

    [Header("플레이어 몸체 (좌우 회전용)")]
    public Transform playerBody;

    public MeshRenderer player_renderer;
    private Texture2D player_icon;

    private float xRotation = 0f;
    private bool isMouseLocked = true;
    public bool is_local_player { get; set; } = false;
    public PlayerInfo this_player;
    public Camera player_camera;
    private Material my_material;

    private async void Start()
    {
        LockCursor();

        Material my_material = new Material(player_renderer.material);

        await UniTask.WaitUntil(() => !string.IsNullOrWhiteSpace(this_player.profileUrl));

        player_icon = await ServerManager.instance.TextureRequest(this_player.profileUrl);

        my_material.mainTexture = player_icon;
        player_renderer.material = my_material;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
        {
            isMouseLocked = !isMouseLocked;

            if (isMouseLocked)
            {
                LockCursor();
            }
            else
            {
                UnlockCursor();
            }
        }
        if (isMouseLocked)
        {
            CameraEnable();
        }
    }
    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void CameraEnable()
    {
        if (is_local_player)
        {
            player_camera.tag = "MainCamera";
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;

            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            player_camera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            playerBody.Rotate(Vector3.up * mouseX);
        }
        else
        {
            player_camera.gameObject.SetActive(false);
        }
    }
}
