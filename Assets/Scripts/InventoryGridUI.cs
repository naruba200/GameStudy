using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InventoryGridUI : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private GridLayoutGroup gridLayoutGroup;
    [SerializeField] private InventorySlotUI slotPrefab;
    [SerializeField] private int columnCount = 2;
    [SerializeField] private TMP_Text emptyStateText;

    private readonly List<InventorySlotUI> spawnedSlots = new List<InventorySlotUI>();

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        RebindPlayer();
        ApplyGridSettings();
        Refresh();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RebindPlayer();
        ApplyGridSettings();
        Refresh();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (player != null)
        {
            player.OnInventoryChanged -= Refresh;
        }
    }

    private void RebindPlayer()
    {
        if (player != null)
        {
            player.OnInventoryChanged -= Refresh;
        }

        PlayerController persistentPlayer = PlayerPersist.GetPlayerController();
        if (persistentPlayer != null)
        {
            player = persistentPlayer;
        }
        else if (player == null)
        {
            player = Object.FindFirstObjectByType<PlayerController>();
        }

        if (player != null)
        {
            player.OnInventoryChanged += Refresh;
        }
    }

    private void ApplyGridSettings()
    {
        if (gridLayoutGroup == null)
        {
            return;
        }

        gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayoutGroup.constraintCount = Mathf.Max(1, columnCount);
    }

    public void Refresh()
    {
        if (player == null || slotPrefab == null || gridLayoutGroup == null)
        {
            return;
        }

        List<InventoryItemEntry> snapshot = player.GetInventorySnapshot();

        for (int i = spawnedSlots.Count - 1; i >= 0; i--)
        {
            if (spawnedSlots[i] != null)
            {
                Destroy(spawnedSlots[i].gameObject);
            }
        }

        spawnedSlots.Clear();

        if (emptyStateText != null)
        {
            emptyStateText.gameObject.SetActive(snapshot.Count == 0);
            if (snapshot.Count == 0)
            {
                emptyStateText.text = "Inventory trống";
            }
        }

        foreach (InventoryItemEntry entry in snapshot)
        {
            InventorySlotUI slot = Instantiate(slotPrefab, gridLayoutGroup.transform);
            slot.SetData(entry);
            spawnedSlots.Add(slot);
        }
    }
}