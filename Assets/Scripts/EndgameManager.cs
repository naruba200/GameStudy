using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

/// <summary>
/// Quản lý màn hình kết thúc game.
/// - Fade in khi scene bắt đầu.
/// - Hiển thị nội dung endgame.
/// - Fade out rồi chuyển scene khi gọi EndGame().
/// </summary>
public class EndgameManager : MonoBehaviour
{
    [Header("Fade")]
    public CanvasGroup fadeCanvasGroup;   // CanvasGroup của panel đen fade
    public float fadeDuration = 1.5f;

    [Header("Endgame UI (tuỳ chọn)")]
    public GameObject endgamePanel;       // Panel hiển thị nội dung kết thúc
    public TMP_Text endgameText;          // Text kết thúc (tuỳ chọn)

    [Header("Chuyển scene sau endgame (tuỳ chọn)")]
    public string nextScene = "";         // Để trống nếu không cần chuyển scene
    public float autoEndDelay = 0f;       // 0 = không tự động kết thúc

    private void Awake()
    {
        // Tìm và ẩn player đến từ DontDestroyOnLoad (từ scene trước)
        // Player của Endgame scene sẽ không bị ảnh hưởng
        PlayerPersist[] allPersists = FindObjectsOfType<PlayerPersist>(true);
        foreach (PlayerPersist p in allPersists)
        {
            if (p.gameObject.scene.name == "DontDestroyOnLoad")
            {
                p.gameObject.SetActive(false);
                break;
            }
        }
    }

    private void Start()
    {
        // Ẩn panel endgame ban đầu
        if (endgamePanel != null)
            endgamePanel.SetActive(false);

        // Kích hoạt animation EndPlayer trên Player
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            Animator anim = player.GetComponent<Animator>();
            if (anim != null)
                anim.SetBool("isEndGame", true);
        }

        // Bắt đầu với màn hình đen rồi fade in
        StartCoroutine(FadeInRoutine());
    }

    // ─────────────────────────────────────────
    // Fade IN (đen → trong suốt) khi scene load
    // ─────────────────────────────────────────
    IEnumerator FadeInRoutine()
    {
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 1f;
            fadeCanvasGroup.gameObject.SetActive(true);

            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                fadeCanvasGroup.alpha = 1f - Mathf.Clamp01(t / fadeDuration);
                yield return null;
            }
            fadeCanvasGroup.alpha = 0f;
        }

        // Hiện nội dung endgame
        if (endgamePanel != null)
            endgamePanel.SetActive(true);

        // Tự động kết thúc sau một khoảng thời gian (nếu có)
        if (autoEndDelay > 0f && !string.IsNullOrEmpty(nextScene))
        {
            yield return new WaitForSeconds(autoEndDelay);
            EndGame();
        }
    }

    // ─────────────────────────────────────────
    // Fade OUT (trong suốt → đen) rồi chuyển scene
    // Gọi từ Button hoặc sự kiện bất kỳ
    // ─────────────────────────────────────────
    public void EndGame()
    {
        StartCoroutine(FadeOutRoutine());
    }

    IEnumerator FadeOutRoutine()
    {
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.gameObject.SetActive(true);

            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Clamp01(t / fadeDuration);
                yield return null;
            }
            fadeCanvasGroup.alpha = 1f;
        }
        else
        {
            yield return new WaitForSeconds(fadeDuration);
        }

        if (!string.IsNullOrEmpty(nextScene))
            SceneManager.LoadScene(nextScene);
    }
}
