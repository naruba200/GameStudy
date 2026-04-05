using UnityEngine;

public class PlayerPersist : MonoBehaviour
{
    private static PlayerPersist instance;

    [Header("Debug")]
    [SerializeField] private bool keepInSceneHierarchyDuringPlay;

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

        if (!(Application.isEditor && keepInSceneHierarchyDuringPlay))
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}