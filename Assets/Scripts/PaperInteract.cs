using UnityEngine;

public class PaperInteract : MonoBehaviour
{
    public Sprite paperImage; // kéo sprite hình ảnh vào đây
    private bool playerNearby = false;

    void Update()
{
    if (playerNearby && Input.GetKeyDown(KeyCode.E))
    {
        // Nếu ảnh đang mở → không làm gì, để ImageViewer tự đóng
        if (ImageViewer.Instance.isActive) return;
        
        Debug.Log("Mở ảnh");
        ImageViewer.Instance.ShowImage(paperImage);
    }
}

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerNearby = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerNearby = false;
    }
}