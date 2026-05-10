using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("회전 감도")]
    public float mouseSensitivity = 300f;

    [Header("플레이어 몸체 (좌우 회전용)")]
    public Transform playerBody;

    // 상하 회전값을 저장할 변수
    private float xRotation = 0f;
    // 현재 마우스가 잠겨있는지(카메라 회전 모드인지) 확인하는 변수
    private bool isMouseLocked = true;
    public bool is_local_player { get; set; } = false;
    public Camera player_camera;

    private void Start()
    {
        LockCursor();
    }
    private void Update()
    {
        // 1. 왼쪽 또는 오른쪽 Ctrl 키를 눌렀을 때 상태를 전환(토글)합니다.
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
        {
            isMouseLocked = !isMouseLocked; // 상태 반전 (true -> false, false -> true)

            if (isMouseLocked)
            {
                LockCursor(); // 마우스 숨기고 카메라 회전 모드로
            }
            else
            {
                UnlockCursor(); // 마우스 보이게 하고 UI 클릭 모드로
            }
        }
        if (isMouseLocked)
        {
            CameraEnable();
        }
    }
    // 마우스를 화면 중앙에 고정하고 숨기는 함수
    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // 마우스 고정을 풀고 화면에 표시하는 함수
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
            // 1. 마우스 이동량 가져오기 (Time.deltaTime을 곱해 프레임 저하 시에도 일정한 속도 유지)
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            // 2. 상하(위아래) 회전 계산
            // 마우스를 위로 올리면 mouseY 양수 -> 카메라 X축 회전은 음수가 되어야 위를 봄
            xRotation -= mouseY;

            // 고개가 뒤로 꺾이지 않도록 위아래 회전 각도를 -90도 ~ 90도로 제한(Clamp)
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            // 3. 카메라에 상하 회전 적용
            player_camera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            // 4. 플레이어 몸체에 좌우 회전 적용 (Y축 기준 회전)
            playerBody.Rotate(Vector3.up * mouseX);
        }
        else
        {
            player_camera.gameObject.SetActive(false);
        }
    }
}
