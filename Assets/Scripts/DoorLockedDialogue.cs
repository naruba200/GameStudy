using UnityEngine;

public class DoorLockedDialogue : MonoBehaviour
{
    public DialogueManager dialogueManager;

    [TextArea]
    public string[] lockedLines;

    private bool playerInRange;

    private void Update()
    {
        // ✅ FIX: Don't allow interaction while dialogue is active
        if (dialogueManager.isDialogueActive) return;

        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (!PlayerInventory.Instance.hasKey)
            {
                dialogueManager.lines = lockedLines;
                dialogueManager.StartDialogue();
            }
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