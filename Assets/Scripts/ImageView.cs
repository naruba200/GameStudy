using UnityEngine;
using UnityEngine.UI;

public class ImageViewer : MonoBehaviour
{
    public static ImageViewer Instance;

    [Header("UI")]
    public GameObject imagePanel;
    public Image displayImage;

    public bool isActive = false;

    void Awake() => Instance = this;

    private int closedFrame = -1;

void Update()
{
    if (isActive && Input.GetKeyDown(KeyCode.E) && Time.frameCount > closedFrame)
        CloseImage();
}

public void ShowImage(Sprite sprite)
{
    displayImage.sprite = sprite;
    imagePanel.SetActive(true);
    isActive = true;
    closedFrame = Time.frameCount; // bỏ qua E frame này
    FindObjectOfType<PlayerController>().StopMovement();
}

void CloseImage()
{
     imagePanel.SetActive(false);
    isActive = false;
    closedFrame = Time.frameCount;
    Debug.Log("ResumeMovement called");
    FindObjectOfType<PlayerController>().ResumeMovement();
}
}