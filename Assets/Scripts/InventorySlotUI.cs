using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text amountText;

    public void SetData(InventoryItemEntry entry)
    {
        if (entry == null)
        {
            Clear();
            return;
        }

        if (iconImage != null)
        {
            iconImage.sprite = entry.icon;
            iconImage.enabled = entry.icon != null;
        }

        if (nameText != null)
        {
            nameText.text = entry.displayName;
        }

        if (amountText != null)
        {
            amountText.text = "x" + Mathf.Max(1, entry.amount);
            amountText.gameObject.SetActive(true);
        }
    }

    public void Clear()
    {
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }

        if (nameText != null)
        {
            nameText.text = string.Empty;
        }

        if (amountText != null)
        {
            amountText.text = string.Empty;
            amountText.gameObject.SetActive(false);
        }
    }
}