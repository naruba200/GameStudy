using UnityEngine;

[System.Serializable]
public class InventoryItemEntry
{
    public string displayName;
    public int amount;
    public bool stackable;
    public Sprite icon;
    public int lastAcquiredOrder;

    public InventoryItemEntry(string displayName, int amount, bool stackable, Sprite icon, int lastAcquiredOrder)
    {
        this.displayName = displayName;
        this.amount = amount;
        this.stackable = stackable;
        this.icon = icon;
        this.lastAcquiredOrder = lastAcquiredOrder;
    }

    public InventoryItemEntry Clone()
    {
        return new InventoryItemEntry(displayName, amount, stackable, icon, lastAcquiredOrder);
    }
}