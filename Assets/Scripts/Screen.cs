using UnityEngine;
using UnityEngine.SceneManagement;

public class Screen : MonoBehaviour
{
    private static bool hasStartedThisSession;
    private static bool autoStartDialogueOnLoad;

    public static void ResetStartSessionFlag()
    {
        hasStartedThisSession = false;
    }

    public static void PrepareForStartMenuReturn()
    {
        hasStartedThisSession = false;
        autoStartDialogueOnLoad = false;

        PlayerPrefs.SetInt("HasPendingSpawn", 0);
        PlayerPrefs.DeleteKey("SpawnPointId");
        PlayerPrefs.DeleteKey("SpawnX");
        PlayerPrefs.DeleteKey("SpawnY");
        PlayerPrefs.Save();

        SessionPlaytime.Reset();
        SaveGameService.ClearPendingLoadState();

        InventoryToggleUI.ResetPersistentInstance();
        PlayerPersist.ResetPersistentInstance();
    }

    // Tên scene (set trong Inspector)
    public string gameSceneName = "Outside";
    public string menuSceneName = "Outside";
    public GameObject dialogueManager;

    [Header("Start Screen")]
    public GameObject startScreen;
    public bool pauseGameWhenStartScreenVisible = true;
    [SerializeField] private bool resetGameStateOnStart = true;

    [Header("Loading Screen")]
    [SerializeField] private Font loadingFont;

    private void Start()
    {
        if (hasStartedThisSession)
        {
            if (startScreen != null)
            {
                startScreen.SetActive(false);
            }

            Time.timeScale = 1f;

            if (autoStartDialogueOnLoad)
            {
                autoStartDialogueOnLoad = false;
                TryStartDialogue();
            }

            ApplyInventoryVisibilityForCurrentScreen();
            return;
        }

        if (pauseGameWhenStartScreenVisible && startScreen != null && startScreen.activeSelf)
        {
            Time.timeScale = 0f;
        }
        else
        {
            if (startScreen != null)
            {
                startScreen.SetActive(true);
            }

            Time.timeScale = 1f;
        }

        ApplyInventoryVisibilityForCurrentScreen();
    }

    // Bấm nút Bắt đầu: tắt màn hình start và vào game
    public void StartGame()
    {
        hasStartedThisSession = true;
        Time.timeScale = 1f;

        if (startScreen != null)
        {
            startScreen.SetActive(false);
        }

        if (resetGameStateOnStart)
        {
            ResetSessionState();

            string sceneToLoad = ResolveStartSceneName();
            autoStartDialogueOnLoad = true;

            SceneLoadingOverlay.LoadScene(sceneToLoad, loadingFont);
            return;
        }

        TryStartDialogue();
    }

    public void ContinueGame()
    {
        if (!SaveGameService.TryPrepareContinue(out string sceneToLoad))        
        {
            Debug.LogWarning("Continue requested but no valid save was found. Falling back to StartGame.");
            StartGame();
            return;
        }

        hasStartedThisSession = true;
        autoStartDialogueOnLoad = false;
        Time.timeScale = 1f;

        if (startScreen != null)
        {
            startScreen.SetActive(false);
        }

        InventoryToggleUI inventoryToggle = Object.FindFirstObjectByType<InventoryToggleUI>();
        if (inventoryToggle != null)
        {
            inventoryToggle.SetInventoryAccess(true);
        }

        PlayerController player = PlayerPersist.GetPlayerController();
        if (player != null)
        {
            player.ResumeMovement();
        }

        ApplyInventoryVisibilityForCurrentScreen();
        SceneLoadingOverlay.LoadScene(sceneToLoad, loadingFont);
    }

    public void SaveGameNow()
    {
        bool saved = SaveGameService.SaveCurrentGame();
        if (!saved)
        {
            Debug.LogWarning("SaveGameNow failed.");
        }
    }

    // Retry - chơi lại
    public void RetryGame()
    {
        if (!string.IsNullOrWhiteSpace(gameSceneName) && Application.CanStreamedLevelBeLoaded(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
            return;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Về menu chính
    public void BackToMenu()
    {
        Time.timeScale = 1f;

        if (!string.IsNullOrWhiteSpace(menuSceneName) && Application.CanStreamedLevelBeLoaded(menuSceneName))
        {
            SceneManager.LoadScene(menuSceneName);
            return;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Thoát game
    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Time.timeScale = 1f;
        Application.Quit();
    }

    private void ResetSessionState()
    {
        PlayerPrefs.SetInt("HasPendingSpawn", 0);
        PlayerPrefs.DeleteKey("SpawnPointId");
        PlayerPrefs.DeleteKey("SpawnX");
        PlayerPrefs.DeleteKey("SpawnY");
        PlayerPrefs.Save();

        SessionPlaytime.Reset();
        GameWorldState.Reset();
        SaveGameService.ClearPendingLoadState();

        InventoryToggleUI.ResetPersistentInstance();
        PlayerPersist.ResetPersistentInstance();
    }

    private string ResolveStartSceneName()
    {
        if (!string.IsNullOrWhiteSpace(gameSceneName) && Application.CanStreamedLevelBeLoaded(gameSceneName))
        {
            return gameSceneName;
        }

        string currentSceneName = SceneManager.GetActiveScene().name;
        return currentSceneName;
    }

    private void TryStartDialogue()
    {
        if (dialogueManager == null)
        {
            return;
        }

        DialogueManager manager = dialogueManager.GetComponent<DialogueManager>();
        if (manager != null)
        {
            manager.StartDialogue();
        }
    }

    private void ApplyInventoryVisibilityForCurrentScreen()
    {
        bool startMenuVisible = startScreen != null && startScreen.activeInHierarchy;
        InventoryToggleUI inventoryToggle = Object.FindFirstObjectByType<InventoryToggleUI>();
        if (inventoryToggle != null)
        {
            inventoryToggle.SetInventoryAccess(!startMenuVisible);
            return;
        }

        if (!startMenuVisible)
        {
            return;
        }

        GameObject showMainInventory = GameObject.Find("ShowMainInventory");    
        if (showMainInventory != null)
        {
            showMainInventory.SetActive(false);
        }
    }
}
