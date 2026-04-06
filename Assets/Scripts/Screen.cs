using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Screen : MonoBehaviour
{
    private static bool hasStartedThisSession;
    private static bool autoStartDialogueOnLoad;
    private readonly System.Collections.Generic.HashSet<Button> reboundButtons = new System.Collections.Generic.HashSet<Button>();

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
    }

    public static bool ReturnToStartMenu(string preferredMenuSceneName, string fallbackSceneName)
    {
        PrepareForStartMenuReturn();
        Time.timeScale = 1f;
        SceneLoadingOverlay.ForceHide();

        InventoryToggleUI.ResetPersistentInstance();
        PlayerPersist.ResetPersistentInstance();
        CanvasFollowPlayerPersist.ResetPersistentInstance();

        if (!string.IsNullOrWhiteSpace(preferredMenuSceneName) && Application.CanStreamedLevelBeLoaded(preferredMenuSceneName))
        {
            SceneManager.LoadScene(preferredMenuSceneName);
            return true;
        }

        if (!string.IsNullOrWhiteSpace(fallbackSceneName) && Application.CanStreamedLevelBeLoaded(fallbackSceneName))
        {
            SceneManager.LoadScene(fallbackSceneName);
            return true;
        }

        return false;
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
        EnsureStartScreenReference();
        EnsureStartScreenButtonBindings();

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

        if (startScreen != null)
        {
            startScreen.SetActive(true);
        }

        bool shouldPause = pauseGameWhenStartScreenVisible && startScreen != null && startScreen.activeInHierarchy;
        Time.timeScale = shouldPause ? 0f : 1f;

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

        DialogueManager.ResetGlobalDialogueState();

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
        SceneManager.LoadScene(gameSceneName);
    }

    // Về menu chính
    public void BackToMenu()
    {
        EnsureStartScreenReference();
        Time.timeScale = 1f;

        if (TryShowStartScreenInCurrentScene())
        {
            return;
        }

        string fallbackScene = ResolveStartSceneName();
        if (ReturnToStartMenu(menuSceneName, fallbackScene))
        {
            return;
        }

        Debug.LogWarning("BackToMenu failed: no start screen in current scene and menu scene is not loadable.");
    }

    // Thoát game
    public void QuitGame()
    {
        EnsureStartScreenReference();
        bool isStartScreenVisible = startScreen != null && startScreen.activeInHierarchy;

        // When called from in-game inventory, always return to start menu instead of closing app.
        if (!isStartScreenVisible)
        {
            BackToMenu();
            return;
        }

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

    private bool TryShowStartScreenInCurrentScene()
    {
        EnsureStartScreenReference();

        if (startScreen == null)
        {
            return false;
        }

        PrepareForStartMenuReturn();

        InventoryToggleUI inventoryToggle = Object.FindFirstObjectByType<InventoryToggleUI>();
        if (inventoryToggle != null)
        {
            inventoryToggle.ForceCloseAllInventoryUI();
            inventoryToggle.SetInventoryAccess(false);
        }

        SceneLoadingOverlay.ForceHide();

        EnsureStartScreenButtonBindings();

        startScreen.SetActive(true);
        Time.timeScale = pauseGameWhenStartScreenVisible ? 0f : 1f;

        ApplyInventoryVisibilityForCurrentScreen();
        return true;
    }

    private void EnsureStartScreenReference()
    {
        if (startScreen != null)
        {
            return;
        }

        startScreen = FindInLoadedScenesByName("StartingScreen");
        if (startScreen == null)
        {
            startScreen = FindInLoadedScenesByName("StartScreen");
        }
    }

    private void EnsureStartScreenButtonBindings()
    {
        if (startScreen == null)
        {
            return;
        }

        Button[] buttons = startScreen.GetComponentsInChildren<Button>(true);
        for (int b = 0; b < buttons.Length; b++)
        {
            Button button = buttons[b];
            if (button == null || reboundButtons.Contains(button))
            {
                continue;
            }

            int eventCount = button.onClick.GetPersistentEventCount();
            for (int i = 0; i < eventCount; i++)
            {
                string methodName = button.onClick.GetPersistentMethodName(i);
                if (string.IsNullOrWhiteSpace(methodName))
                {
                    continue;
                }

                Object target = button.onClick.GetPersistentTarget(i);
                if (target != null)
                {
                    continue;
                }

                if (!TryAddRuntimeBinding(button, methodName))
                {
                    continue;
                }

                reboundButtons.Add(button);
                break;
            }
        }
    }

    private bool TryAddRuntimeBinding(Button button, string methodName)
    {
        switch (methodName)
        {
            case nameof(StartGame):
                button.onClick.AddListener(StartGame);
                return true;
            case nameof(ContinueGame):
                button.onClick.AddListener(ContinueGame);
                return true;
            case nameof(BackToMenu):
                button.onClick.AddListener(BackToMenu);
                return true;
            case nameof(QuitGame):
                button.onClick.AddListener(QuitGame);
                return true;
            case nameof(RetryGame):
                button.onClick.AddListener(RetryGame);
                return true;
            case nameof(SaveGameNow):
                button.onClick.AddListener(SaveGameNow);
                return true;
            default:
                return false;
        }
    }

    private static GameObject FindInLoadedScenesByName(string objectName)
    {
        if (string.IsNullOrWhiteSpace(objectName))
        {
            return null;
        }

        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < allObjects.Length; i++)
        {
            GameObject candidate = allObjects[i];
            if (candidate == null || candidate.name != objectName)
            {
                continue;
            }

            if (!candidate.scene.IsValid() || !candidate.scene.isLoaded)
            {
                continue;
            }

            return candidate;
        }

        return null;
    }
}
