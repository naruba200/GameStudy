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

    public static void ResetPersistentInstance()
    {
        if (instance == null)
        {
            return;
        }

        Destroy(instance.gameObject);
        instance = null;
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
            CloseInventoryInternal(false, false);
            HideAllShowMainInventoryObjects();

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
}
