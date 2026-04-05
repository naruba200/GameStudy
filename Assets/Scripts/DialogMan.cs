using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static bool IsAnyDialogueActive { get; private set; }

    public GameObject dialoguePanel;
    public TMP_Text dialogueText;

    public string[] lines;
    private string[] activeLines;
    private int currentLine = 0;

    private bool isDialogueActive = false;

    void Start()
    {
        IsAnyDialogueActive = false;

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        Time.timeScale = 1f;
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