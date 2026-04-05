using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoadingOverlay : MonoBehaviour
{
    private static SceneLoadingOverlay instance;

    private Canvas rootCanvas;
    private CanvasGroup canvasGroup;
    private Text loadingText;
    private bool isLoading;
    private Font overrideFont;

    public static bool IsLoading => instance != null && instance.isLoading;

    public static void LoadScene(string sceneName, Font loadingFontOverride = null)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("SceneLoadingOverlay.LoadScene called with an empty scene name.");
            return;
        }

        EnsureInstance();
        if (instance == null || instance.isLoading)
        {
            return;
        }

        instance.ApplyFontOverride(loadingFontOverride);

        instance.StartCoroutine(instance.LoadSceneRoutine(sceneName));
    }

    private static void EnsureInstance()
    {
        if (instance != null)
        {
            return;
        }

        GameObject host = new GameObject("SceneLoadingOverlay");
        instance = host.AddComponent<SceneLoadingOverlay>();
        DontDestroyOnLoad(host);
        instance.InitializeUI();
    }

    private void InitializeUI()
    {
        rootCanvas = gameObject.AddComponent<Canvas>();
        rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        rootCanvas.overrideSorting = true;
        rootCanvas.sortingOrder = 20000;

        gameObject.AddComponent<CanvasScaler>();
        gameObject.AddComponent<GraphicRaycaster>();

        canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        GameObject bgObject = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgObject.transform.SetParent(transform, false);

        RectTransform bgRect = bgObject.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        Image bgImage = bgObject.GetComponent<Image>();
        bgImage.color = Color.black;
        bgImage.raycastTarget = true;

        GameObject textObject = new GameObject("LoadingText", typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, 0f);
        textRect.anchorMax = new Vector2(0f, 0f);
        textRect.pivot = new Vector2(0f, 0f);
        textRect.anchoredPosition = new Vector2(36f, 24f);
        textRect.sizeDelta = new Vector2(700f, 120f);

        loadingText = textObject.GetComponent<Text>();
        loadingText.alignment = TextAnchor.LowerLeft;
        loadingText.fontSize = 36;
        loadingText.color = Color.white;
        loadingText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        loadingText.text = "Loading...";
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        isLoading = true;

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
        loadingText.text = "Loading...";

        yield return null;

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        if (operation == null)
        {
            HideOverlay();
            yield break;
        }

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            loadingText.text = "Loading... " + Mathf.RoundToInt(progress * 100f) + "%";
            yield return null;
        }

        HideOverlay();
    }

    private void HideOverlay()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        isLoading = false;
    }

    private void ApplyFontOverride(Font loadingFontOverride)
    {
        if (loadingText == null)
        {
            return;
        }

        overrideFont = loadingFontOverride;
        if (overrideFont != null)
        {
            loadingText.font = overrideFont;
        }
    }
}
