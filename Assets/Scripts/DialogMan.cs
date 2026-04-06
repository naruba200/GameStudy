using UnityEngine;
using TMPro;
using System.Collections;
public class DialogueManager : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;

    public string[] lines;
    private int currentLine = 0;

    public bool isDialogueActive = false;
    private bool justStarted = false;
    private int startedFrame = -1;
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
    if (isDialogueActive && Input.GetKeyDown(KeyCode.E) && Time.frameCount > startedFrame)
        NextLine();
    }

    public void StartDialogue()
    {
    dialoguePanel.SetActive(true);
    isDialogueActive = true;
    startedFrame = Time.frameCount; // lưu frame hiện tại
    currentLine = 0;
    dialogueText.text = lines[currentLine];
    FindObjectOfType<PlayerController>().StopMovement();
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
    FindObjectOfType<PlayerController>().ResumeMovement();
    StartCoroutine(DeactivateNextFrame());
}

IEnumerator DeactivateNextFrame()
{
    yield return null;
    yield return null;
    isDialogueActive = false;
}
}