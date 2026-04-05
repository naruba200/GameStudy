using System.Collections.Generic;
using UnityEngine;

public class SelectionMenuController : MonoBehaviour
{
    [SerializeField] private float blinkInterval = 1f;
    [SerializeField] private float dimAlpha = 0.2f;

    private readonly List<SelectionMenuOption> options = new List<SelectionMenuOption>();
    private int selectedIndex;
    private float blinkTimer;
    private bool blinkVisible = true;
    private bool blinkEnabled = true;

    private void OnEnable()
    {
        RefreshOptions();
        SelectIndex(0, true);
    }

    private void Update()
    {
        if (options.Count == 0)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            MoveSelection(-1);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            MoveSelection(1);
        }
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            ConfirmSelectedOption();
        }

        UpdateBlink();
    }

    public void RefreshOptions()
    {
        options.Clear();
        GetComponentsInChildren(true, options);
        options.RemoveAll(option => option == null);
        options.Sort((left, right) => left.transform.GetSiblingIndex().CompareTo(right.transform.GetSiblingIndex()));
    }

    public void FocusOption(SelectionMenuOption option)
    {
        int index = options.IndexOf(option);
        if (index < 0)
        {
            RefreshOptions();
            index = options.IndexOf(option);
        }

        if (index < 0)
        {
            return;
        }

        SelectIndex(index, true);
    }

    public void ConfirmOption(SelectionMenuOption option)
    {
        int index = options.IndexOf(option);
        if (index >= 0)
        {
            SelectIndex(index, false);
        }

        ConfirmSelectedOption();
    }

    private void MoveSelection(int direction)
    {
        if (options.Count == 0)
        {
            return;
        }

        int nextIndex = selectedIndex + direction;
        if (nextIndex < 0)
        {
            nextIndex = options.Count - 1;
        }
        else if (nextIndex >= options.Count)
        {
            nextIndex = 0;
        }

        SelectIndex(nextIndex, true);
    }

    private void ConfirmSelectedOption()
    {
        if (options.Count == 0)
        {
            return;
        }

        blinkEnabled = false;
        blinkVisible = true;
        ApplyVisuals();
        options[selectedIndex].Execute();
    }

    private void SelectIndex(int index, bool allowBlink)
    {
        if (options.Count == 0)
        {
            return;
        }

        selectedIndex = Mathf.Clamp(index, 0, options.Count - 1);
        blinkEnabled = allowBlink;
        blinkVisible = true;
        blinkTimer = 0f;
        ApplyVisuals();
    }

    private void UpdateBlink()
    {
        if (!blinkEnabled || options.Count == 0)
        {
            return;
        }

        blinkTimer += Time.unscaledDeltaTime;
        if (blinkTimer < blinkInterval)
        {
            return;
        }

        blinkTimer = 0f;
        blinkVisible = !blinkVisible;
        ApplyVisuals();
    }

    private void ApplyVisuals()
    {
        for (int i = 0; i < options.Count; i++)
        {
            SelectionMenuOption option = options[i];
            bool isSelected = i == selectedIndex;
            float alpha = isSelected ? (blinkEnabled ? (blinkVisible ? 1f : dimAlpha) : 1f) : 0f;
            option.SetHighlighted(isSelected, alpha);
        }
    }
}
