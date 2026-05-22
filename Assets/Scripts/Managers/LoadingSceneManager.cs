using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingSceneManager : MonoBehaviour
{
    [Header("Loading Target")]
    public string nextSceneName;

    [Header("Loading UI")]
    public Slider loadingBar;
    public TextMeshProUGUI progressText;

    [Header("Option")]
    public float minimumLoadingTime = 0.5f;

    private ClientSceneLoadMode loadMode = ClientSceneLoadMode.Single;

    private void Start()
    {
        StartCoroutine(LoadNextSceneRoutine());
    }

    private IEnumerator LoadNextSceneRoutine()
    {
        ResolveNextSceneInfo();

        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("[LoadingSceneManager] nextSceneName이 비어 있어서 MainHall로 이동합니다.");
            nextSceneName = "MainHall";
            loadMode = ClientSceneLoadMode.Single;
        }

        LoadSceneMode unityLoadMode =
            loadMode == ClientSceneLoadMode.Additive
                ? LoadSceneMode.Additive
                : LoadSceneMode.Single;

        Scene alreadyLoadedScene = SceneManager.GetSceneByName(nextSceneName);

        if (loadMode == ClientSceneLoadMode.Additive && alreadyLoadedScene.isLoaded)
        {
            SceneManager.SetActiveScene(alreadyLoadedScene);
            yield break;
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(nextSceneName, unityLoadMode);
        op.allowSceneActivation = false;

        float timer = 0f;

        while (!op.isDone)
        {
            timer += Time.deltaTime;

            float realProgress = Mathf.Clamp01(op.progress / 0.9f);
            float timeProgress = Mathf.Clamp01(timer / minimumLoadingTime);
            float displayProgress = Mathf.Min(realProgress, timeProgress);

            if (loadingBar != null)
                loadingBar.value = displayProgress;

            if (progressText != null)
                progressText.text = (displayProgress * 100f).ToString("F0") + "%";

            if (op.progress >= 0.9f && timer >= minimumLoadingTime)
            {
                if (loadingBar != null)
                    loadingBar.value = 1f;

                if (progressText != null)
                    progressText.text = "100%";

                op.allowSceneActivation = true;
            }

            yield return null;
        }

        if (loadMode == ClientSceneLoadMode.Additive)
        {
            Scene loadedScene = SceneManager.GetSceneByName(nextSceneName);

            if (loadedScene.IsValid() && loadedScene.isLoaded)
            {
                SceneManager.SetActiveScene(loadedScene);
            }
        }
    }

    private void ResolveNextSceneInfo()
    {
        if (MainGameManager.instance == null)
        {
            Debug.LogWarning("[LoadingSceneManager] MainGameManager.instance가 없습니다.");
            return;
        }

        loadMode = MainGameManager.instance.nextSceneLoadMode;

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            return;
        }

        nextSceneName = MainGameManager.instance.GetGotoSceneName();
    }
}