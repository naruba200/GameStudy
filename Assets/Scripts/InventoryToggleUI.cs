using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InventoryToggleUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject showMainInventory;
    [SerializeField] private GameObject generalUIPanel;
    [SerializeField] private GameObject inventoryUIPanel;
    [SerializeField] private GameObject saveUIPanel;
    [SerializeField] private Image generalUIIconImage;
    [SerializeField] private TMP_Text generalUINameText;
    [SerializeField] private TMP_Text saveStatusText;
    [SerializeField] private TMP_Text saveResultText;
    [SerializeField] private TMP_Text saveSlotNameText;
    [SerializeField] private TMP_Text savePlaytimeText;
    [SerializeField] private TMP_Text saveDateText;
    [SerializeField] private Image savePreviewImage;

    [Header("Player")]
    [SerializeField] private PlayerController player;
    [SerializeField] private string playerSpriteObjectName = "M_08_0";

    [Header("General UI Icon")]
    [SerializeField] private Sprite playerIconSpriteOverride;
    [SerializeField] private GameObject playerIconSourceObject;
    [SerializeField] private bool lockIconAfterResolve = true;

    [Header("Hotkeys")]
    [SerializeField] private KeyCode openInventoryKey = KeyCode.B;
    [SerializeField] private KeyCode closeInventoryKey = KeyCode.Escape;

    [Header("Startup")]
    [SerializeField] private bool startWithInventoryClosed = true;

    [Header("Debug")]
    [SerializeField] private bool keepInSceneHierarchyDuringPlay;

    private static InventoryToggleUI instance;
    private Screen screen;
    private bool inventoryAccessEnabled = true;
    private bool lastStartScreenVisible;
    private SpriteRenderer playerSpriteRenderer;
    private bool wasDialogueActive;
    private Sprite resolvedPlayerIcon;
    private bool hasResolvedPlayerIcon;
    private bool lastSaveSucceeded;

    public static void ResetPersistentInstance()
    {
        if (instance == null)
        {
            return;
        }

        Destroy(instance.gameObject);
        instance = null;
    }

    public static bool IsInventoryOpen()
    {
        if (instance == null || instance.inventoryPanel == null)
        {
            return false;
        }

        return instance.inventoryPanel.activeSelf;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        if (!(Application.isEditor && keepInSceneHierarchyDuringPlay))
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        TryResolveReferences();
        SyncWithStartScreen();
        UpdateGeneralUIPresentation();

        if (startWithInventoryClosed)
        {
            CloseInventory();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryResolveReferences();
        SyncWithStartScreen();
        UpdateGeneralUIPresentation();
    }

    private void Update()
    {
        SyncWithStartScreen();
        TryResolveReferences();
        UpdateGeneralUIPresentation();

        bool isDialogueActive = DialogueManager.IsAnyDialogueActive;
        if (isDialogueActive)
        {
            wasDialogueActive = true;
        }
        else if (wasDialogueActive)
        {
            wasDialogueActive = false;
            RefreshBackpackButtonVisibility();
        }

        if (isDialogueActive)
        {
            if (inventoryPanel != null && inventoryPanel.activeSelf)
            {
                CloseInventoryInternal(false, false);
            }

            RefreshBackpackButtonVisibility();

            return;
        }

        RefreshBackpackButtonVisibility();

        if (!inventoryAccessEnabled)
        {
            return;
        }

        if (Input.GetKeyDown(openInventoryKey))
        {
            OpenInventory();
        }

        if (Input.GetKeyDown(closeInventoryKey))
        {
            CloseInventory();
        }
    }

    public void OpenInventory()
    {
        TryResolveReferences();

        if (!inventoryAccessEnabled || DialogueManager.IsAnyDialogueActive)
        {
            return;
        }

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(true);
        }

        ShowGeneralUI();

        if (showMainInventory != null)
        {
            RefreshBackpackButtonVisibility();
        }

        if (player != null)
        {
            player.StopMovement();
        }
    }

    public void CloseInventory()
    {
        TryResolveReferences();
        CloseInventoryInternal(true, true);
    }

    public void SetInventoryAccess(bool enabled)
    {
        inventoryAccessEnabled = enabled;

        if (!enabled)
        {
            ForceCloseAllInventoryUI();

            if (player != null)
            {
                player.StopMovement();
            }

            return;
        }

        RefreshBackpackButtonVisibility();

        if (player != null)
        {
            player.ResumeMovement();
        }
    }

    public void ForceCloseAllInventoryUI()
    {
        CloseInventoryInternal(false, false);

        if (generalUIPanel != null)
        {
            generalUIPanel.SetActive(false);
        }

        if (inventoryUIPanel != null)
        {
            inventoryUIPanel.SetActive(false);
        }

        if (saveUIPanel != null)
        {
            saveUIPanel.SetActive(false);
        }

        HideAllShowMainInventoryObjects();
    }

    public void ToggleInventory()
    {
        bool isOpen = inventoryPanel != null && inventoryPanel.activeSelf;

        if (isOpen)
        {
            CloseInventory();
        }
        else
        {
            OpenInventory();
        }
    }

    public void ShowGeneralUI()
    {
        if (generalUIPanel != null)
        {
            generalUIPanel.SetActive(true);
        }

        if (inventoryUIPanel != null)
        {
            inventoryUIPanel.SetActive(false);
        }

        if (saveUIPanel != null)
        {
            saveUIPanel.SetActive(false);
        }
    }

    public void ShowInventoryUI()
    {
        if (generalUIPanel != null)
        {
            generalUIPanel.SetActive(false);
        }

        if (inventoryUIPanel != null)
        {
            inventoryUIPanel.SetActive(true);
        }

        if (saveUIPanel != null)
        {
            saveUIPanel.SetActive(false);
        }
    }

    public void ShowSaveUI()
    {
        if (generalUIPanel != null)
        {
            generalUIPanel.SetActive(false);
        }

        if (inventoryUIPanel != null)
        {
            inventoryUIPanel.SetActive(false);
        }

        if (saveUIPanel != null)
        {
            saveUIPanel.SetActive(true);
        }

        RefreshSavePreviewUI();
        SaveCurrentGame();
    }

    public void SaveCurrentGame()
    {
        bool saved = SaveGameService.SaveCurrentGame();
        UpdateSaveStatusText(saved);
        RefreshSavePreviewUI();
        if (!saved)
        {
            Debug.LogWarning("SaveCurrentGame from InventoryToggleUI failed.");
        }
    }

    public void SaveFromUIButton()
    {
        SaveCurrentGame();
    }

    public void CloseSaveUI()
    {
        ShowGeneralUI();
    }

    private void TryResolveReferences()
    {
        if (inventoryPanel == null)
        {
            inventoryPanel = FindInLoadedScenesByName("Inventory");
        }

        if (showMainInventory == null)
        {
            showMainInventory = FindInLoadedScenesByName("ShowMainInventory");
        }

        if (generalUIPanel == null)
        {
            generalUIPanel = FindInLoadedScenesByName("GeneralUI");
        }

        if (inventoryUIPanel == null)
        {
            inventoryUIPanel = FindInLoadedScenesByName("InventoryUI");
        }

        if (saveUIPanel == null)
        {
            saveUIPanel = FindInLoadedScenesByName("SaveUI");
        }

        if (player == null)
        {
            PlayerController persistentPlayer = PlayerPersist.GetPlayerController();
            if (persistentPlayer != null)
            {
                player = persistentPlayer;
            }
        }

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.GetComponent<PlayerController>();
                playerSpriteRenderer = playerObject.GetComponentInChildren<SpriteRenderer>(true);
            }
        }
        else if (playerSpriteRenderer == null)
        {
            playerSpriteRenderer = player.GetComponentInChildren<SpriteRenderer>(true);
        }

        if (playerSpriteRenderer == null && !string.IsNullOrWhiteSpace(playerSpriteObjectName))
        {
            GameObject spriteObject = FindInLoadedScenesByName(playerSpriteObjectName);
            if (spriteObject != null)
            {
                playerSpriteRenderer = spriteObject.GetComponent<SpriteRenderer>();
                if (playerSpriteRenderer == null)
                {
                    playerSpriteRenderer = spriteObject.GetComponentInChildren<SpriteRenderer>(true);
                }
            }
        }

        ResolvePlayerIcon();

        if (screen == null)
        {
            screen = Object.FindFirstObjectByType<Screen>();
        }

        if (generalUIPanel != null)
        {
            if (generalUIIconImage == null)
            {
                Transform iconTransform = generalUIPanel.transform.Find("ItemIcon");
                if (iconTransform != null)
                {
                    generalUIIconImage = iconTransform.GetComponent<Image>();
                }
                else
                {
                    generalUIIconImage = CreateFallbackIconImage(generalUIPanel.transform);
                }
            }

            if (generalUINameText == null)
            {
                Transform nameTransform = generalUIPanel.transform.Find("ItemName");
                if (nameTransform != null)
                {
                    generalUINameText = nameTransform.GetComponent<TMP_Text>();
                }
            }
        }

        if (saveUIPanel != null && saveStatusText == null)
        {
            Transform saveStatusTransform = saveUIPanel.transform.Find("ItemName");
            if (saveStatusTransform != null)
            {
                saveStatusText = saveStatusTransform.GetComponent<TMP_Text>();
            }
        }

        if (saveUIPanel != null && saveResultText == null)
        {
            Transform saveResultTransform = saveUIPanel.transform.Find("SaveResultText");
            if (saveResultTransform != null)
            {
                saveResultText = saveResultTransform.GetComponent<TMP_Text>();
            }
        }

        if (saveUIPanel != null)
        {
            EnsureSaveSlotVisuals();

            if (savePlaytimeText == null)
            {
                Transform playtimeTransform = saveUIPanel.transform.Find("SavePlaytimeText");
                if (playtimeTransform != null)
                {
                    savePlaytimeText = playtimeTransform.GetComponent<TMP_Text>();
                }
            }

            if (saveDateText == null)
            {
                Transform dateTransform = saveUIPanel.transform.Find("SaveDateText");
                if (dateTransform != null)
                {
                    saveDateText = dateTransform.GetComponent<TMP_Text>();
                }
            }

            if (savePreviewImage == null)
            {
                Transform imageTransform = saveUIPanel.transform.Find("SaveImage");
                if (imageTransform != null)
                {
                    savePreviewImage = imageTransform.GetComponent<Image>();
                }
            }
        }
    }

    private void CloseInventoryInternal(bool showBackpackButton, bool resumePlayerMovement)
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }

        if (showMainInventory != null)
        {
            bool shouldShow = showBackpackButton && !DialogueManager.IsAnyDialogueActive && !IsStartScreenVisible();
            showMainInventory.SetActive(shouldShow);
        }

        if (resumePlayerMovement && player != null)
        {
            player.ResumeMovement();
        }
    }

    private void SyncWithStartScreen()
    {
        if (screen == null || screen.startScreen == null)
        {
            return;
        }

        bool isStartScreenVisible = screen.startScreen.activeInHierarchy;
        if (isStartScreenVisible == lastStartScreenVisible)
        {
            return;
        }

        lastStartScreenVisible = isStartScreenVisible;
        SetInventoryAccess(!isStartScreenVisible);

        if (isStartScreenVisible)
        {
            HideAllShowMainInventoryObjects();
        }
    }

    private void HideAllShowMainInventoryObjects()
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < allObjects.Length; i++)
        {
            GameObject candidate = allObjects[i];
            if (candidate == null)
            {
                continue;
            }

            if (candidate.name != "ShowMainInventory")
            {
                continue;
            }

            if (!candidate.scene.IsValid() || !candidate.scene.isLoaded)
            {
                continue;
            }

            candidate.SetActive(false);
        }
    }

    private void UpdateGeneralUIPresentation()
    {
        if (generalUINameText != null)
        {
            generalUINameText.text = "You";
        }

        if (generalUIIconImage == null)
        {
            return;
        }

        Sprite playerSprite = resolvedPlayerIcon;
        if (playerSprite == null && !hasResolvedPlayerIcon)
        {
            ResolvePlayerIcon();
            playerSprite = resolvedPlayerIcon;
        }

        if (playerSprite == null && !lockIconAfterResolve)
        {
            playerSprite = playerSpriteRenderer != null ? playerSpriteRenderer.sprite : null;
        }

        generalUIIconImage.sprite = playerSprite;
        generalUIIconImage.enabled = playerSprite != null;
    }

    private void ResolvePlayerIcon()
    {
        if (lockIconAfterResolve && hasResolvedPlayerIcon)
        {
            return;
        }

        if (playerIconSpriteOverride != null)
        {
            resolvedPlayerIcon = playerIconSpriteOverride;
            hasResolvedPlayerIcon = true;
            return;
        }

        if (playerIconSourceObject == null && !string.IsNullOrWhiteSpace(playerSpriteObjectName))
        {
            playerIconSourceObject = FindInLoadedScenesByName(playerSpriteObjectName);
        }

        if (TryResolveSpriteFromObject(playerIconSourceObject, out Sprite iconFromSource))
        {
            resolvedPlayerIcon = iconFromSource;
            hasResolvedPlayerIcon = true;
            return;
        }

        if (playerSpriteRenderer != null && playerSpriteRenderer.sprite != null)
        {
            resolvedPlayerIcon = playerSpriteRenderer.sprite;
            hasResolvedPlayerIcon = true;
            return;
        }

        if (!lockIconAfterResolve)
        {
            hasResolvedPlayerIcon = false;
        }
    }

    private bool TryResolveSpriteFromObject(GameObject source, out Sprite sprite)
    {
        sprite = null;
        if (source == null)
        {
            return false;
        }

        SpriteRenderer renderer = source.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = source.GetComponentInChildren<SpriteRenderer>(true);
        }

        if (renderer != null && renderer.sprite != null)
        {
            sprite = renderer.sprite;
            return true;
        }

        Image image = source.GetComponent<Image>();
        if (image == null)
        {
            image = source.GetComponentInChildren<Image>(true);
        }

        if (image != null && image.sprite != null)
        {
            sprite = image.sprite;
            return true;
        }

        return false;
    }

    private Image CreateFallbackIconImage(Transform generalRoot)
    {
        if (generalRoot == null)
        {
            return null;
        }

        GameObject iconObject = new GameObject("ItemIcon", typeof(RectTransform), typeof(Image));
        iconObject.transform.SetParent(generalRoot, false);

        RectTransform rect = iconObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(18f, -18f);
        rect.sizeDelta = new Vector2(36f, 36f);

        Image image = iconObject.GetComponent<Image>();
        image.raycastTarget = false;
        image.preserveAspect = true;
        image.enabled = false;
        return image;
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

    private void RefreshBackpackButtonVisibility()
    {
        if (showMainInventory == null)
        {
            return;
        }

        bool inventoryOpen = inventoryPanel != null && inventoryPanel.activeSelf;
        bool shouldShow = inventoryAccessEnabled && !DialogueManager.IsAnyDialogueActive && !inventoryOpen && !IsStartScreenVisible();
        showMainInventory.SetActive(shouldShow);
    }

    private bool IsStartScreenVisible()
    {
        return screen != null && screen.startScreen != null && screen.startScreen.activeInHierarchy;
    }

    private void UpdateSaveStatusText(bool saved)
    {
        lastSaveSucceeded = saved;

        SaveGameService.SavePreview preview = SaveGameService.GetSavePreview();

        if (saveSlotNameText != null)
        {
            saveSlotNameText.text = ">File1";
        }

        if (savePlaytimeText != null)
        {
            savePlaytimeText.text = preview.playtimeText;
        }

        if (saveDateText != null)
        {
            saveDateText.text = preview.savedAtText;
        }

        if (saveStatusText == null)
        {
            UpdateSaveResultText(saved);
            return;
        }

        if (saveSlotNameText != null || savePlaytimeText != null || saveDateText != null)
        {
            if (saveStatusText != saveSlotNameText)
            {
                saveStatusText.text = string.Empty;
            }

            UpdateSaveResultText(saved);
            return;
        }

        saveStatusText.text = BuildSaveStatusMessage(preview, saved);
        UpdateSaveResultText(saved);
    }

    private void RefreshSavePreviewUI()
    {
        SaveGameService.SavePreview preview = SaveGameService.GetSavePreview();

        if (savePlaytimeText != null)
        {
            savePlaytimeText.text = preview.playtimeText;
        }

        if (saveDateText != null)
        {
            saveDateText.text = preview.savedAtText;
        }

        if (saveSlotNameText != null)
        {
            saveSlotNameText.text = ">File1";
        }

        if (saveStatusText != null && (savePlaytimeText == null || saveDateText == null))
        {
            saveStatusText.text = BuildSaveStatusMessage(preview, lastSaveSucceeded);
        }

        if (savePreviewImage != null)
        {
            Sprite slotIcon = ResolveCurrentSaveIcon();
            savePreviewImage.sprite = slotIcon;
            savePreviewImage.enabled = preview.hasData && slotIcon != null;
        }
    }

    private string BuildSaveStatusMessage(SaveGameService.SavePreview preview, bool saved)
    {
        if (!saved)
        {
            return "Luu game that bai";
        }

        if (!preview.hasData)
        {
            return "Luu game thanh cong";
        }

        return "Luu game thanh cong\n" + preview.playtimeText + "\n" + preview.savedAtText;
    }

    private void EnsureSaveSlotVisuals()
    {
        if (saveUIPanel == null)
        {
            return;
        }

        if (saveSlotNameText == null)
        {
            if (saveStatusText != null)
            {
                saveSlotNameText = saveStatusText;
            }
            else
            {
                Transform fileNameTransform = saveUIPanel.transform.Find("SaveFileText");
                if (fileNameTransform != null)
                {
                    saveSlotNameText = fileNameTransform.GetComponent<TMP_Text>();
                }
            }
        }

        if (saveSlotNameText == null)
        {
            saveSlotNameText = CreateSaveText(
                saveUIPanel.transform,
                "SaveFileText",
                new Vector2(-215f, 165f),
                new Vector2(200f, 44f),
                TextAlignmentOptions.MidlineLeft,
                24f);
        }

        if (savePlaytimeText == null)
        {
            Transform playtimeTransform = saveUIPanel.transform.Find("SavePlaytimeText");
            if (playtimeTransform != null)
            {
                savePlaytimeText = playtimeTransform.GetComponent<TMP_Text>();
            }
        }

        if (savePlaytimeText == null)
        {
            savePlaytimeText = CreateSaveText(
                saveUIPanel.transform,
                "SavePlaytimeText",
                new Vector2(170f, 165f),
                new Vector2(180f, 46f),
                TextAlignmentOptions.MidlineRight,
                30f);
        }

        if (saveDateText == null)
        {
            Transform dateTransform = saveUIPanel.transform.Find("SaveDateText");
            if (dateTransform != null)
            {
                saveDateText = dateTransform.GetComponent<TMP_Text>();
            }
        }

        if (saveDateText == null)
        {
            saveDateText = CreateSaveText(
                saveUIPanel.transform,
                "SaveDateText",
                new Vector2(170f, 120f),
                new Vector2(230f, 40f),
                TextAlignmentOptions.MidlineRight,
                20f);
        }

        if (savePreviewImage == null)
        {
            Transform imageTransform = saveUIPanel.transform.Find("SaveImage");
            if (imageTransform != null)
            {
                savePreviewImage = imageTransform.GetComponent<Image>();
            }
        }

        if (savePreviewImage == null)
        {
            GameObject imageObject = new GameObject("SaveImage", typeof(RectTransform), typeof(Image));
            imageObject.transform.SetParent(saveUIPanel.transform, false);

            RectTransform imageRect = imageObject.GetComponent<RectTransform>();
            imageRect.anchorMin = new Vector2(0.5f, 0.5f);
            imageRect.anchorMax = new Vector2(0.5f, 0.5f);
            imageRect.pivot = new Vector2(0.5f, 0.5f);
            imageRect.anchoredPosition = new Vector2(0f, 160f);
            imageRect.sizeDelta = new Vector2(54f, 68f);

            savePreviewImage = imageObject.GetComponent<Image>();
            savePreviewImage.preserveAspect = true;
            savePreviewImage.raycastTarget = false;
        }

        if (saveResultText == null)
        {
            saveResultText = CreateSaveText(
                saveUIPanel.transform,
                "SaveResultText",
                new Vector2(0f, -190f),
                new Vector2(520f, 40f),
                TextAlignmentOptions.Center,
                20f);
        }

        ConfigureSaveTextRect(saveResultText, new Vector2(0f, -190f), new Vector2(520f, 40f), TextAlignmentOptions.Center, 20f);

        ConfigureSaveTextRect(saveSlotNameText, new Vector2(-215f, 165f), new Vector2(200f, 44f), TextAlignmentOptions.MidlineLeft, 24f);
        ConfigureSaveTextRect(savePlaytimeText, new Vector2(170f, 165f), new Vector2(180f, 46f), TextAlignmentOptions.MidlineRight, 30f);
        ConfigureSaveTextRect(saveDateText, new Vector2(170f, 120f), new Vector2(230f, 40f), TextAlignmentOptions.MidlineRight, 20f);

        if (saveSlotNameText != null)
        {
            saveSlotNameText.fontSize = 26f;
        }
    }

    private Sprite ResolveCurrentSaveIcon()
    {
        Sprite sprite = resolvedPlayerIcon;
        if (sprite != null)
        {
            return sprite;
        }

        ResolvePlayerIcon();
        if (resolvedPlayerIcon != null)
        {
            return resolvedPlayerIcon;
        }

        if (!string.IsNullOrWhiteSpace(playerSpriteObjectName))
        {
            GameObject source = FindInLoadedScenesByName(playerSpriteObjectName);
            if (TryResolveSpriteFromObject(source, out Sprite fromNamedObject))
            {
                return fromNamedObject;
            }
        }

        if (playerSpriteRenderer != null)
        {
            return playerSpriteRenderer.sprite;
        }

        return null;
    }

    private TMP_Text CreateSaveText(Transform parent, string objectName, Vector2 anchoredPosition, Vector2 sizeDelta, TextAlignmentOptions alignment, float fontSize)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        TMP_Text text = textObject.GetComponent<TMP_Text>();
        text.raycastTarget = false;
        text.color = Color.white;

        if (saveStatusText != null && saveStatusText.font != null)
        {
            text.font = saveStatusText.font;
        }

        ConfigureSaveTextRect(text, anchoredPosition, sizeDelta, alignment, fontSize);
        return text;
    }

    private void ConfigureSaveTextRect(TMP_Text text, Vector2 anchoredPosition, Vector2 sizeDelta, TextAlignmentOptions alignment, float fontSize)
    {
        if (text == null)
        {
            return;
        }

        RectTransform rect = text.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;

        text.alignment = alignment;
        text.fontSize = fontSize;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.enableAutoSizing = true;
        text.fontSizeMax = fontSize;
        text.fontSizeMin = Mathf.Max(14f, fontSize - 8f);
        text.overflowMode = TextOverflowModes.Ellipsis;
    }

    private void UpdateSaveResultText(bool saved)
    {
        if (saveResultText == null)
        {
            return;
        }

        saveResultText.text = saved ? "Luu game thanh cong" : "Luu game that bai";
        saveResultText.color = saved ? new Color(0.75f, 1f, 0.75f, 1f) : new Color(1f, 0.7f, 0.7f, 1f);
    }
}
