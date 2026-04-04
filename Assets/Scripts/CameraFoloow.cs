using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraFoloow : MonoBehaviour
{
    public Transform target; // nhân vật
    public float smoothSpeed = 5f;
    public Vector3 offset;

    [Header("Camera Bounds")]
    public bool useBounds = true;
    public bool autoCalculateBounds = true;
    public Collider2D boundsCollider;
    public Renderer boundsRenderer;
    public Vector2 minBounds;
    public Vector2 maxBounds;

    private Camera cam;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryResolveTarget();
    }

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = Camera.main;
        }

        TryResolveTarget();
    }

    void TryResolveTarget()
    {
        if (target != null)
        {
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }
    }

    void OnValidate()
    {
        if (maxBounds.x < minBounds.x)
        {
            float temp = minBounds.x;
            minBounds.x = maxBounds.x;
            maxBounds.x = temp;
        }

        if (maxBounds.y < minBounds.y)
        {
            float temp = minBounds.y;
            minBounds.y = maxBounds.y;
            maxBounds.y = temp;
        }
    }

    void LateUpdate()
    {
        if (target == null)
        {
            TryResolveTarget();
        }

        if (target == null) return;

        Vector2 currentMinBounds = minBounds;
        Vector2 currentMaxBounds = maxBounds;
        if (useBounds && autoCalculateBounds)
        {
            TryGetAutoBounds(out currentMinBounds, out currentMaxBounds);
        }

        Vector3 desiredPosition = target.position + offset;

        desiredPosition.z = transform.position.z;

        if (useBounds)
        {
            ClampPositionToBounds(ref desiredPosition, currentMinBounds, currentMaxBounds);
        }

        Vector3 smoothedPosition = Vector3.Lerp(
        transform.position, 
        desiredPosition, 
        smoothSpeed * Time.deltaTime);

        if (useBounds)
        {
            ClampPositionToBounds(ref smoothedPosition, currentMinBounds, currentMaxBounds);
        }

        transform.position = smoothedPosition;
    }

    void ClampPositionToBounds(ref Vector3 position, Vector2 worldMinBounds, Vector2 worldMaxBounds)
    {
        if (cam == null)
        {
            position.x = Mathf.Clamp(position.x, worldMinBounds.x, worldMaxBounds.x);
            position.y = Mathf.Clamp(position.y, worldMinBounds.y, worldMaxBounds.y);
            return;
        }

        float verticalHalfSize;
        float horizontalHalfSize;

        if (cam.orthographic)
        {
            verticalHalfSize = cam.orthographicSize;
            horizontalHalfSize = verticalHalfSize * cam.aspect;
        }
        else
        {
            float distance = Mathf.Abs(position.z - cam.transform.position.z);
            verticalHalfSize = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * distance;
            horizontalHalfSize = verticalHalfSize * cam.aspect;
        }

        float minX = worldMinBounds.x + horizontalHalfSize;
        float maxX = worldMaxBounds.x - horizontalHalfSize;
        float minY = worldMinBounds.y + verticalHalfSize;
        float maxY = worldMaxBounds.y - verticalHalfSize;

        if (minX > maxX)
        {
            float centerX = (worldMinBounds.x + worldMaxBounds.x) * 0.5f;
            minX = centerX;
            maxX = centerX;
        }

        if (minY > maxY)
        {
            float centerY = (worldMinBounds.y + worldMaxBounds.y) * 0.5f;
            minY = centerY;
            maxY = centerY;
        }

        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.y = Mathf.Clamp(position.y, minY, maxY);
    }

    bool TryGetAutoBounds(out Vector2 outMinBounds, out Vector2 outMaxBounds)
    {
        Bounds bounds;

        if (boundsCollider != null)
        {
            bounds = boundsCollider.bounds;
        }
        else if (boundsRenderer != null)
        {
            bounds = boundsRenderer.bounds;
        }
        else
        {
            outMinBounds = minBounds;
            outMaxBounds = maxBounds;
            return false;
        }

        outMinBounds = bounds.min;
        outMaxBounds = bounds.max;
        return true;
    }
}
