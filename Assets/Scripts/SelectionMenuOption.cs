using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class SelectionMenuOption : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
{
    public enum SelectionAction
    {
        OpenInventory,
        OpenSave,
        BackToMenu
    }

    [SerializeField] private SelectionAction action;
    [SerializeField] private GameObject selectionRoot;
    [SerializeField] private GameObject generalPanel;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject savePanel;
    [SerializeField] private string menuSceneName = "Outside";
    [SerializeField] private string fallbackStartSceneName = "Outside";
    [SerializeField] private float framePadding = 2f;
    [SerializeField] private float frameThickness = 2f;

    private readonly List<Image> borderImages = new List<Image>(4);

    private void Awake()
    {
        EnsureHighlightFrame();
        SetHighlighted(false, 0f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SelectionMenuController controller = GetComponentInParent<SelectionMenuController>();
        if (controller != null)
        {
            controller.ConfirmOption(this);
            return;
        }

        Execute();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SelectionMenuController controller = GetComponentInParent<SelectionMenuController>();
        if (controller != null)
        {
            controller.FocusOption(this);
        }
    }

    public void Execute()
    {
        InventoryToggleUI inventoryToggle = Object.FindFirstObjectByType<InventoryToggleUI>();

        switch (action)
        {
            case SelectionAction.OpenInventory:
                if (inventoryToggle != null)
                {
                    inventoryToggle.ShowInventoryUI();
                    return;
                }

                ShowPanel(inventoryPanel, savePanel);
                break;
            case SelectionAction.OpenSave:
                if (inventoryToggle != null)
                {
                    inventoryToggle.ShowSaveUI();
                    return;
                }

                ShowPanel(savePanel, inventoryPanel);
                SaveGameService.SaveCurrentGame();
                break;
            case SelectionAction.BackToMenu:
                Time.timeScale = 1f;
                if (!string.IsNullOrWhiteSpace(menuSceneName) && Application.CanStreamedLevelBeLoaded(menuSceneName))
                {
                    SceneManager.LoadScene(menuSceneName);
                }
                else
                {
                    TryOpenStartScreen();
                }
                break;
        }
    }

    public void SetHighlighted(bool highlighted, float alpha)
    {
        if (borderImages.Count == 0)
        {
            return;
        }

        float targetAlpha = highlighted ? alpha : 0f;

        for (int i = 0; i < borderImages.Count; i++)
        {
            if (borderImages[i] == null)
            {
                continue;
            }

            Color color = borderImages[i].color;
            color.a = targetAlpha;
            borderImages[i].color = color;
        }
    }

    private void ShowPanel(GameObject panelToShow, GameObject panelToHide)
    {
        if (generalPanel == null)
        {
            generalPanel = GameObject.Find("GeneralUI");
        }

        if (generalPanel != null)
        {
            generalPanel.SetActive(false);
        }

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(panelToShow == inventoryPanel);
        }

        if (savePanel != null)
        {
            savePanel.SetActive(panelToShow == savePanel);
        }

        if (panelToHide != null)
        {
            panelToHide.SetActive(false);
        }

        if (panelToShow != null)
        {
            panelToShow.SetActive(true);
        }
    }

    private bool TryOpenStartScreen()
    {
        Screen.PrepareForStartMenuReturn();

        InventoryToggleUI inventoryToggle = Object.FindFirstObjectByType<InventoryToggleUI>();
        if (inventoryToggle != null)
        {
            inventoryToggle.SetInventoryAccess(false);
        }

        GameObject inventoryRoot = GameObject.Find("Inventory");
        if (inventoryRoot != null)
        {
            inventoryRoot.SetActive(false);
        }

        GameObject showMainInventory = GameObject.Find("ShowMainInventory");
        if (showMainInventory != null)
        {
            showMainInventory.SetActive(false);
        }

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }

        if (savePanel != null)
        {
            savePanel.SetActive(false);
        }

        if (selectionRoot != null)
        {
            selectionRoot.SetActive(true);
        }

        Screen screen = ResolveScreenInLoadedScenes();
        if (screen != null && screen.startScreen != null)
        {
            screen.startScreen.SetActive(true);
            Time.timeScale = screen.pauseGameWhenStartScreenVisible ? 0f : 1f;
            return true;
        }

        if (!string.IsNullOrWhiteSpace(fallbackStartSceneName) && Application.CanStreamedLevelBeLoaded(fallbackStartSceneName))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(fallbackStartSceneName);
            return true;
        }

        Debug.LogWarning("Unable to open start menu: no start screen found and fallback scene is not loadable.");
        return false;
    }

    private Screen ResolveScreenInLoadedScenes()
    {
        Screen direct = Object.FindFirstObjectByType<Screen>();
        if (direct != null)
        {
            return direct;
        }

        Screen[] all = Resources.FindObjectsOfTypeAll<Screen>();
        return all.FirstOrDefault(screen =>
            screen != null &&
            screen.gameObject.scene.IsValid() &&
            screen.gameObject.scene.isLoaded &&
            (screen.hideFlags & HideFlags.HideAndDontSave) == 0);
    }

    private void EnsureHighlightFrame()
    {
        Transform existing = transform.Find("SelectionHighlightFrame");
        GameObject frameObject;

        if (existing != null)
        {
            frameObject = existing.gameObject;
        }
        else
        {
            frameObject = new GameObject("SelectionHighlightFrame", typeof(RectTransform));
            frameObject.transform.SetParent(transform, false);
            frameObject.transform.SetAsFirstSibling();
        }

        RectTransform frameRect = frameObject.GetComponent<RectTransform>();
        frameRect.anchorMin = Vector2.zero;
        frameRect.anchorMax = Vector2.one;
        frameRect.offsetMin = new Vector2(-framePadding, -framePadding);
        frameRect.offsetMax = new Vector2(framePadding, framePadding);
        frameRect.pivot = new Vector2(0.5f, 0.5f);

        Image legacyImage = frameObject.GetComponent<Image>();
        if (legacyImage != null)
        {
            legacyImage.color = new Color(1f, 1f, 1f, 0f);
            legacyImage.raycastTarget = false;
        }

        Outline legacyOutline = frameObject.GetComponent<Outline>();
        if (legacyOutline != null)
        {
            Destroy(legacyOutline);
        }

        borderImages.Clear();
        CreateOrConfigureBorder(frameRect, "Top", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(0f, frameThickness));
        CreateOrConfigureBorder(frameRect, "Bottom", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, frameThickness));
        CreateOrConfigureBorder(frameRect, "Left", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(frameThickness, 0f));
        CreateOrConfigureBorder(frameRect, "Right", new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0f), new Vector2(frameThickness, 0f));
    }

    private void CreateOrConfigureBorder(RectTransform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta)
    {
        Transform existing = parent.Find(name);
        GameObject borderObject;

        if (existing != null)
        {
            borderObject = existing.gameObject;
        }
        else
        {
            borderObject = new GameObject(name, typeof(RectTransform));
            borderObject.transform.SetParent(parent, false);
        }

        RectTransform borderRect = borderObject.GetComponent<RectTransform>();
        borderRect.anchorMin = anchorMin;
        borderRect.anchorMax = anchorMax;
        borderRect.pivot = pivot;
        borderRect.anchoredPosition = Vector2.zero;
        borderRect.sizeDelta = sizeDelta;

        Image borderImage = borderObject.GetComponent<Image>();
        if (borderImage == null)
        {
            borderImage = borderObject.AddComponent<Image>();
        }

        borderImage.raycastTarget = false;
        borderImage.color = new Color(1f, 1f, 1f, 0f);
        borderImages.Add(borderImage);
    }
}
