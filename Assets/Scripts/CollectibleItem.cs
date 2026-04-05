using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CollectibleItem : MonoBehaviour
{
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

        if (string.IsNullOrWhiteSpace(itemName))
        {
            itemName = gameObject.name;
        }

        if (icon == null && cachedSpriteRenderer != null)
        {
            icon = cachedSpriteRenderer.sprite;
        }
    }

    public void Collect(PlayerController player)
    {
        if (player == null)
        {
            return;
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

        DialogueManager dialogueManager = Object.FindFirstObjectByType<DialogueManager>();
        if (dialogueManager != null)
        {
            dialogueManager.ShowAcquireMessage(itemName);
        }

        Destroy(gameObject);
    }
}