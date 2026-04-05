using UnityEngine;

public class DoorLockedDialogue : MonoBehaviour
{
    public DialogueManager dialogueManager;

    [Header("Required Collectible")]
    [Tooltip("Assign the collectible key item that this door expects.")]
    [SerializeField] private CollectibleItem requiredKeyItem;
    [SerializeField, HideInInspector] private string cachedRequiredItemName;
    [SerializeField, HideInInspector] private int cachedRequiredItemAmount = 1;

    [TextArea]
    public string[] lockedLines;

    private bool playerInRange;

    private void Update()
    {
        // ✅ FIX: Don't allow interaction while dialogue is active
        if (dialogueManager.isDialogueActive) return;

        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (!HasRequiredKey())
            {
                dialogueManager.lines = lockedLines;
                dialogueManager.StartDialogue();
            }
        }
    }

    private bool HasRequiredKey()
    {
        if (!TryGetRequiredItem(out string requiredItemName, out int requiredAmount))
        {
            return false;
        }

        PlayerController player = PlayerPersist.GetPlayerController();
        if (player == null)
        {
            player = Object.FindFirstObjectByType<PlayerController>();
        }

        return player != null && player.HasItem(requiredItemName, requiredAmount);
    }

    private bool TryGetRequiredItem(out string itemName, out int amount)
    {
        SyncRequiredItemCache();

        itemName = cachedRequiredItemName;
        amount = Mathf.Max(1, cachedRequiredItemAmount);

        return !string.IsNullOrWhiteSpace(itemName);
    }

    private void SyncRequiredItemCache()
    {
        if (requiredKeyItem == null || string.IsNullOrWhiteSpace(requiredKeyItem.ItemName))
        {
            // Clear cache if key item is invalid
            cachedRequiredItemName = null;
            cachedRequiredItemAmount = 1;
            return;
        }

        cachedRequiredItemName = requiredKeyItem.ItemName;
        cachedRequiredItemAmount = Mathf.Max(1, requiredKeyItem.Amount);
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

#if UNITY_EDITOR
    private void OnValidate()
    {
        SyncRequiredItemCache();
    }
#endif
}