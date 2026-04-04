using UnityEngine;

public class SignDialogue : MonoBehaviour
{
    public DialogueManager dialogueManager;

    [TextArea]
    public string[] lines;

    private bool playerInRange = false;

    void Update()
    {
        // Prevent restarting dialogue while it's already running
        if (dialogueManager != null && dialogueManager.isDialogueActive)
            return;

        if (playerInRange && Input.GetKeyDown(KeyCode.E))
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