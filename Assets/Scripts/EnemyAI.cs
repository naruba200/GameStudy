using UnityEngine;

public class EnemyAI : MonoBehaviour
{
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

    private Transform player;
    private Vector2 wanderTarget;
    private float timer;
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        timer = wanderTimer;
        SetNewWanderTarget();
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= detectionRange)
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
        rb.MovePosition(rb.position + direction * moveSpeed * Time.deltaTime);
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