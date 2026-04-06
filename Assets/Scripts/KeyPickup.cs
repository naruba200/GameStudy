using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    public string[] pickupDialogue;
    public DialogueManager dialogueManager;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInventory.Instance.SetKey(true);

            DialogueManager resolvedDialogueManager = DialogueManager.Resolve();
            if (resolvedDialogueManager != null)
            {
                dialogueManager = resolvedDialogueManager;
            }

            if (dialogueManager != null && pickupDialogue.Length > 0)
            {
                dialogueManager.lines = pickupDialogue;
                dialogueManager.StartDialogue();
            }

            gameObject.SetActive(false); // remove key
        }
    }
}