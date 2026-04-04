using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;

    public string[] lines;
    private int currentLine = 0;

    private bool isDialogueActive = false;

    void Start()
    {
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
        currentLine = 0;
        dialogueText.text = lines[currentLine];
    }

    void NextLine()
    {
        currentLine++;

        if (currentLine < lines.Length)
        {
            dialogueText.text = lines[currentLine];
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
        Time.timeScale = 1f;
    }
}