using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainButtonUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Button Target")]
    public ClientScene gotoSceneName = ClientScene.TITLE;

    [Header("Scene Load Option")]
    public ClientSceneLoadMode loadMode = ClientSceneLoadMode.Single;

    [Tooltip("Additive 로딩이면 보통 LoadingScene을 사용하지 않는 것을 추천")]
    public bool useLoadingScene = false;

    public string loadingSceneName = "LoadingScene";

    [Header("Direct Scene Name")]
    public string directSceneName = "";

    [Header("Visual Effect")]
    public Renderer targetRenderer;
    public float targetIntensity = 0.1f;

    private Material targetMaterial;
    private int intensityID;
    private float currentIntensity = 0f;
    private bool isLoading = false;

    private void Start()
    {
        if (targetRenderer != null)
        {
            targetMaterial = targetRenderer.material;
            intensityID = Shader.PropertyToID("_GlitchIntensity");

            if (targetMaterial.HasProperty(intensityID))
            {
                currentIntensity = targetMaterial.GetFloat(intensityID);
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isLoading)
            return;

        if (MainGameManager.instance == null)
        {
            Debug.LogWarning("[MainButtonUI] MainGameManager.instance가 없습니다.");
            return;
        }

        if (gotoSceneName == ClientScene.EXIT)
        {
            QuitGame();
            return;
        }

        string targetScene = ResolveTargetSceneName();

        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogWarning($"[MainButtonUI] 이동할 씬 이름을 찾지 못했습니다. gotoSceneName={gotoSceneName}");
            return;
        }

        MainGameManager.instance.currentClientScene = gotoSceneName;
        MainGameManager.instance.nextSceneName = targetScene;
        MainGameManager.instance.nextSceneLoadMode = loadMode;

        if (useLoadingScene)
        {
            SceneManager.LoadScene(loadingSceneName, LoadSceneMode.Single);
        }
        else
        {
            StartCoroutine(LoadSceneRoutine(targetScene, loadMode));
        }
    }

    private IEnumerator LoadSceneRoutine(string sceneName, ClientSceneLoadMode mode)
    {
        isLoading = true;

        LoadSceneMode unityLoadMode =
            mode == ClientSceneLoadMode.Additive
                ? LoadSceneMode.Additive
                : LoadSceneMode.Single;

        Scene alreadyLoadedScene = SceneManager.GetSceneByName(sceneName);

        if (mode == ClientSceneLoadMode.Additive && alreadyLoadedScene.isLoaded)
        {
            SceneManager.SetActiveScene(alreadyLoadedScene);
            isLoading = false;
            yield break;
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, unityLoadMode);

        while (!op.isDone)
        {
            yield return null;
        }

        if (mode == ClientSceneLoadMode.Additive)
        {
            Scene loadedScene = SceneManager.GetSceneByName(sceneName);

            if (loadedScene.IsValid() && loadedScene.isLoaded)
            {
                SceneManager.SetActiveScene(loadedScene);
            }
        }

        isLoading = false;
    }

    private string ResolveTargetSceneName()
    {
        if (!string.IsNullOrEmpty(directSceneName))
        {
            return directSceneName;
        }

        switch (gotoSceneName)
        {
            case ClientScene.TITLE:
                return "Title";

            case ClientScene.OPTION:
                return "Option";

            case ClientScene.ROOM_CREATE:
                return "RoomCreate";

            case ClientScene.IN_GAME:
                return MainGameManager.instance.GetGotoSceneName();

            default:
                return "";
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (targetMaterial != null && targetMaterial.HasProperty(intensityID))
        {
            targetMaterial.SetFloat(intensityID, targetIntensity);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (targetMaterial != null && targetMaterial.HasProperty(intensityID))
        {
            targetMaterial.SetFloat(intensityID, currentIntensity);
        }
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}