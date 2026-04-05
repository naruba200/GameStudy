using UnityEngine;

public class PlayerPersist : MonoBehaviour
{
    private static PlayerPersist instance;

    public static void ResetPersistentInstance()
    {
        if (instance == null)
        {
            return;
        }

        Destroy(instance.gameObject);
        instance = null;
    }

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject); // tránh bị nhân đôi player
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}