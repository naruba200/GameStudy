using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DoorItemLock : MonoBehaviour
{
    [Header("References")]
    public DialogueManager dialogueManager;
    public SceneTransition sceneTransition;

    [Header("Required Item")]
    [Tooltip("Must match InventoryItemEntry.displayName exactly (case-insensitive).")]
    public string requiredItemName = "Key";
    [Min(1)] public int requiredAmount = 1;

    [Header("Dialogue")]
    [TextArea] public string[] lockedLinesNoItem;
    [TextArea] public string[] unlockedLines;
    public bool showUnlockedDialogueOnce = true;

    [Header("Interaction")]
    [Tooltip("If enabled, uses OnTriggerEnter2D/OnTriggerExit2D range. If disabled, uses distance to player.")]
    public bool useTriggerRange = false;
    [Min(0.1f)] public float interactDistance = 1.1f;
    public Transform interactionPoint;

    [Header("Collision Dialogue")]
    [Tooltip("Show locked dialogue automatically when player collides with this solid door.")]
    public bool showLockedDialogueOnCollision = true;
    [Min(0f)] public float collisionDialogueCooldown = 0.6f;

    private bool playerInRange;
    private bool playerTouchingDoor;
    private bool hasShownUnlockedDialogue;
    private bool wasDialogueActive;
    private float interactCooldown;
    private float collisionDialogueTimer;
    private Collider2D doorCollider;

    private void Awake()
    {
        doorCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        DialogueManager resolvedDialogueManager = DialogueManager.Resolve();
        if (resolvedDialogueManager != null)
        {
            dialogueManager = resolvedDialogueManager;
        }

        if (collisionDialogueTimer > 0f)
        {
            collisionDialogueTimer -= Time.unscaledDeltaTime;
        }

        UpdatePlayerInRangeState();

        if (!playerInRange)
        {
            return;
        }

        bool isDialogueActive = dialogueManager != null && dialogueManager.isDialogueActive;

        // Prevent the same key press from closing and immediately retriggering interaction.
        if (wasDialogueActive && !isDialogueActive)
        {
            wasDialogueActive = false;
            return;
        }

        wasDialogueActive = isDialogueActive;

        if (interactCooldown > 0f)
        {
            interactCooldown -= Time.unscaledDeltaTime;
            return;
        }

        if (isDialogueActive)
        {
            return;
        }

        if (!Input.GetKeyDown(KeyCode.E))
        {
            return;
        }

        interactCooldown = 0.2f;

        if (!HasRequiredItem())
        {
            StartDialogue(lockedLinesNoItem);
            return;
        }

        if (showUnlockedDialogueOnce && !hasShownUnlockedDialogue && unlockedLines != null && unlockedLines.Length > 0)
        {
            hasShownUnlockedDialogue = true;
            StartDialogue(unlockedLines);
            return;
        }

        if (sceneTransition != null)
        {
            sceneTransition.TriggerTransition();
        }
    }

    private bool HasRequiredItem()
    {
        if (string.IsNullOrWhiteSpace(requiredItemName))
        {
            return true;
        }

        PlayerController player = PlayerPersist.GetPlayerController();
        if (player == null)
        {
            player = Object.FindFirstObjectByType<PlayerController>();
        }

        if (player == null)
        {
            return false;
        }

        List<InventoryItemEntry> snapshot = player.GetInventorySnapshot();
        int total = 0;

        for (int i = 0; i < snapshot.Count; i++)
        {
            InventoryItemEntry entry = snapshot[i];
            if (entry == null || string.IsNullOrWhiteSpace(entry.displayName))
            {
                continue;
            }

            if (!string.Equals(entry.displayName.Trim(), requiredItemName.Trim(), System.StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            total += Mathf.Max(0, entry.amount);
            if (total >= requiredAmount)
            {
                return true;
            }
        }

        return false;
    }

    private void StartDialogue(string[] lines)
    {
        DialogueManager resolvedDialogueManager = DialogueManager.Resolve();
        if (resolvedDialogueManager != null)
        {
            dialogueManager = resolvedDialogueManager;
        }

        if (dialogueManager == null || lines == null || lines.Length == 0)
        {
            return;
        }

        StartCoroutine(StartDialogueNextFrame(lines));
    }

    private IEnumerator StartDialogueNextFrame(string[] lines)
    {
        yield return null;
        dialogueManager.lines = lines;
        dialogueManager.StartDialogue();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!useTriggerRange)
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!useTriggerRange)
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision != null && collision.collider != null && collision.collider.CompareTag("Player"))
        {
            playerTouchingDoor = true;
        }

        TryShowLockedDialogueFromCollision(collision != null ? collision.collider : null);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision != null && collision.collider != null && collision.collider.CompareTag("Player"))
        {
            playerTouchingDoor = true;
        }

        TryShowLockedDialogueFromCollision(collision != null ? collision.collider : null);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision != null && collision.collider != null && collision.collider.CompareTag("Player"))
        {
            playerTouchingDoor = false;
        }
    }

    private void TryShowLockedDialogueFromCollision(Collider2D other)
    {
        if (!showLockedDialogueOnCollision)
        {
            return;
        }

        if (other == null || !other.CompareTag("Player"))
        {
            return;
        }

        if (collisionDialogueTimer > 0f)
        {
            return;
        }

        if (dialogueManager != null && dialogueManager.isDialogueActive)
        {
            return;
        }

        if (HasRequiredItem())
        {
            return;
        }

        collisionDialogueTimer = collisionDialogueCooldown;
        StartDialogue(lockedLinesNoItem);
    }

    private void UpdatePlayerInRangeState()
    {
        if (playerTouchingDoor)
        {
            playerInRange = true;
            return;
        }

        PlayerController player = PlayerPersist.GetPlayerController();
        if (player == null)
        {
            player = Object.FindFirstObjectByType<PlayerController>();
        }

        if (player == null)
        {
            playerInRange = false;
            return;
        }

        if (useTriggerRange && playerInRange)
        {
            return;
        }

        Vector2 playerPosition = player.transform.position;
        Vector2 origin;

        if (interactionPoint != null)
        {
            origin = interactionPoint.position;
        }
        else if (doorCollider != null)
        {
            origin = doorCollider.ClosestPoint(playerPosition);
        }
        else
        {
            origin = transform.position;
        }

        playerInRange = Vector2.Distance(origin, playerPosition) <= interactDistance;
    }
}
