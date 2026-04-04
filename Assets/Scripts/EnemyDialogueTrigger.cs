using UnityEngine;

public class EnemyDialogueTrigger : MonoBehaviour
{
    public DialogueManager dialogueManager;
    public Transform player;

    public string[] lines;
    public float triggerDistance = 3f;

    private bool hasTriggered = false;

    void Update()
    {
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= triggerDistance && !hasTriggered)
        {
            hasTriggered = true;

            dialogueManager.lines = lines;
            dialogueManager.StartDialogue();
        }
    }
}