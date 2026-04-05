using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed;
    public float pickupRadius = 1.2f;
    public TMP_Text inventoryText;

    private bool isMoving;
    public bool canMove = true;

    private Vector2 input;
    private Animator animator;
    private readonly List<InventoryItemEntry> inventory = new List<InventoryItemEntry>();
    private int pickupSequence;

    public event Action OnInventoryChanged;

    public LayerMask solidObjectsLayer;
    public LayerMask interactableLayer;

    private void Awake() 
    {
        animator = GetComponent<Animator>();
    }

    public void StopMovement()
    {
        StopAllCoroutines();
        canMove = false;
        isMoving = false;
        input = Vector2.zero;

        animator.SetBool("isMoving", false);
    }

    public void ResumeMovement()
    {
        StopAllCoroutines();
        canMove = true;
        isMoving = false;
        input = Vector2.zero;

        animator.SetBool("isMoving", false);
    }

    private void Update()
    {
        if (!canMove)
        {
            isMoving = false;
            animator.SetBool("isMoving", false);
            return;
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            TryPickupItem();
        }
        if (!isMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            if (input != Vector2.zero)
            {
                animator.SetFloat("moveX", input.x);
                animator.SetFloat("moveY", input.y);

                var targetPos = transform.position;
                targetPos.x += input.x;
                targetPos.y += input.y;

                if (IsWalkable(targetPos))
                    StartCoroutine(Move(targetPos));
            }
        }
        animator.SetBool("isMoving", isMoving);
    }

    IEnumerator Move(Vector3 targetPos)
    {
        isMoving = true;

        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPos;

        isMoving = false;
    }

    private bool IsWalkable(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, 0.2f, solidObjectsLayer | interactableLayer) != null)
        {
            return false;
        }
        return true;
    }

    private void TryPickupItem()
    {
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, pickupRadius);

        foreach (Collider2D nearbyCollider in nearbyColliders)
        {
            CollectibleItem collectibleItem = nearbyCollider.GetComponent<CollectibleItem>();

            if (collectibleItem == null)
            {
                collectibleItem = nearbyCollider.GetComponentInParent<CollectibleItem>();
            }

            if (collectibleItem != null)
            {
                collectibleItem.Collect(this);
                return;
            }
        }
    }

    public void AddItem(string itemName, int amount, bool stackable, Sprite icon)
    {
        if (string.IsNullOrWhiteSpace(itemName) || amount <= 0)
        {
            return;
        }

        if (stackable)
        {
            InventoryItemEntry existingEntry = inventory.Find(entry => entry.displayName == itemName && entry.stackable);

            if (existingEntry != null)
            {
                existingEntry.amount += amount;
                existingEntry.lastAcquiredOrder = ++pickupSequence;
                if (existingEntry.icon == null)
                {
                    existingEntry.icon = icon;
                }
            }
            else
            {
                inventory.Add(new InventoryItemEntry(itemName, amount, true, icon, ++pickupSequence));
            }
        }
        else
        {
            for (int i = 0; i < amount; i++)
            {
                inventory.Add(new InventoryItemEntry(itemName, 1, false, icon, ++pickupSequence));
            }
        }

        UpdateInventoryText();
        OnInventoryChanged?.Invoke();
        Debug.Log("Picked up " + itemName + " x" + amount + (stackable ? " (stackable)" : " (non-stackable)"));
    }

    public List<InventoryItemEntry> GetInventorySnapshot()
    {
        List<InventoryItemEntry> snapshot = new List<InventoryItemEntry>(inventory.Count);

        foreach (InventoryItemEntry entry in inventory)
        {
            snapshot.Add(entry.Clone());
        }

        snapshot.Sort((left, right) => right.lastAcquiredOrder.CompareTo(left.lastAcquiredOrder));
        return snapshot;
    }

    public void RestoreInventorySnapshot(List<InventoryItemEntry> snapshot)
    {
        inventory.Clear();
        pickupSequence = 0;

        if (snapshot != null)
        {
            foreach (InventoryItemEntry entry in snapshot)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.displayName) || entry.amount <= 0)
                {
                    continue;
                }

                inventory.Add(entry.Clone());
                if (entry.lastAcquiredOrder > pickupSequence)
                {
                    pickupSequence = entry.lastAcquiredOrder;
                }
            }
        }

        UpdateInventoryText();
        OnInventoryChanged?.Invoke();
    }

    public Vector2 GetCurrentPosition2D()
    {
        return transform.position;
    }

    private void UpdateInventoryText()
    {
        if (inventoryText == null)
        {
            return;
        }

        if (inventory.Count == 0)
        {
            inventoryText.text = "Inventory: Empty";
            return;
        }

        StringBuilder builder = new StringBuilder();
        builder.AppendLine("Inventory:");

        List<InventoryItemEntry> sortedInventory = GetInventorySnapshot();

        foreach (InventoryItemEntry entry in sortedInventory)
        {
            if (entry.stackable && entry.amount > 1)
            {
                builder.AppendLine(entry.displayName + " x" + entry.amount);
            }
            else
            {
                builder.AppendLine(entry.displayName);
            }
        }

        inventoryText.text = builder.ToString();
    }
}
