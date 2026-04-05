using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class FadeController : MonoBehaviour
{
    public Animator animator;

    private CanvasGroup canvasGroup;
    private CanvasGroup loadingCanvasGroup;
    private GameObject loadingOverlayRoot;
    private TMP_Text loadingText;
    private static bool fadeInOnNextScene;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f;

        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.ResumeMovement();
        }

        if (!fadeInOnNextScene)
        {
            HideLoadingOverlay();
            return;
        }

        fadeInOnNextScene = false;
        StartCoroutine(FadeIn());
        HideLoadingOverlay();
    }

    public void RequestFadeInOnNextScene()
    {
        fadeInOnNextScene = true;
    }

    private bool TryResolveAnimator()
    {
        if (animator == null)
        {
            GameObject fadeObject = GameObject.Find("Fade");
            if (fadeObject != null)
            {
                animator = fadeObject.GetComponent<Animator>();

                GameObject fadeRoot = fadeObject.transform.root.gameObject;
                Canvas rootCanvas = fadeRoot.GetComponent<Canvas>();
                if (rootCanvas != null)
                {
                    rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    rootCanvas.overrideSorting = true;
                    rootCanvas.sortingOrder = 1000;

                    if (fadeRoot.GetComponent<GraphicRaycaster>() == null)
                    {
                        fadeRoot.AddComponent<GraphicRaycaster>();
                    }
                }

                DontDestroyOnLoad(fadeRoot);
            }
        }

        if (animator == null)
        {
            return false;
        }

        animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        canvasGroup = animator.GetComponent<CanvasGroup>();
        return true;
    }

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (!TryResolveAnimator())
        {
            return;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        animator.enabled = false;
        animator.Play("Start", 0, 0f);

        DontDestroyOnLoad(gameObject);
    }

    public IEnumerator FadeOut()
    {
        if (!TryResolveAnimator())
        {
            yield break;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        animator.enabled = true;
        animator.Play("End", 0, 0f);
        yield return new WaitForSecondsRealtime(1f);
    }

    public IEnumerator FadeIn()
    {
        if (!TryResolveAnimator())
        {
            yield break;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        animator.enabled = true;
        animator.Play("Start", 0, 0f);
        yield return new WaitForSecondsRealtime(1f);
        animator.enabled = false;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }

    public IEnumerator FadeToScene(string sceneName, Vector2 spawnPosition, string spawnPointId)
    {
        yield return FadeOut();
        RequestFadeInOnNextScene();

        PlayerPrefs.SetString("SpawnPointId", spawnPointId ?? string.Empty);
        PlayerPrefs.SetFloat("SpawnX", spawnPosition.x);
        PlayerPrefs.SetFloat("SpawnY", spawnPosition.y);

        SceneManager.LoadScene(sceneName);
    }

    public IEnumerator LoadSceneWithLoading(string sceneName)
    {
        ShowLoadingOverlay("Loading...");

        yield return null;

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName);
        if (loadOperation == null)
        {
            HideLoadingOverlay();
            yield break;
        }

        while (!loadOperation.isDone)
        {
            if (loadingText != null)
            {
                loadingText.text = "Loading... " + Mathf.RoundToInt(Mathf.Clamp01(loadOperation.progress / 0.9f) * 100f) + "%";
            }

            yield return null;
        }

        HideLoadingOverlay();
    }

    private void ShowLoadingOverlay(string message)
    {
        if (!TryResolveAnimator())
        {
            return;
        }

        EnsureLoadingOverlayExists();

        if (loadingOverlayRoot != null)
        {
            loadingOverlayRoot.SetActive(true);
        }

        if (loadingCanvasGroup != null)
        {
            loadingCanvasGroup.alpha = 1f;
            loadingCanvasGroup.blocksRaycasts = true;
            loadingCanvasGroup.interactable = true;
        }

        if (loadingText != null)
        {
            loadingText.text = message;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        if (animator != null)
        {
            animator.enabled = false;
        }
    }

    private void HideLoadingOverlay()
    {
        if (loadingOverlayRoot != null)
        {
            loadingOverlayRoot.SetActive(false);
        }

        if (loadingCanvasGroup != null)
        {
            loadingCanvasGroup.alpha = 0f;
            loadingCanvasGroup.blocksRaycasts = false;
            loadingCanvasGroup.interactable = false;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }

    private void EnsureLoadingOverlayExists()
    {
        if (loadingOverlayRoot != null)
        {
            return;
        }

        GameObject fadeRoot = animator != null ? animator.transform.root.gameObject : gameObject;

        loadingOverlayRoot = new GameObject("LoadingOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
        loadingOverlayRoot.transform.SetParent(fadeRoot.transform, false);
        loadingOverlayRoot.transform.SetAsLastSibling();

        RectTransform overlayRect = loadingOverlayRoot.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        Image background = loadingOverlayRoot.GetComponent<Image>();
        background.sprite = CreateSolidSprite(Color.black);
        background.type = Image.Type.Sliced;
        background.color = new Color(0f, 0f, 0f, 0.92f);
        background.raycastTarget = true;

        loadingCanvasGroup = loadingOverlayRoot.GetComponent<CanvasGroup>();
        loadingCanvasGroup.alpha = 0f;
        loadingCanvasGroup.blocksRaycasts = false;
        loadingCanvasGroup.interactable = false;

        GameObject textObject = new GameObject("LoadingText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(loadingOverlayRoot.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = new Vector2(500f, 100f);

        loadingText = textObject.GetComponent<TextMeshProUGUI>();
        loadingText.text = "Loading...";
        loadingText.alignment = TextAlignmentOptions.Center;
        loadingText.fontSize = 36f;
        loadingText.color = Color.white;
    }

    private Sprite CreateSolidSprite(Color color)
    {
        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f));
    }
}