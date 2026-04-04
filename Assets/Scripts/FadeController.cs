using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class FadeController : MonoBehaviour
{
    public Animator animator;

    private CanvasGroup canvasGroup;
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
            return;
        }

        fadeInOnNextScene = false;
        StartCoroutine(FadeIn());
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
}