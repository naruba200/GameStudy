using UnityEngine;

public class DoorController : MonoBehaviour
{
     [Header("Audio")]
public AudioClip openSound;
public AudioClip closeSound;
private AudioSource audioSource;
    [Header("Sprites")]
    public GameObject doorClosed;
    public GameObject doorOpen;

    [Header("Collider")]
    public Collider2D doorCollider;

    [Header("Dialogue")]
    public DialogueManager dialogueManager;

    private bool isOpen = false;
    private bool hasOpenedBefore = false;
    private bool dialogueDone = false; // dialogue đã xong chưa
    private int playerCount = 0;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        doorClosed.SetActive(true);
        doorOpen.SetActive(false);
        doorCollider.enabled = true;
    }

    void Update()
{
    if (!Input.GetKeyDown(KeyCode.E)) return;


    if (dialogueManager.isDialogueActive) return;
    if (playerCount <= 0) return;

    if (!hasOpenedBefore && !dialogueDone)
    {
        
        hasOpenedBefore = true;
        dialogueManager.lines = new string[] { "Tay nắm rỉ sét rồi, khó mở quá..." };
        dialogueManager.StartDialogue();
        return;
    }

        // Dialogue đã xong, lần bấm E tiếp theo mở cửa
        if (hasOpenedBefore && !dialogueDone)
        {
            dialogueDone = true;
            ToggleDoor();
            CutsceneManager.Instance.PlayDoorCutscene();
            return;
        }

        // Bình thường sau đó
        ToggleDoor();
    }

    void ToggleDoor()
    {
        isOpen = !isOpen;
        doorClosed.SetActive(!isOpen);
        doorOpen.SetActive(isOpen);
        doorCollider.enabled = !isOpen;

         if (audioSource != null)
    {
        AudioClip clip = isOpen ? openSound : closeSound;
        if (clip != null) audioSource.PlayOneShot(clip);
    }
    }

    public void OnPlayerEnter() => playerCount++;
    public void OnPlayerExit() => playerCount = Mathf.Max(0, playerCount - 1);
}