using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CollectibleItem : MonoBehaviour
{
    [Header("Save")]
    [SerializeField] private string persistentId;

    [SerializeField] private string itemName = "Item";
    [SerializeField] private int amount = 1;
    [SerializeField] private bool stackable = true;
    [SerializeField] private Sprite icon;

    private SpriteRenderer cachedSpriteRenderer;

    private void Reset()
    {
        Collider2D itemCollider = GetComponent<Collider2D>();
        if (itemCollider != null)
        {
            itemCollider.isTrigger = true;
        }
    }

    private void Awake()
    {
        cachedSpriteRenderer = GetComponent<SpriteRenderer>();

        EnsurePersistentId();

        if (string.IsNullOrWhiteSpace(itemName))
        {
            itemName = gameObject.name;
        }

        if (icon == null && cachedSpriteRenderer != null)
        {
            icon = cachedSpriteRenderer.sprite;
        }

        ApplyCollectedStateIfNeeded();
    }

    public void Collect(PlayerController player)
    {
        if (player == null)
        {
            return;
        }

        PickupEnemyActivator[] enemyActivators = GetComponentsInChildren<PickupEnemyActivator>(true);
        if (enemyActivators.Length == 0)
        {
            enemyActivators = GetComponentsInParent<PickupEnemyActivator>(true);
        }

        for (int i = 0; i < enemyActivators.Length; i++)
        {
            enemyActivators[i].OnCollected(player);
        }

        if (icon == null)
        {
            SpriteRenderer spriteRenderer = cachedSpriteRenderer != null ? cachedSpriteRenderer : GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                icon = spriteRenderer.sprite;
            }
        }

        player.AddItem(itemName, amount, stackable, icon);
        GameWorldState.MarkCollected(GetSaveId());

        DialogueManager dialogueManager = Object.FindFirstObjectByType<DialogueManager>();
        if (dialogueManager != null)
        {
            dialogueManager.ShowAcquireMessage(itemName);
        }

        Destroy(gameObject);
    }

    public void ApplyCollectedStateIfNeeded()
    {
        if (GameWorldState.IsCollected(GetSaveId()))
        {
            Destroy(gameObject);
        }
    }

    private string GetSaveId()
    {
        string sceneName = gameObject.scene.IsValid() ? gameObject.scene.name : "UnknownScene";
        string localId = string.IsNullOrWhiteSpace(persistentId) ? BuildRuntimeFallbackId() : persistentId;
        return sceneName + "|" + localId;
    }

    private void EnsurePersistentId()
    {
        if (!string.IsNullOrWhiteSpace(persistentId))
        {
            return;
        }
    }

    private string BuildRuntimeFallbackId()
    {
        return gameObject.name + "@" + BuildHierarchyPath(transform);
    }

    private string BuildHierarchyPath(Transform target)
    {
        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        Transform current = target;

        while (current != null)
        {
            if (builder.Length > 0)
            {
                builder.Insert(0, "/");
            }

            builder.Insert(0, current.name);
            current = current.parent;
        }

        return builder.ToString();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(persistentId))
        {
            persistentId = System.Guid.NewGuid().ToString("N");
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif
}