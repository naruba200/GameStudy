using UnityEngine;
using UnityEngine.SceneManagement;

public class CanvasFollowPlayerPersist : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool parentToPlayerOnSceneLoad = true;

    [Header("World Space Follow")]
    [SerializeField] private bool followPlayerPosition;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2f, 0f);

    private static CanvasFollowPlayerPersist instance;
    private GameObject persistentRoot;
    private Transform playerTransform;

    public static void ResetPersistentInstance()
    {
        if (instance == null)
        {
            return;
        }

        GameObject root = instance.persistentRoot != null ? instance.persistentRoot : instance.gameObject;
        Destroy(root);
        instance = null;
    }

    private void Awake()
    {
        persistentRoot = transform.root.gameObject;

        if (instance != null && instance != this)
        {
            Destroy(persistentRoot);
            return;
        }

        instance = this;
        DontDestroyOnLoad(persistentRoot);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        RebindPlayer();
        AttachToPlayerIfNeeded();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RebindPlayer();
        AttachToPlayerIfNeeded();
    }

    private void LateUpdate()
    {
        if (!followPlayerPosition || playerTransform == null || parentToPlayerOnSceneLoad)
        {
            return;
        }

        transform.position = playerTransform.position + worldOffset;
    }

    private void RebindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
        playerTransform = playerObject != null ? playerObject.transform : null;
    }

    private void AttachToPlayerIfNeeded()
    {
        if (!parentToPlayerOnSceneLoad || playerTransform == null)
        {
            return;
        }

        transform.SetParent(playerTransform, true);
    }
}
