using UnityEngine;

public class SignDialogue : MonoBehaviour
{
    public DialogueManager dialogueManager;

    [TextArea]
    public string[] lines;

    private bool playerInRange = false;

    void Update()
    {
        if (dialogueManager == null)
        {
            dialogueManager = DialogueManager.Resolve();
        }

        // Prevent restarting dialogue while it's already running
        if (dialogueManager != null && dialogueManager.isDialogueActive)
            return;

        if (dialogueManager != null && playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            dialogueManager.lines = lines;
            dialogueManager.StartDialogue();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}