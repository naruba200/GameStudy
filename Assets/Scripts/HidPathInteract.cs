using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Gắn vào GameObject "HidPath" trong scene House#2.
/// Khi player đứng gần và bấm E → fade out → chuyển sang scene Endgame.
/// Yêu cầu PictureFrame phải được giải xong trước.
/// </summary>
public class HidPathInteract : MonoBehaviour
{
    [Header("Scene")]
    public string targetScene = "Endgame";

    [Header("Fade")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 1.5f;

    [Header("Điều kiện mở khoá")]
    [Tooltip("HidPath chỉ hoạt động khi PictureFrame đã được giải xong")]
    public PictureFrameInteract pictureFrame; // kéo PictureFrameInteract vào đây

    [Header("Hint UI (tuỳ chọn)")]
    public GameObject interactPrompt;

    private bool playerNearby = false;
    private bool isTransitioning = false;

    private void Start()
    {
        if (fadeCanvasGroup == null)
        {
            GameObject fadeObj = GameObject.Find("FadePanel");
            if (fadeObj != null)
                fadeCanvasGroup = fadeObj.GetComponent<CanvasGroup>();
        }

        // Tự tìm PictureFrameInteract nếu chưa gán
        if (pictureFrame == null)
            pictureFrame = FindObjectOfType<PictureFrameInteract>();

        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }

    private void Update()
    {
        if (!playerNearby || isTransitioning) return;

        // Chặn hoàn toàn nếu PictureFrame chưa được giải
        // (dù pictureFrame null hay không cũng phải solved mới cho qua)
        bool solved = pictureFrame != null && pictureFrame.IsSolved;
        if (!solved) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            isTransitioning = true;

            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null) player.StopMovement();

            if (interactPrompt != null)
                interactPrompt.SetActive(false);

            StartCoroutine(FadeAndLoadScene());
        }
    }

    IEnumerator FadeAndLoadScene()
    {
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.gameObject.SetActive(true);
            fadeCanvasGroup.alpha = 0f;

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

        SceneManager.LoadScene(targetScene);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerNearby = true;
        if (interactPrompt != null)
            interactPrompt.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerNearby = false;
        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }
}
