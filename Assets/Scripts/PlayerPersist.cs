using UnityEngine;

public class PlayerPersist : MonoBehaviour
{
    private static PlayerPersist instance;

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