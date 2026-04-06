using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SaveGameService
{
    public struct SavePreview
    {
        public bool hasData;
        public string playtimeText;
        public string savedAtText;
    }

    [Serializable]
    private class SaveData
    {
        public string sceneName;
        public string spawnPointId;
        public float spawnX;
        public float spawnY;
        public float sessionPlaytimeSeconds;
        public string savedAtUtcIso;
        public List<SavedInventoryItem> inventoryItems = new List<SavedInventoryItem>();
        public List<string> collectedItemIds = new List<string>();
    }

    [Serializable]
    private class SavedInventoryItem
    {
        public string displayName;
        public int amount;
        public bool stackable;
        public int lastAcquiredOrder;
        public string iconSpriteName;
    }

    private static SaveData pendingLoadData;
    private static bool isAwaitingSceneLoad;

    private static string SaveFilePath => Path.Combine(Application.persistentDataPath, "savegame.json");

    public static bool HasSaveFile()
    {
        return File.Exists(SaveFilePath);
    }

    public static bool SaveCurrentGame()
    {
        PlayerController player = PlayerPersist.GetPlayerController();
        if (player == null)
        {
            player = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
        }

        if (player == null)
        {
            Debug.LogWarning("Save failed: player not found.");
            return false;
        }

        SaveData data = new SaveData
        {
            sceneName = SceneManager.GetActiveScene().name,
            spawnPointId = string.Empty,
            spawnX = player.GetCurrentPosition2D().x,
            spawnY = player.GetCurrentPosition2D().y,
            sessionPlaytimeSeconds = SessionPlaytime.GetSeconds(),
            savedAtUtcIso = DateTime.UtcNow.ToString("o"),
            inventoryItems = SerializeInventory(player.GetInventorySnapshot()),
            collectedItemIds = GameWorldState.GetCollectedItemIdsSnapshot()
        };

        return WriteToFile(data);
    }

    public static SavePreview GetSavePreview()
    {
        SaveData data = ReadFromFile();
        if (data == null)
        {
            return new SavePreview
            {
                hasData = false,
                playtimeText = "00:00:00",
                savedAtText = "----/--/-- --:--"
            };
        }

        return new SavePreview
        {
            hasData = true,
            playtimeText = FormatPlaytime(data.sessionPlaytimeSeconds),
            savedAtText = FormatSavedAt(data.savedAtUtcIso)
        };
    }

    public static void ClearPendingLoadState()
    {
        pendingLoadData = null;
        isAwaitingSceneLoad = false;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public static bool TryPrepareContinue(out string sceneNameToLoad)
    {
        sceneNameToLoad = string.Empty;
        SaveData data = ReadFromFile();
        if (data == null || string.IsNullOrWhiteSpace(data.sceneName))
        {
            return false;
        }

        pendingLoadData = data;
        isAwaitingSceneLoad = true;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;

        PlayerPrefs.SetString("SpawnPointId", data.spawnPointId ?? string.Empty);
        PlayerPrefs.SetFloat("SpawnX", data.spawnX);
        PlayerPrefs.SetFloat("SpawnY", data.spawnY);
        PlayerPrefs.SetInt("HasPendingSpawn", 1);
        PlayerPrefs.Save();

        sceneNameToLoad = data.sceneName;
        return true;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!isAwaitingSceneLoad || pendingLoadData == null)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            return;
        }

        DialogueManager.ResetGlobalDialogueState();
        RebindDialogueManagersInScene();

        PlayerController player = PlayerPersist.GetPlayerController();
        if (player == null)
        {
            player = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
        }

        if (player != null)
        {
            List<InventoryItemEntry> restoredInventory = DeserializeInventory(pendingLoadData.inventoryItems);
            player.RestoreInventorySnapshot(restoredInventory);
            player.ResumeMovement();
        }

        SessionPlaytime.RestoreFromSave(pendingLoadData.sessionPlaytimeSeconds);
        GameWorldState.RestoreCollectedItemsFromSave(pendingLoadData.collectedItemIds);
        GameWorldState.ApplyLoadedStateToCurrentScene();

        pendingLoadData = null;
        isAwaitingSceneLoad = false;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private static void RebindDialogueManagersInScene()
    {
        DialogueManager dialogueManager = DialogueManager.Resolve();
        if (dialogueManager == null)
        {
            return;
        }

        DoorItemLock[] doorLocks = UnityEngine.Object.FindObjectsByType<DoorItemLock>(FindObjectsSortMode.None);
        for (int i = 0; i < doorLocks.Length; i++)
        {
            if (doorLocks[i] != null)
            {
                doorLocks[i].dialogueManager = dialogueManager;
            }
        }

        DoorInteraction[] doorInteractions = UnityEngine.Object.FindObjectsByType<DoorInteraction>(FindObjectsSortMode.None);
        for (int i = 0; i < doorInteractions.Length; i++)
        {
            if (doorInteractions[i] != null)
            {
                doorInteractions[i].dialogueManager = dialogueManager;
            }
        }

        DoorLockedDialogue[] lockedDialogues = UnityEngine.Object.FindObjectsByType<DoorLockedDialogue>(FindObjectsSortMode.None);
        for (int i = 0; i < lockedDialogues.Length; i++)
        {
            if (lockedDialogues[i] != null)
            {
                lockedDialogues[i].dialogueManager = dialogueManager;
            }
        }

        KeyPickup[] keyPickups = UnityEngine.Object.FindObjectsByType<KeyPickup>(FindObjectsSortMode.None);
        for (int i = 0; i < keyPickups.Length; i++)
        {
            if (keyPickups[i] != null)
            {
                keyPickups[i].dialogueManager = dialogueManager;
            }
        }

        SignDialogue[] signs = UnityEngine.Object.FindObjectsByType<SignDialogue>(FindObjectsSortMode.None);
        for (int i = 0; i < signs.Length; i++)
        {
            if (signs[i] != null)
            {
                signs[i].dialogueManager = dialogueManager;
            }
        }
    }

    private static bool WriteToFile(SaveData data)
    {
        try
        {
            string directory = Path.GetDirectoryName(SaveFilePath);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SaveFilePath, json);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError("Save failed: " + ex.Message);
            return false;
        }
    }

    private static SaveData ReadFromFile()
    {
        if (!File.Exists(SaveFilePath))
        {
            return null;
        }

        try
        {
            string json = File.ReadAllText(SaveFilePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonUtility.FromJson<SaveData>(json);
        }
        catch (Exception ex)
        {
            Debug.LogError("Load failed: " + ex.Message);
            return null;
        }
    }

    private static List<SavedInventoryItem> SerializeInventory(List<InventoryItemEntry> inventory)
    {
        List<SavedInventoryItem> result = new List<SavedInventoryItem>();
        if (inventory == null)
        {
            return result;
        }

        foreach (InventoryItemEntry entry in inventory)
        {
            if (entry == null)
            {
                continue;
            }

            result.Add(new SavedInventoryItem
            {
                displayName = entry.displayName,
                amount = entry.amount,
                stackable = entry.stackable,
                lastAcquiredOrder = entry.lastAcquiredOrder,
                iconSpriteName = entry.icon != null ? entry.icon.name : string.Empty
            });
        }

        return result;
    }

    private static List<InventoryItemEntry> DeserializeInventory(List<SavedInventoryItem> inventory)
    {
        List<InventoryItemEntry> result = new List<InventoryItemEntry>();
        if (inventory == null)
        {
            return result;
        }

        foreach (SavedInventoryItem entry in inventory)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.displayName) || entry.amount <= 0)
            {
                continue;
            }

            Sprite icon = ResolveSprite(entry.iconSpriteName);
            result.Add(new InventoryItemEntry(
                entry.displayName,
                entry.amount,
                entry.stackable,
                icon,
                entry.lastAcquiredOrder));
        }

        return result;
    }

    private static Sprite ResolveSprite(string spriteName)
    {
        if (string.IsNullOrWhiteSpace(spriteName))
        {
            return null;
        }

        Sprite[] loadedSprites = Resources.FindObjectsOfTypeAll<Sprite>();
        for (int i = 0; i < loadedSprites.Length; i++)
        {
            Sprite sprite = loadedSprites[i];
            if (sprite != null && sprite.name == spriteName)
            {
                return sprite;
            }
        }

        return null;
    }

    private static string FormatPlaytime(float elapsedSeconds)
    {
        int totalSeconds = Mathf.FloorToInt(Mathf.Max(0f, elapsedSeconds));
        int hours = totalSeconds / 3600;
        int minutes = (totalSeconds % 3600) / 60;

        return hours.ToString("00") + ":" + minutes.ToString("00");
    }

    private static string FormatSavedAt(string utcIso)
    {
        if (string.IsNullOrWhiteSpace(utcIso))
        {
            return "----/--/-- --:--";
        }

        if (!DateTime.TryParse(utcIso, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime utcTime))
        {
            return "----/--/-- --:--";
        }

        DateTime localTime = utcTime.ToLocalTime();
        return localTime.ToString("yyyy/MM/dd HH:mm");
    }
}
