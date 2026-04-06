using UnityEngine;
using TMPro;
using System.Collections;

public class IntroSequence : MonoBehaviour
{
    [Header("UI")]
    public GameObject introPanel;       // Panel nền đen chứa text
    public TMP_Text introText;          // Text hiển thị trong panel

    [Header("Flashlight")]
    public GameObject flashlight;       // GameObject đèn pin gắn với player

    [Header("Settings")]
    public float delayBetweenLines = 2f;  // Thời gian giữa 2 dòng text
    public float fadeDuration = 0.5f;     // Thời gian fade in/out text

    private PlayerController player;

    private void Start()
    {
        player = FindObjectOfType<PlayerController>();

        // Tắt flashlight khi bắt đầu
        if (flashlight != null)
            flashlight.SetActive(false);

        // Ẩn panel ban đầu
        if (introPanel != null)
            introPanel.SetActive(false);

        // Dừng player di chuyển trong lúc intro
        if (player != null)
            player.StopMovement();

        StartCoroutine(PlayIntro());
    }

    IEnumerator PlayIntro()
    {
        // Hiện panel
        introPanel.SetActive(true);

        CanvasGroup cg = introPanel.GetComponent<CanvasGroup>();
        if (cg == null) cg = introPanel.AddComponent<CanvasGroup>();

        // --- Dòng 1: "Tối quá..." ---
        introText.text = "Tối quá...";
        yield return FadeTextIn(cg);
        yield return new WaitForSeconds(delayBetweenLines);
        yield return FadeTextOut(cg);

        // --- Dòng 2: "À, có đèn pin..." ---
        introText.text = "À, có đèn pin...";
        yield return FadeTextIn(cg);
        yield return new WaitForSeconds(delayBetweenLines);
        yield return FadeTextOut(cg);

        // Ẩn panel
        introPanel.SetActive(false);

        // Bật đèn pin
        if (flashlight != null)
            flashlight.SetActive(true);

        // Cho phép player di chuyển
        if (player != null)
            player.ResumeMovement();
    }

    IEnumerator FadeTextIn(CanvasGroup cg)
    {
        float t = 0f;
        cg.alpha = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }
        cg.alpha = 1f;
    }

    IEnumerator FadeTextOut(CanvasGroup cg)
    {
        float t = 0f;
        cg.alpha = 1f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            cg.alpha = 1f - Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }
        cg.alpha = 0f;
    }
}
