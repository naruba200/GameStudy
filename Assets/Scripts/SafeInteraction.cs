using UnityEngine;

public class SafeInteraction : MonoBehaviour
{
    [Header("Safe Identification")]
    [SerializeField] private string safeId = "safe_main"; // Unique ID for this safe
    
    [Header("Scene Transition")]
    public SceneTransition sceneTransition;
    
    [Header("Visual States")]
    public GameObject safeLockedVisual;      // The locked safe appearance
    public GameObject safeUnlockedVisual;    // The unlocked safe appearance
    
    private bool playerInRange = false;
    private bool safeUnlocked = false;

    private void Awake()
    {
        // Find SceneTransition if not assigned
        if (sceneTransition == null)
        {
            sceneTransition = GetComponent<SceneTransition>();
        }
    }

    private void Start()
    {
        // Check if this safe was already unlocked
        safeUnlocked = PlayerPrefs.GetInt("SafeUnlocked_" + safeId, 0) == 1;
        UpdateVisuals();
    }

    private void Update()
    {
        if (!playerInRange) return;
        
        // If already unlocked, don't allow re-entry
        if (safeUnlocked) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            // Transition to safe opening scene
            if (sceneTransition != null)
            {
                sceneTransition.TriggerTransition();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    // Called when returning from the safe opening scene to check if it was unlocked
    public void CheckAndUpdateSafeState()
    {
        safeUnlocked = PlayerPrefs.GetInt("SafeUnlocked_" + safeId, 0) == 1;
        UpdateVisuals();
        
        if (safeUnlocked)
        {
            Debug.Log("Safe " + safeId + " is now unlocked!");
        }
    }

    private void UpdateVisuals()
    {
        if (safeLockedVisual != null)
        {
            safeLockedVisual.SetActive(!safeUnlocked);
        }

        if (safeUnlockedVisual != null)
        {
            safeUnlockedVisual.SetActive(safeUnlocked);
        }
    }
}
