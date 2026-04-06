using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    private static EnemyAI persistentInstance;
    private static readonly float[] avoidanceAngles = { 30f, -30f, 60f, -60f, 90f, -90f, 120f, -120f, 150f, -150f };

    public static void ResetPersistentInstance()
    {
        if (persistentInstance == null)
        {
            return;
        }

        Destroy(persistentInstance.gameObject);
        persistentInstance = null;
    }

    [Header("Movement Settings")]
    public float moveSpeed = 2f;

    [Header("Detection Settings")]
    public float detectionRange = 5f;
    public float playerContactRadius = 0.35f;

    [Header("Wander Settings")]
    public float wanderRadius = 3f;
    public float wanderTimer = 2f;

    [Header("Collision Settings")]
    public LayerMask obstacleLayer;   // 👈 chọn layer trong Inspector
    public float checkDistance = 0.5f;
    public float radius = 0.2f;
    public bool autoIncludeDefaultObstacleLayers = true;

    [Header("Cross-Scene Follow")]
    public bool persistAcrossScenes = true;
    public bool followPlayerAcrossScenes = true;
    public float sceneFollowDelay = 2f;
    public float minSpawnDistanceFromPlayer = 2f;
    public float maxSpawnDistanceFromPlayer = 4f;

    [Header("Hide Reaction")]
    public float hideWanderMinDuration = 1f;
    public float hideWanderMaxDuration = 2f;
    public float doorReachDistance = 0.2f;
    public float doorReachTimeout = 4f;

    private Transform player;
    private Vector2 wanderTarget;
    private float timer;
    private bool isChasingPlayer;
    private Coroutine followSceneRoutine;
    private Coroutine hideReactionRoutine;
    private bool isHandlingPlayerHide;
    private bool hasTriggeredGameOver;
    private Renderer[] cachedRenderers;
    private Collider2D[] cachedColliders;
    private EnemyProximityAudio enemyAudio;

    private void Awake()
    {
        EnsureObstacleLayers();
        enemyAudio = GetComponent<EnemyProximityAudio>();

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
        PlayerHideState.OnHiddenStateChanged += OnPlayerHiddenStateChanged;
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        PlayerHideState.OnHiddenStateChanged -= OnPlayerHiddenStateChanged;

        if (followSceneRoutine != null)
        {
            StopCoroutine(followSceneRoutine);
            followSceneRoutine = null;
        }

        if (hideReactionRoutine != null)
        {
            StopCoroutine(hideReactionRoutine);
            hideReactionRoutine = null;
        }

        isHandlingPlayerHide = false;
        hasTriggeredGameOver = false;
        isChasingPlayer = false;
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
        CacheVisibilityComponents();
        TryResolvePlayer();
        hasTriggeredGameOver = false;
        isHandlingPlayerHide = false;
        isChasingPlayer = false;

        timer = wanderTimer;
        SetNewWanderTarget();
    }

    private void Update()
    {
        if (hasTriggeredGameOver && !Screen.IsGameOver)
        {
            hasTriggeredGameOver = false;
        }

        if (Screen.IsGameOver)
        {
            return;
        }

        if (SceneLoadingOverlay.IsLoading || FadeController.IsSceneTransitionFading)
        {
            return;
        }

        if (InventoryToggleUI.IsInventoryOpen())
        {
            return;
        }

        if (isHandlingPlayerHide)
        {
            return;
        }

        if (player == null)
        {
            TryResolvePlayer();
            if (player == null)
            {
                return;
            }
        }

        // Transform-based movement can miss physics callbacks, so keep a direct proximity check.
        TryTriggerGameOverByProximity();
        if (Screen.IsGameOver)
        {
            return;
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
        TryMoveAvoidingObstacles(direction);
    }

    void Wander()
    {
        timer += Time.deltaTime;

        Vector2 direction = (wanderTarget - (Vector2)transform.position).normalized;

        if (!TryMoveAvoidingObstacles(direction))
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

    bool TryMoveAvoidingObstacles(Vector2 preferredDirection)
    {
        if (preferredDirection.sqrMagnitude < 0.0001f)
        {
            return false;
        }

        Vector2 direction = preferredDirection.normalized;
        if (!IsBlocked(direction))
        {
            Move(direction);
            return true;
        }

        for (int i = 0; i < avoidanceAngles.Length; i++)
        {
            Vector2 candidate = Rotate(direction, avoidanceAngles[i]);
            if (!IsBlocked(candidate))
            {
                Move(candidate);
                return true;
            }
        }

        return false;
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

    Vector2 Rotate(Vector2 direction, float angleDegrees)
    {
        float radians = angleDegrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);
        return new Vector2(
            direction.x * cos - direction.y * sin,
            direction.x * sin + direction.y * cos
        );
    }

    void EnsureObstacleLayers()
    {
        if (!autoIncludeDefaultObstacleLayers)
        {
            return;
        }

        AddLayerToObstacleMask("SolidObjects");
        AddLayerToObstacleMask("SoligObjects");
        AddLayerToObstacleMask("Tiles1");
    }

    void AddLayerToObstacleMask(string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer < 0)
        {
            return;
        }

        obstacleLayer |= (1 << layer);
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
        TryResolvePlayer();

        if (!followPlayerAcrossScenes || !isChasingPlayer || isHandlingPlayerHide || PlayerHideState.IsPlayerHidden)
        {
            return;
        }

        if (followSceneRoutine != null)
        {
            StopCoroutine(followSceneRoutine);
        }

        SetEnemyVisible(false);
        followSceneRoutine = StartCoroutine(FollowPlayerToSceneRoutine());
    }

    void OnPlayerHiddenStateChanged(bool isHidden)
    {
        if (!isHidden || !gameObject.activeInHierarchy || isHandlingPlayerHide)
        {
            return;
        }

        TryResolvePlayer();
        bool canSeePlayer = isChasingPlayer;
        if (!canSeePlayer && player != null)
        {
            canSeePlayer = Vector2.Distance(transform.position, player.position) <= detectionRange;
        }

        if (!canSeePlayer)
        {
            return;
        }

        if (hideReactionRoutine != null)
        {
            StopCoroutine(hideReactionRoutine);
        }

        hideReactionRoutine = StartCoroutine(HideReactionRoutine());
    }

    IEnumerator FollowPlayerToSceneRoutine()
    {
        if (sceneFollowDelay > 0f)
        {
            yield return new WaitForSecondsRealtime(sceneFollowDelay);
        }

        TryResolvePlayer();
        if (player == null)
        {
            SetEnemyVisible(true);
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
            transform.position = GetFallbackFollowPosition();
        }

        SetNewWanderTarget();
        SetEnemyVisible(true);
        followSceneRoutine = null;
    }

    IEnumerator HideReactionRoutine()
    {
        isHandlingPlayerHide = true;

        if (followSceneRoutine != null)
        {
            StopCoroutine(followSceneRoutine);
            followSceneRoutine = null;
            SetEnemyVisible(true);
        }

        float minDuration = Mathf.Max(0f, hideWanderMinDuration);
        float maxDuration = Mathf.Max(minDuration, hideWanderMaxDuration);
        float wanderDuration = Random.Range(minDuration, maxDuration);

        float elapsed = 0f;
        while (elapsed < wanderDuration)
        {
            Wander();
            elapsed += Time.deltaTime;
            yield return null;
        }

        SceneTransition door = FindRandomDoorInScene();
        if (door != null)
        {
            yield return MoveToDoorRoutine(door.transform.position);
        }

        DespawnEnemy();
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

    Vector2 GetFallbackFollowPosition()
    {
        if (player == null)
        {
            return transform.position;
        }

        float minDistance = Mathf.Max(0.5f, minSpawnDistanceFromPlayer);
        float maxDistance = Mathf.Max(minDistance, maxSpawnDistanceFromPlayer);

        for (int i = 0; i < 8; i++)
        {
            Vector2 dir = Random.insideUnitCircle.normalized;
            if (dir.sqrMagnitude < 0.0001f)
            {
                dir = Vector2.right;
            }

            float distance = Random.Range(minDistance, maxDistance);
            Vector2 candidate = (Vector2)player.position + dir * distance;

            if (Physics2D.OverlapCircle(candidate, radius, obstacleLayer) == null)
            {
                return candidate;
            }
        }

        // Last fallback: push enemy away from player instead of placing directly on them.
        return (Vector2)player.position + Vector2.right * minDistance;
    }

    SceneTransition FindRandomDoorInScene()
    {
        SceneTransition[] doors = Object.FindObjectsByType<SceneTransition>(FindObjectsSortMode.None);
        if (doors == null || doors.Length == 0)
        {
            return null;
        }

        int index = Random.Range(0, doors.Length);
        return doors[index];
    }

    IEnumerator MoveToDoorRoutine(Vector3 targetPosition)
    {
        float timeout = Mathf.Max(0.1f, doorReachTimeout);
        float elapsed = 0f;

        while (elapsed < timeout)
        {
            Vector2 toTarget = (Vector2)(targetPosition - transform.position);
            if (toTarget.sqrMagnitude <= doorReachDistance * doorReachDistance)
            {
                yield break;
            }

            Vector2 direction = toTarget.normalized;
            TryMoveAvoidingObstacles(direction);

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    void DespawnEnemy()
    {
        isHandlingPlayerHide = false;
        SetEnemyVisible(false);
        gameObject.SetActive(false);
        hideReactionRoutine = null;
    }

    void CacheVisibilityComponents()
    {
        cachedRenderers = GetComponentsInChildren<Renderer>(true);
        cachedColliders = GetComponentsInChildren<Collider2D>(true);
    }

    void SetEnemyVisible(bool visible)
    {
        if (cachedRenderers == null || cachedColliders == null)
        {
            CacheVisibilityComponents();
        }

        for (int i = 0; i < cachedRenderers.Length; i++)
        {
            if (cachedRenderers[i] != null)
            {
                cachedRenderers[i].enabled = visible;
            }
        }

        for (int i = 0; i < cachedColliders.Length; i++)
        {
            if (cachedColliders[i] != null)
            {
                cachedColliders[i].enabled = visible;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryTriggerGameOver(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision == null)
        {
            return;
        }

        TryTriggerGameOver(collision.collider);
    }

    private void TryTriggerGameOver(Collider2D other)
    {
        if (hasTriggeredGameOver || Screen.IsGameOver || other == null || !other.CompareTag("Player"))
        {
            return;
        }

        if (PlayerHideState.IsPlayerHidden)
        {
            return;
        }

        // Play kill sound before game over
        if (enemyAudio != null)
        {
            enemyAudio.PlayKillSound();
        }

        hasTriggeredGameOver = Screen.TriggerGameOver();
    }

    private void TryTriggerGameOverByProximity()
    {
        if (hasTriggeredGameOver || Screen.IsGameOver || player == null || PlayerHideState.IsPlayerHidden)
        {
            return;
        }

        float triggerDistance = Mathf.Max(0.05f, playerContactRadius);
        Vector2 delta = (Vector2)(player.position - transform.position);
        if (delta.sqrMagnitude > triggerDistance * triggerDistance)
        {
            return;
        }

        // Play kill sound before game over
        if (enemyAudio != null)
        {
            enemyAudio.PlayKillSound();
        }

        hasTriggeredGameOver = Screen.TriggerGameOver();
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