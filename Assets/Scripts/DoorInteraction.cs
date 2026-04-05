using UnityEngine;
using System.Collections;

public class DoorInteraction : MonoBehaviour
{
    public DialogueManager dialogueManager;
    public SceneTransition sceneTransition;

    [Header("Required Collectible")]
    [Tooltip("Enter the item name (string) that unlocks this door. E.g., 'key2'")]
    [SerializeField] private string requiredItemName = "key2";

    [Header("Dialogue")]
    [TextArea] public string[] lockedLines;
    [TextArea] public string[] unlockedLines;

    public bool skipDialogue = false;

    private bool playerInRange = false;
    private bool hasShownUnlockDialogue = false;
    private bool wasDialogueActive = false;

    private float interactCooldown = 0f;

    private void Awake()
    {
        // Nothing special needed
    }

    void Update()
    {
        if (!playerInRange) return;

        bool isDialogueActive = dialogueManager != null && dialogueManager.isDialogueActive;

        // Prevent instant retrigger after dialogue closes
        if (wasDialogueActive && !isDialogueActive)
        {
            wasDialogueActive = isDialogueActive;
            return;
        }

        wasDialogueActive = isDialogueActive;

        // Cooldown
        if (interactCooldown > 0f)
        {
            interactCooldown -= Time.unscaledDeltaTime;
            return;
        }

        // Don't interrupt dialogue
        if (isDialogueActive) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            interactCooldown = 0.2f;

            // 🔒 LOCKED
            if (!HasRequiredKey())
            {
                if (dialogueManager != null && lockedLines.Length > 0)
                {
                    StartCoroutine(StartDialogueNextFrame(lockedLines));
                }
                return;
            }

            // 🔓 HAS KEY

            if (skipDialogue)
            {
                if (sceneTransition != null)
                {
                    sceneTransition.TriggerTransition();
                }
                return;
            }

            // First time → unlock dialogue
            if (!hasShownUnlockDialogue)
            {
                hasShownUnlockDialogue = true;

                if (dialogueManager != null && unlockedLines.Length > 0)
                {
                    StartCoroutine(StartDialogueNextFrame(unlockedLines));
                }
                return;
            }

            // 🚪 Open door
            if (sceneTransition != null)
            {
                sceneTransition.TriggerTransition();
            }
        }
    }

    private bool HasRequiredKey()
    {
        // ❗ Check if required item name is set
        if (string.IsNullOrEmpty(requiredItemName))
        {
            Debug.LogWarning("DoorInteraction: No required item name specified. Door is LOCKED.");
            return false;
        }

        PlayerController player = PlayerPersist.GetPlayerController();

        if (player == null)
        {
            player = Object.FindFirstObjectByType<PlayerController>();
        }

        if (player == null)
        {
            Debug.LogError("DoorInteraction: Cannot find PlayerController!");
            return false;
        }

        // ✅ Use string-based inventory check
        bool hasItem = player.HasItem(requiredItemName, 1);

        Debug.Log($"DoorInteraction: Checking for '{requiredItemName}' → {hasItem}");

        return hasItem;
    }

    IEnumerator StartDialogueNextFrame(string[] dialogueLines)
    {
        yield return null;

        dialogueManager.lines = dialogueLines;
        dialogueManager.StartDialogue();
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