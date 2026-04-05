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

    // Tên scene (set trong Inspector)
    public string gameSceneName = "Game";
    public string menuSceneName = "MainMenu";
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
            Time.timeScale = 1f;
        }

        ApplyInventoryVisibilityForCurrentScreen();
    }

    // Bấm nút Bắt đầu: tắt màn hình start và vào game
    public void StartGame()
    {
        hasStartedThisSession = true;
        Time.timeScale = 1f;

        if (resetGameStateOnStart)
        {
            ResetSessionState();

            string sceneToLoad = ResolveStartSceneName();
            autoStartDialogueOnLoad = true;

            SceneLoadingOverlay.LoadScene(sceneToLoad, loadingFont);
            return;
        }

        if (startScreen != null)
        {
            startScreen.SetActive(false);
        }

        TryStartDialogue();
    }

    // Retry - chơi lại
    public void RetryGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    // Về menu chính
    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
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
