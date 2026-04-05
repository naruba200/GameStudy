using UnityEngine;
using UnityEngine.SceneManagement;

public class SafeTransition : MonoBehaviour
{
    [Header("Safe Identification")]
    [SerializeField] private string safeId = "safe_main";
    
    [Header("Scene Transition")]
    [SerializeField] private string safeOpenScene = "SafeOpen";
    
    [Header("Reset on Game Start")]
    [SerializeField] private bool resetSafeOnGameStart = true;
    
    [Header("Visual States - Locked")]
    [SerializeField] private GameObject[] lockedVisuals;
    
    [Header("Visual States - Unlocked")]
    [SerializeField] private GameObject[] unlockedVisuals;
    
    private bool playerInRange = false;
    private bool safeUnlocked = false;
    private static bool hasResetOnGameStart = false;
    
    // Track the player instance disabled directly, so we can re-enable it even when inactive
    private static GameObject hiddenPlayerInstance;

    private void Start()
    {
        // Re-enable player if we have a reference to the hidden one AND we are not in the safe puzzle scene
        if (hiddenPlayerInstance != null && SceneManager.GetActiveScene().name != safeOpenScene)
        {
            hiddenPlayerInstance.SetActive(true);
            Debug.Log("Player re-enabled from static reference");
            hiddenPlayerInstance = null;
        }
        
        if (resetSafeOnGameStart && !hasResetOnGameStart)
        {
            PlayerPrefs.DeleteKey("SafeUnlocked_" + safeId);
            PlayerPrefs.Save();
            hasResetOnGameStart = true;
            Debug.Log("Safe " + safeId + " reset to locked state on game start");
        }

        safeUnlocked = PlayerPrefs.GetInt("SafeUnlocked_" + safeId, 0) == 1;
        UpdateVisuals();
    }

    private void Update()
    {
        if (!playerInRange) return;
        if (safeUnlocked) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            // Disable player so it's not in SafeOpen and store reference directly since FindWithTag fails on inactive objects
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                player.SetActive(false);
                hiddenPlayerInstance = player;
                Debug.Log("Player disabled for safe puzzle");
            }
            
            // Load SafeOpen scene
            SceneManager.LoadScene(safeOpenScene);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }

    private void UpdateVisuals()
    {
        if (lockedVisuals != null)
        {
            foreach (GameObject visual in lockedVisuals)
            {
                if (visual != null)
                    visual.SetActive(!safeUnlocked);
            }
        }

        if (unlockedVisuals != null)
        {
            foreach (GameObject visual in unlockedVisuals)
            {
                if (visual != null)
                    visual.SetActive(safeUnlocked);
            }
        }
    }
}
