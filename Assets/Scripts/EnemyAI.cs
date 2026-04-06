using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    private static EnemyAI persistentInstance;

    [Header("Movement Settings")]
    public float moveSpeed = 2f;

    [Header("Detection Settings")]
    public float detectionRange = 5f;

    [Header("Wander Settings")]
    public float wanderRadius = 3f;
    public float wanderTimer = 2f;

    [Header("Collision Settings")]
    public LayerMask obstacleLayer;   // 👈 chọn layer trong Inspector
    public float checkDistance = 0.5f;
    public float radius = 0.2f;

    [Header("Cross-Scene Follow")]
    public bool persistAcrossScenes = true;
    public bool followPlayerAcrossScenes = true;
    public float sceneFollowDelay = 1f;

    private Transform player;
    private Vector2 wanderTarget;
    private float timer;
    private bool isChasingPlayer;
    private Coroutine followSceneRoutine;

    private void Awake()
    {
        if (!persistAcrossScenes)
        {
            return;
        }

        if (persistentInstance != null && persistentInstance != this)
        {
            Destroy(gameObject);
            return;
        }

        persistentInstance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    private void OnDestroy()
    {
        if (persistentInstance == this)
        {
            persistentInstance = null;
        }
    }

    private void Start()
    {
        TryResolvePlayer();

        timer = wanderTimer;
        SetNewWanderTarget();
    }

    private void Update()
    {
        if (player == null)
        {
            TryResolvePlayer();
            if (player == null)
            {
                return;
            }
        }

        float distance = Vector2.Distance(transform.position, player.position);
        isChasingPlayer = distance <= detectionRange;

        if (isChasingPlayer)
        {
            ChasePlayer();
        }
        else
        {
            Wander();
        }
    }

    void ChasePlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;

        if (!IsBlocked(direction))
        {
            Move(direction);
        }
    }

    void Wander()
    {
        timer += Time.deltaTime;

        Vector2 direction = (wanderTarget - (Vector2)transform.position).normalized;

        if (!IsBlocked(direction))
        {
            Move(direction);
        }
        else
        {
            // Nếu bị chặn → chọn hướng mới
            SetNewWanderTarget();
        }

        if (Vector2.Distance(transform.position, wanderTarget) < 0.2f || timer >= wanderTimer)
        {
            SetNewWanderTarget();
            timer = 0;
        }
    }

    void Move(Vector2 direction)
    {
        transform.position += (Vector3)(direction * moveSpeed * Time.deltaTime);
    }

    bool IsBlocked(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.CircleCast(
            transform.position,
            radius,
            direction,
            checkDistance,
            obstacleLayer
        );

        return hit.collider != null;
    }

    void SetNewWanderTarget()
    {
        Vector2 randomDirection = Random.insideUnitCircle * wanderRadius;
        wanderTarget = (Vector2)transform.position + randomDirection;
    }

    void TryResolvePlayer()
    {
        PlayerController persistentPlayer = PlayerPersist.GetPlayerController();
        if (persistentPlayer != null)
        {
            player = persistentPlayer.transform;
            return;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        if (!followPlayerAcrossScenes || !isChasingPlayer)
        {
            return;
        }

        if (followSceneRoutine != null)
        {
            StopCoroutine(followSceneRoutine);
        }

        followSceneRoutine = StartCoroutine(FollowPlayerToSceneRoutine());
    }

    IEnumerator FollowPlayerToSceneRoutine()
    {
        if (sceneFollowDelay > 0f)
        {
            yield return new WaitForSeconds(sceneFollowDelay);
        }

        TryResolvePlayer();
        if (player == null)
        {
            followSceneRoutine = null;
            yield break;
        }

        if (gameObject.scene != player.gameObject.scene)
        {
            SceneManager.MoveGameObjectToScene(gameObject, player.gameObject.scene);
        }

        Vector2 spawnPosition;
        if (TryGetPlayerSpawnPosition(out spawnPosition))
        {
            transform.position = spawnPosition;
        }
        else
        {
            transform.position = player.position;
        }

        SetNewWanderTarget();
        followSceneRoutine = null;
    }

    bool TryGetPlayerSpawnPosition(out Vector2 spawnPosition)
    {
        string spawnPointId = PlayerPrefs.GetString("SpawnPointId", string.Empty);
        if (!string.IsNullOrEmpty(spawnPointId))
        {
            SpawnPoint[] points = Object.FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
            foreach (SpawnPoint point in points)
            {
                if (point != null && point.spawnId == spawnPointId)
                {
                    spawnPosition = point.transform.position;
                    return true;
                }
            }
        }

        if (PlayerPrefs.GetInt("HasPendingSpawn", 0) == 1)
        {
            float x = PlayerPrefs.GetFloat("SpawnX", transform.position.x);
            float y = PlayerPrefs.GetFloat("SpawnY", transform.position.y);
            spawnPosition = new Vector2(x, y);
            return true;
        }

        spawnPosition = default;
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Wander range
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);

        // Collision check
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}