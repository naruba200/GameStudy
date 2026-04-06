using UnityEngine;

public class TriggerZone : MonoBehaviour
{
    public GameObject door; // nhận bất kỳ GameObject nào

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            door.SendMessage("OnPlayerEnter");
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            door.SendMessage("OnPlayerExit");
    }
}