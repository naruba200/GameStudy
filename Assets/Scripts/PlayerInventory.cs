using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance;

    public bool hasKey = false;

    private void Awake()
    {
        Instance = this;

        // ✅ Always start without key (for now)
        hasKey = false;
    }

    public void SetKey(bool value)
    {
        hasKey = value;
    }
}