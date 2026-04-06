using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class Screen : MonoBehaviour
{
    private static bool hasStartedThisSession;
    private static bool autoStartDialogueOnLoad;
    private static bool isGameOver;
    private static Screen activeInstance;
    private static string cachedGameSceneName = "Outside";
    private static string cachedMenuSceneName = "MainMenu";
    private static Font cachedLoadingFont;
    private static readonly System.Collections.Generic.HashSet<Button> fallbackBoundButtons = new System.Collections.Generic.HashSet<Button>();
    private readonly System.Collections.Generic.HashSet<Button> reboundButtons = new System.Collections.Generic.HashSet<Button>();

    public static bool IsGameOver => isGameOver;

    private void Awake()
    {
        activeInstance = this;
    }

    private void OnEnable()
    {
        activeInstance = this;
    }

    private void OnDisable()
    {
        if (activeInstance == this)
        {
            activeInstance = null;
        }
    }

    private void OnDestroy()
    {
        if (activeInstance == this)
        {
            activeInstance = null;
        }
    }

    public static void ResetStartSessionFlag()
    {
        hasStartedThisSession = false;
    }

    public static void PrepareForStartMenuReturn()
    {
        hasStartedThisSession = false;
        autoStartDialogueOnLoad = false;
        isGameOver = false;

        PlayerPrefs.SetInt("HasPendingSpawn", 0);
        PlayerPrefs.DeleteKey("SpawnPointId");
        PlayerPrefs.DeleteKey("SpawnX");
        PlayerPrefs.DeleteKey("SpawnY");
        PlayerPrefs.Save();

        SessionPlaytime.Reset();
        SaveGameService.ClearPendingLoadState();
    }

    public static bool TriggerGameOver()
    {
        if (isGameOver)
        {
            return false;
        }

        isGameOver = true;

        activeInstance = ResolveActiveScreenInstance();

        if (activeInstance != null)
        {
            activeInstance.ShowDeathScreen();
        }
        else
        {
            ShowDeathScreenWithoutScreenInstance();
        }

        return true;
    }

    public static bool ReturnToStartMenu(string preferredMenuSceneName, string fallbackSceneName)
    {
        PrepareForStartMenuReturn();
        Time.timeScale = 1f;
        SceneLoadingOverlay.ForceHide();

        InventoryToggleUI.ResetPersistentInstance();
        PlayerPersist.ResetPersistentInstance();
        EnemyAI.ResetPersistentInstance();

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
    public string gameSceneName = "Outside";
    public string menuSceneName = "MainMenu";
    public GameObject dialogueManager;

    [Header("Start Screen")]
    public GameObject startScreen;
    public bool pauseGameWhenStartScreenVisible = true;
    [SerializeField] private bool resetGameStateOnStart = true;

    [Header("Death Screen")]
    [SerializeField] private GameObject deathScreen;
    [SerializeField] private bool pauseGameWhenDeathScreenVisible = true;

    [Header("Loading Screen")]
    [SerializeField] private Font loadingFont;

    private void Start()
    {
        CacheSceneSettings();
        EnsureStartScreenReference();
        EnsureStartScreenButtonBindings();
        EnsureDeathScreenReference();
        EnsureDeathScreenButtonBindings();

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
        CacheSceneSettings();
        hasStartedThisSession = true;
        isGameOver = false;
        Time.timeScale = 1f;

        if (startScreen != null)
        {
            startScreen.SetActive(false);
        }

        if (deathScreen != null)
        {
            deathScreen.SetActive(false);
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
        CacheSceneSettings();
        if (!SaveGameService.TryPrepareContinue(out string sceneToLoad))
        {
            Debug.LogWarning("Continue requested but no valid save was found. Falling back to StartGame.");
            StartGame();
            return;
        }

        DialogueManager.ResetGlobalDialogueState();

        hasStartedThisSession = true;
        isGameOver = false;
        autoStartDialogueOnLoad = false;
        Time.timeScale = 1f;

        if (startScreen != null)
        {
            startScreen.SetActive(false);
        }

        if (deathScreen != null)
        {
            deathScreen.SetActive(false);
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
        CacheSceneSettings();
        hasStartedThisSession = true;
        autoStartDialogueOnLoad = true;
        isGameOver = false;
        Time.timeScale = 1f;

        if (deathScreen != null)
        {
            deathScreen.SetActive(false);
        }

        if (startScreen != null)
        {
            startScreen.SetActive(false);
        }

        ResetSessionState();
        string sceneToLoad = ResolveStartSceneName();
        SceneLoadingOverlay.LoadScene(sceneToLoad, loadingFont);
    }

    // Về menu chính
    public void BackToMenu()
    {
        CacheSceneSettings();
        EnsureStartScreenReference();
        Time.timeScale = 1f;

        if (deathScreen != null)
        {
            deathScreen.SetActive(false);
        }

        // Prefer showing the in-scene start screen first (same behavior as exit from selection screen).
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

    public void ReturnToMainMenu()
    {
        BackToMenu();
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
        isGameOver = false;

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
        EnemyAI.ResetPersistentInstance();
    }

    private static void ResetSessionStateForFallbackRetry()
    {
        isGameOver = false;

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
        EnemyAI.ResetPersistentInstance();
    }

    private void CacheSceneSettings()
    {
        cachedGameSceneName = gameSceneName;
        cachedMenuSceneName = menuSceneName;
        cachedLoadingFont = loadingFont;
    }

    private string ResolveStartSceneName()
    {
        if (!string.IsNullOrWhiteSpace(gameSceneName) && Application.CanStreamedLevelBeLoaded(gameSceneName))
        {
            return gameSceneName;
        }

        if (Application.CanStreamedLevelBeLoaded("Outside"))
        {
            return "Outside";
        }

        string currentSceneName = SceneManager.GetActiveScene().name;
        return currentSceneName;
    }

    private void TryStartDialogue()
    {
        DialogueManager manager = null;

        if (dialogueManager != null)
        {
            manager = dialogueManager.GetComponent<DialogueManager>();
        }

        if (manager == null)
        {
            manager = DialogueManager.Resolve();
        }

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

        if (deathScreen != null)
        {
            deathScreen.SetActive(false);
        }

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

    private void ShowDeathScreen()
    {
        SceneLoadingOverlay.ForceHide();
        EnsureDeathScreenReference();
        EnsureDeathScreenButtonBindings();
        BindDeathScreenButtonsFallback(deathScreen);
        EnsureEventSystemExistsStatic();

        if (deathScreen != null)
        {
            deathScreen.SetActive(true);

            CanvasGroup group = deathScreen.GetComponent<CanvasGroup>();
            if (group != null)
            {
                group.alpha = 1f;
                group.interactable = true;
                group.blocksRaycasts = true;
            }
        }

        InventoryToggleUI inventoryToggle = Object.FindFirstObjectByType<InventoryToggleUI>();
        if (inventoryToggle != null)
        {
            inventoryToggle.ForceCloseAllInventoryUI();
            inventoryToggle.SetInventoryAccess(false);
        }

        PlayerController player = PlayerPersist.GetPlayerController();
        if (player != null)
        {
            player.StopMovement();
        }

        if (pauseGameWhenDeathScreenVisible)
        {
            Time.timeScale = 0f;
        }
    }

    private void EnsureEventSystemExists()
    {
        EnsureEventSystemExistsStatic();
    }

    private static void EnsureEventSystemExistsStatic()
    {
        EventSystem current = Object.FindFirstObjectByType<EventSystem>();
        if (current != null && current.gameObject.activeInHierarchy)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        eventSystemObject.tag = "Untagged";
    }

    private static Screen ResolveActiveScreenInstance()
    {
        if (activeInstance != null)
        {
            return activeInstance;
        }

        Screen found = Object.FindFirstObjectByType<Screen>();
        if (found != null)
        {
            return found;
        }

        Screen[] allScreens = Resources.FindObjectsOfTypeAll<Screen>();
        for (int i = 0; i < allScreens.Length; i++)
        {
            Screen candidate = allScreens[i];
            if (candidate == null || candidate.gameObject == null)
            {
                continue;
            }

            if (!candidate.gameObject.scene.IsValid() || !candidate.gameObject.scene.isLoaded)
            {
                continue;
            }

            return candidate;
        }

        return null;
    }

    private static void ShowDeathScreenWithoutScreenInstance()
    {
        cachedGameSceneName = SceneManager.GetActiveScene().name;
        SceneLoadingOverlay.ForceHide();
        EnsureEventSystemExistsStatic();

        InventoryToggleUI inventoryToggle = Object.FindFirstObjectByType<InventoryToggleUI>();
        if (inventoryToggle != null)
        {
            inventoryToggle.ForceCloseAllInventoryUI();
            inventoryToggle.SetInventoryAccess(false);
        }

        PlayerController player = PlayerPersist.GetPlayerController();
        if (player != null)
        {
            player.StopMovement();
        }

        GameObject death = FindInLoadedScenesByName("DeathScreen");
        if (death == null)
        {
            death = FindInLoadedScenesByName("GameOverScreen");
        }

        if (death == null)
        {
            death = FindInLoadedScenesByName("deathscreen");
        }

        if (death != null)
        {
            death.SetActive(true);
            BindDeathScreenButtonsFallback(death);

            CanvasGroup group = death.GetComponent<CanvasGroup>();
            if (group != null)
            {
                group.alpha = 1f;
                group.interactable = true;
                group.blocksRaycasts = true;
            }
        }
        else
        {
            Debug.LogWarning("GameOver triggered but no Screen and no DeathScreen object were found in loaded scenes.");
        }

        Time.timeScale = 0f;
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

    private void EnsureDeathScreenReference()
    {
        if (deathScreen != null)
        {
            return;
        }

        deathScreen = FindInLoadedScenesByName("DeathScreen");
        if (deathScreen == null)
        {
            deathScreen = FindInLoadedScenesByName("GameOverScreen");
        }

        if (deathScreen == null)
        {
            deathScreen = FindInLoadedScenesByName("deathscreen");
        }
    }

    private void EnsureDeathScreenButtonBindings()
    {
        if (deathScreen == null)
        {
            return;
        }

        BindDeathScreenButtonsFallback(deathScreen);

        Button[] buttons = deathScreen.GetComponentsInChildren<Button>(true);
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

    private static void BindDeathScreenButtonsFallback(GameObject deathScreenObject)
    {
        if (deathScreenObject == null)
        {
            return;
        }

        Button[] buttons = deathScreenObject.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            if (button == null || fallbackBoundButtons.Contains(button))
            {
                continue;
            }

            string label = GetButtonLabel(button);
            if (IsRetryButton(label, button.name))
            {
                button.onClick.RemoveListener(RetryGameFromDeathScreen);
                button.onClick.AddListener(RetryGameFromDeathScreen);
                fallbackBoundButtons.Add(button);
                continue;
            }

            if (IsReturnButton(label, button.name))
            {
                button.onClick.RemoveListener(ReturnToMainMenuFromDeathScreen);
                button.onClick.AddListener(ReturnToMainMenuFromDeathScreen);
                fallbackBoundButtons.Add(button);
            }
        }
    }

    private static void RetryGameFromDeathScreen()
    {
        CacheSceneSettingsForFallback();
        hasStartedThisSession = true;
        autoStartDialogueOnLoad = true;
        isGameOver = false;
        Time.timeScale = 1f;

        SceneLoadingOverlay.ForceHide();
        ResetSessionStateForFallbackRetry();

        string sceneToLoad = ResolveRetrySceneName();
        SceneLoadingOverlay.LoadScene(sceneToLoad, cachedLoadingFont);
    }

    private static void ReturnToMainMenuFromDeathScreen()
    {
        CacheSceneSettingsForFallback();
        isGameOver = false;
        hasStartedThisSession = false;  // This makes the start screen show when Outside loads
        autoStartDialogueOnLoad = false;
        Time.timeScale = 1f;

        PlayerPrefs.SetInt("HasPendingSpawn", 0);
        PlayerPrefs.DeleteKey("SpawnPointId");
        PlayerPrefs.DeleteKey("SpawnX");
        PlayerPrefs.DeleteKey("SpawnY");
        PlayerPrefs.Save();

        SessionPlaytime.Reset();
        SaveGameService.ClearPendingLoadState();

        InventoryToggleUI.ResetPersistentInstance();
        PlayerPersist.ResetPersistentInstance();
        EnemyAI.ResetPersistentInstance();

        // Use SceneLoadingOverlay to show loading screen and prevent visible scene flash
        SceneLoadingOverlay.LoadScene("Outside", cachedLoadingFont);
    }

    private static void CacheSceneSettingsForFallback()
    {
        if (!string.IsNullOrWhiteSpace(cachedGameSceneName) && !string.IsNullOrWhiteSpace(cachedMenuSceneName))
        {
            return;
        }

        Screen screen = activeInstance != null ? activeInstance : Object.FindFirstObjectByType<Screen>();
        if (screen == null)
        {
            return;
        }

        cachedGameSceneName = screen.gameSceneName;
        cachedMenuSceneName = screen.menuSceneName;
        cachedLoadingFont = screen.loadingFont;
    }

    private static string ResolveGameSceneName()
    {
        if (!string.IsNullOrWhiteSpace(cachedGameSceneName) && Application.CanStreamedLevelBeLoaded(cachedGameSceneName))
        {
            return cachedGameSceneName;
        }

        string currentSceneName = SceneManager.GetActiveScene().name;
        return currentSceneName;
    }

    private static string ResolveRetrySceneName()
    {
        if (Application.CanStreamedLevelBeLoaded("Outside"))
        {
            return "Outside";
        }

        return ResolveGameSceneName();
    }

    private static string ResolveMenuSceneName()
    {
        if (Application.CanStreamedLevelBeLoaded("MainMenu"))
        {
            return "MainMenu";
        }

        string currentSceneName = SceneManager.GetActiveScene().name;
        return currentSceneName;
    }

    private static string GetButtonLabel(Button button)
    {
        if (button == null)
        {
            return string.Empty;
        }

        TMP_Text tmpText = button.GetComponentInChildren<TMP_Text>(true);
        if (tmpText != null && !string.IsNullOrWhiteSpace(tmpText.text))
        {
            return tmpText.text;
        }

        Text legacyText = button.GetComponentInChildren<Text>(true);
        if (legacyText != null && !string.IsNullOrWhiteSpace(legacyText.text))
        {
            return legacyText.text;
        }

        return button.name;
    }

    private static bool IsRetryButton(string label, string objectName)
    {
        string combined = (label + " " + objectName).ToLowerInvariant();
        return combined.Contains("retry") || combined.Contains("restart") || combined.Contains("play again");
    }

    private static bool IsReturnButton(string label, string objectName)
    {
        string combined = (label + " " + objectName).ToLowerInvariant();
        return combined.Contains("return") || combined.Contains("menu") || combined.Contains("main menu");
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
            case nameof(ReturnToMainMenu):
                button.onClick.AddListener(ReturnToMainMenu);
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
