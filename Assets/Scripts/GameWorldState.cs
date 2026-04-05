using System.Collections.Generic;
using UnityEngine;

public static class GameWorldState
{
    private static readonly HashSet<string> collectedItemIds = new HashSet<string>();

    public static void Reset()
    {
        collectedItemIds.Clear();
    }

    public static void MarkCollected(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return;
        }

        collectedItemIds.Add(itemId);
    }

    public static bool IsCollected(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return false;
        }

        return collectedItemIds.Contains(itemId);
    }

    public static List<string> GetCollectedItemIdsSnapshot()
    {
        return new List<string>(collectedItemIds);
    }

    public static void RestoreCollectedItemsFromSave(List<string> itemIds)
    {
        collectedItemIds.Clear();
        if (itemIds == null)
        {
            return;
        }

        for (int i = 0; i < itemIds.Count; i++)
        {
            string id = itemIds[i];
            if (!string.IsNullOrWhiteSpace(id))
            {
                collectedItemIds.Add(id);
            }
        }
    }

    public static void ApplyLoadedStateToCurrentScene()
    {
        CollectibleItem[] collectibles = Object.FindObjectsByType<CollectibleItem>(FindObjectsSortMode.None);
        for (int i = 0; i < collectibles.Length; i++)
        {
            CollectibleItem collectible = collectibles[i];
            if (collectible != null)
            {
                collectible.ApplyCollectedStateIfNeeded();
            }
        }
    }
}