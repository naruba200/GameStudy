using UnityEngine;
using System.Collections;

public class PictureFrameInteract : MonoBehaviour
{
    [Header("Audio")]
public AudioClip slideSound;
private AudioSource audioSource;
    [Header("References")]
    public DialogueManager dialogueManager;

    [Header("Slide Settings")]
    public float slideDistance = 2f;
    public float slideSpeed = 3f;

    private bool playerNearby = false;
    private bool codePanelShown = false;
    private bool isSliding = false;
    private bool isSolved = false; // chỉ khóa sau khi nhập đúng mã
    private Vector3 targetPosition;

    void Start()
    {
        targetPosition = transform.position;
         audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (isSliding)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, targetPosition, slideSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                isSliding = false;
            }
        }

        if (!playerNearby || isSolved) return;
        if (dialogueManager.isDialogueActive) return;
        if (CodePanelManager.Instance.isActive) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!codePanelShown)
            {
                // Lần đầu: hiện dialogue
                codePanelShown = true;
                dialogueManager.lines = new string[] { "Đằng sau bức tranh có gì đó... Là một ổ khóa số" };
                dialogueManager.StartDialogue();
            }
            else
            {
                // Lần 2 trở đi: mở thẳng code panel
                CodePanelManager.Instance.OpenPanel(OnCorrectCode);
            }
        }
    }

    void OnCorrectCode()
    {
        isSolved = true; // khóa sau khi giải xong
        targetPosition = transform.position + Vector3.right * slideDistance;
        isSliding = true;
        if (audioSource != null && slideSound != null)
        audioSource.PlayOneShot(slideSound);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerNearby = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerNearby = false;
    }
}