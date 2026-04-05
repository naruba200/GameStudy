using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SafeDialController : MonoBehaviour
{
    [Header("Safe Identification")]
    [SerializeField] private string safeId = "safe_main"; // Must match the SafeTransition safeId
    
    [Header("Return to Scene")]
    [SerializeField] private string returnSceneName = "SafeRoom"; // Scene to return to after opening
    [SerializeField] private float returnDelay = 2f; // Wait time before returning
    
    [Header("Dial Settings")]
    public Transform dial; // assign safe_dial_0
    public float rotationStep = 36f;

    private int currentNumber = 0;

    [Header("Code Settings")]
    public List<int> correctCode = new List<int>() { 3, 7, 1, 5 };
    private List<int> playerInput = new List<int>();

    [Header("Safe Parts")]
    public GameObject safe_locked_0;
    public GameObject safe_ring_0;
    public GameObject safe_dial_0;
    public GameObject safe_unlocked_0;

    private void Start()
    {
        safe_unlocked_0.SetActive(false);
    }

    private void Update()
    {
        // Allow returning to previous scene with E key
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E pressed - returning to SafeRoom");
            ReturnToSafeRoom();
            return;
        }

        HandleRotation();
        HandleInput();
    }

    void HandleRotation()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            // Rotate left (clockwise visually)
            dial.Rotate(0, 0, rotationStep);
            currentNumber = (currentNumber + 1) % 10;
            Debug.Log("Current Number: " + currentNumber);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            // Rotate right (counterclockwise visually)
            dial.Rotate(0, 0, -rotationStep);
            currentNumber = (currentNumber + 9) % 10;
            Debug.Log("Current Number: " + currentNumber);
        }
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            playerInput.Add(currentNumber);

            // Keep only last N inputs (sliding window)
            if (playerInput.Count > correctCode.Count)
            {
                playerInput.RemoveAt(0);
            }

            Debug.Log("Sequence: " + string.Join(" ", playerInput));

            if (playerInput.Count == correctCode.Count)
            {
                CheckCode();
            }
        }
    }

    void CheckCode()
    {
        for (int i = 0; i < correctCode.Count; i++)
        {
            if (playerInput[i] != correctCode[i])
            {
                return; // allow brute force (no reset)
            }
        }

        Debug.Log("Safe Opened!");
        OpenSafe();
    }

    void OpenSafe()
    {
        safe_locked_0.SetActive(false);
        safe_ring_0.SetActive(false);
        safe_dial_0.SetActive(false);
        safe_unlocked_0.SetActive(true);
        
        // Save the unlock state persistent
        PlayerPrefs.SetInt("SafeUnlocked_" + safeId, 1);
        PlayerPrefs.Save();
        
        Debug.Log("Safe " + safeId + " unlocked and state saved!");
        
        // Return to SafeRoom after delay
        StartCoroutine(ReturnToSafeRoomDelayed());
    }
    
    private void ReturnToSafeRoom()
    {
        // Load SafeRoom - SafeTransition.OnEnable will re-enable the player
        SceneManager.LoadScene(returnSceneName);
    }
    
    System.Collections.IEnumerator ReturnToSafeRoomDelayed()
    {
        yield return new WaitForSeconds(returnDelay);
        ReturnToSafeRoom();
    }
}