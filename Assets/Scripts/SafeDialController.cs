using System.Collections.Generic;
using UnityEngine;

public class SafeDialController : MonoBehaviour
{
    [Header("Dial Settings")]
    public Transform dial; // assign safe_dial_0
    public float rotationStep = 36f;

    private int currentNumber = 0;

    [Header("Code Settings")]
    public List<int> correctCode = new List<int>() { 3, 7, 1, 5 }; // CHANGE THIS
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
        HandleRotation();
        HandleInput();
    }

    void HandleRotation()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            // Rotate left
            dial.Rotate(0, 0, rotationStep);
            currentNumber = (currentNumber + 1) % 10; // wrap backward
            Debug.Log("Current Number: " + currentNumber);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            // Rotate right
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
            Debug.Log("Entered: " + currentNumber);

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
                Debug.Log("Wrong Code!");
                playerInput.Clear();
                return;
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
    }
}