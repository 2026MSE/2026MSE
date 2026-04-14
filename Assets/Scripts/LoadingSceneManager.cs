using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingSceneManager : MonoBehaviour
{
    public string nextSceneName; // 이동할 다음 씬의 이름
    public Slider loadingBar;    // 로딩 진행도를 보여줄 UI 슬라이더
    public TextMeshProUGUI progressText;    // "50%" 처럼 보여줄 텍스트

    void Start()
    {
        // 로딩 씬이 시작되자마자 다음 씬을 비동기로 불러오는 코루틴 실행
        StartCoroutine(LoadNextSceneRoutine());
    }

    IEnumerator LoadNextSceneRoutine()
    {
        // 1. 비동기 로딩 시작
        AsyncOperation op = SceneManager.LoadSceneAsync(nextSceneName);

        // 로딩이 100% 완료되어도 자동으로 씬을 넘기지 않도록 설정 (선택 사항)
        op.allowSceneActivation = false;

        float timer = 0.0f; // 페이크 로딩이나 부드러운 UI 처리를 위한 타이머

        // 2. 로딩이 완료될 때까지 반복
        while (!op.isDone)
        {
            yield return null; // 1프레임 대기
            timer += Time.deltaTime;

            // op.progress는 0.0 부터 0.9까지만 오름 (0.9가 로딩 완료, 1.0은 씬 활성화)
            if (op.progress < 0.9f)
            {
                loadingBar.value = Mathf.Lerp(loadingBar.value, op.progress, timer);
                if (loadingBar.value >= op.progress)
                {
                    timer = 0f;
                }
            }
            else // 로딩이 실제로는 다 끝난 상태 (0.9)
            {
                // UI 바를 1.0(100%)까지 부드럽게 채움
                loadingBar.value = Mathf.Lerp(loadingBar.value, 1f, timer);

                // 로딩 바가 완전히 다 찼다면
                if (loadingBar.value == 1.0f)
                {
                    // "아무 키나 누르세요" 텍스트를 띄우거나 바로 넘어가게 할 수 있습니다.
                    // 여기서는 바로 넘어가도록 설정
                    op.allowSceneActivation = true;
                }
            }

            // 텍스트 퍼센트 업데이트
            progressText.text = (loadingBar.value * 100f).ToString("F0") + "%";
        }
    }
}