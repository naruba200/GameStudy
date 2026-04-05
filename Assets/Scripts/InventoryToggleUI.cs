using UnityEngine;
using UnityEngine.SceneManagement;

public class InventoryToggleUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject showMainInventory;
    [SerializeField] private GameObject generalUIPanel;
    [SerializeField] private GameObject inventoryUIPanel;
    [SerializeField] private GameObject saveUIPanel;

    [Header("Player")]
    [SerializeField] private PlayerController player;

    [Header("Hotkeys")]
    [SerializeField] private KeyCode openInventoryKey = KeyCode.B;
    [SerializeField] private KeyCode closeInventoryKey = KeyCode.Escape;

    [Header("Startup")]
    [SerializeField] private bool startWithInventoryClosed = true;

    private static InventoryToggleUI instance;
    private Screen screen;
    private bool inventoryAccessEnabled = true;
    private bool lastStartScreenVisible;

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
        DontDestroyOnLoad(gameObject);
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

        if (startWithInventoryClosed)
        {
            CloseInventory();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryResolveReferences();
        SyncWithStartScreen();
    }

    private void Update()
    {
        SyncWithStartScreen();

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

        if (!inventoryAccessEnabled)
        {
            return;
        }

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(true);
        }

        ApplyDefaultBackpackView();

        if (showMainInventory != null)
        {
            showMainInventory.SetActive(false);
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
            if (showMainInventory != null)
            {
                showMainInventory.SetActive(false);
            }

            if (player != null)
            {
                player.StopMovement();
            }

            return;
        }

        if (showMainInventory != null)
        {
            showMainInventory.SetActive(true);
        }

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

    private void TryResolveReferences()
    {
        if (inventoryPanel == null)
        {
            inventoryPanel = GameObject.Find("Inventory");
        }

        if (showMainInventory == null)
        {
            showMainInventory = GameObject.Find("ShowMainInventory");
        }

        if (generalUIPanel == null)
        {
            generalUIPanel = GameObject.Find("GeneralUI");
        }

        if (inventoryUIPanel == null)
        {
            inventoryUIPanel = GameObject.Find("InventoryUI");
        }

        if (saveUIPanel == null)
        {
            saveUIPanel = GameObject.Find("SaveUI");
        }

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.GetComponent<PlayerController>();
            }
        }

        if (screen == null)
        {
            screen = Object.FindFirstObjectByType<Screen>();
        }
    }

    private void ApplyDefaultBackpackView()
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

    private void CloseInventoryInternal(bool showBackpackButton, bool resumePlayerMovement)
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }

        if (showMainInventory != null)
        {
            showMainInventory.SetActive(showBackpackButton);
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
    }
}
