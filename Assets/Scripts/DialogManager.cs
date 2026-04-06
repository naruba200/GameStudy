using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static bool IsAnyDialogueActive { get; private set; }
    public static DialogueManager ActiveInstance { get; private set; }

    public GameObject dialoguePanel;
    public TMP_Text dialogueText;

    public string[] lines;
    private string[] activeLines;
    private int currentLine = 0;

    public bool isDialogueActive { get; private set; }
    public bool CanPresentDialogue => dialoguePanel != null && dialogueText != null && gameObject.scene.IsValid() && gameObject.scene.isLoaded;

    private void Awake()
    {
        if (ActiveInstance == null || !IsManagerUsable(ActiveInstance))
        {
            ActiveInstance = this;
        }
    }

    void Start()
    {
        IsAnyDialogueActive = false;

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        Time.timeScale = 1f;
    }

    private void OnDestroy()
    {
        if (ActiveInstance == this)
        {
            ActiveInstance = null;
        }
    }

    public static DialogueManager Resolve()
    {
        if (IsManagerUsable(ActiveInstance))
        {
            return ActiveInstance;
        }

        DialogueManager[] managers = Object.FindObjectsByType<DialogueManager>(FindObjectsSortMode.None);
        for (int i = 0; i < managers.Length; i++)
        {
            if (IsManagerUsable(managers[i]))
            {
                ActiveInstance = managers[i];
                return ActiveInstance;
            }
        }

        // Fallback: accept first loaded manager even if references are not wired yet.
        for (int i = 0; i < managers.Length; i++)
        {
            DialogueManager candidate = managers[i];
            if (candidate != null && candidate.gameObject.scene.IsValid() && candidate.gameObject.scene.isLoaded)
            {
                ActiveInstance = candidate;
                return ActiveInstance;
            }
        }

        ActiveInstance = Object.FindFirstObjectByType<DialogueManager>();
        return ActiveInstance;
    }

    public static void ResetGlobalDialogueState()
    {
        IsAnyDialogueActive = false;
        Time.timeScale = 1f;

        DialogueManager manager = Resolve();
        if (manager == null)
        {
            return;
        }

        if (manager.dialoguePanel != null)
        {
            manager.dialoguePanel.SetActive(false);
        }

        manager.isDialogueActive = false;
    }

    private static bool IsManagerUsable(DialogueManager manager)
    {
        return manager != null &&
               manager.gameObject != null &&
               manager.gameObject.scene.IsValid() &&
               manager.gameObject.scene.isLoaded &&
               manager.dialoguePanel != null &&
               manager.dialogueText != null;
    }

    void Update()
    {
        if (isDialogueActive && Input.GetKeyDown(KeyCode.E))
        {
            NextLine();
        }
    }

    public void StartDialogue()
    {
        if (!CanPresentDialogue)
        {
            DialogueManager resolved = Resolve();
            if (resolved != null && resolved != this)
            {
                resolved.lines = lines;
                resolved.StartDialogue();
            }
            return;
        }

        if (dialoguePanel == null || dialogueText == null)
        {
            return;
        }

        Time.timeScale = 0f;
        dialoguePanel.SetActive(true);
        isDialogueActive = true;
        IsAnyDialogueActive = true;
        activeLines = lines;
        currentLine = 0;
        dialogueText.text = activeLines != null && activeLines.Length > 0 ? activeLines[currentLine] : string.Empty;
    }

    public void ShowAcquireMessage(string itemDisplayName)
    {
        if (!CanPresentDialogue)
        {
            DialogueManager resolved = Resolve();
            if (resolved != null && resolved != this)
            {
                resolved.ShowAcquireMessage(itemDisplayName);
            }
            return;
        }

        if (isDialogueActive || dialoguePanel == null || dialogueText == null)
        {
            return;
        }

        Time.timeScale = 0f;
        dialoguePanel.SetActive(true);
        isDialogueActive = true;
        IsAnyDialogueActive = true;
        activeLines = new[] { "Acquire " + itemDisplayName };
        currentLine = 0;
        dialogueText.text = activeLines[0];
    }

    void NextLine()
    {
        currentLine++;

        if (activeLines != null && currentLine < activeLines.Length)
        {
            dialogueText.text = activeLines[currentLine];
        }
        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        isDialogueActive = false;
        IsAnyDialogueActive = false;
        activeLines = null;
        Time.timeScale = 1f;
    }
}