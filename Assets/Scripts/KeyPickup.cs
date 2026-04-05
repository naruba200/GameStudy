using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    public string[] pickupDialogue;
    public DialogueManager dialogueManager;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (PlayerInventory.Instance != null)
            {
                PlayerInventory.Instance.SetKey(true);
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