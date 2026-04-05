using UnityEngine;
using System.Collections;

public class DoorInteraction : MonoBehaviour
{
    public DialogueManager dialogueManager;
    public SceneTransition sceneTransition;

    [Header("Dialogue")]
    [TextArea] public string[] lockedLines;
    [TextArea] public string[] unlockedLines;

    public bool skipDialogue = false;

    private bool playerInRange = false;
    private bool hasShownUnlockDialogue = false;
    private bool wasDialogueActive = false;

    private float interactCooldown = 0f;

    void Update()
    {
        if (!playerInRange) return;

        bool isDialogueActive = dialogueManager != null && dialogueManager.isDialogueActive;

        // Prevent the same E press that closes dialogue from reopening it or triggering the door.
        if (wasDialogueActive && !isDialogueActive)
        {
            wasDialogueActive = isDialogueActive;
            return;
        }

        wasDialogueActive = isDialogueActive;

        // ⏱ Cooldown to prevent instant retrigger after dialogue ends
        if (interactCooldown > 0f)
        {
            interactCooldown -= Time.unscaledDeltaTime;
            return;
        }

        // 🛑 Don't interrupt dialogue
        if (isDialogueActive) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            interactCooldown = 0.2f; // 👈 KEY FIX

            // 🚪 Inside door (no dialogue at all)
            if (skipDialogue)
            {
                sceneTransition.TriggerTransition();
                return;
            }

            // ❌ NO KEY → show locked dialogue (repeatable)
            if (!PlayerInventory.Instance.hasKey)
            {
                StartCoroutine(StartDialogueNextFrame(lockedLines));
                return;
            }

            // ✅ FIRST TIME WITH KEY → show unlock dialogue ONCE
            if (!hasShownUnlockDialogue)
            {
                hasShownUnlockDialogue = true;

                StartCoroutine(StartDialogueNextFrame(unlockedLines));
                return;
            }

            // ✅ AFTER THAT → open door
            sceneTransition.TriggerTransition();
        }
    }

    IEnumerator StartDialogueNextFrame(string[] dialogueLines)
    {
        yield return null; // wait 1 frame to avoid skipping first line

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