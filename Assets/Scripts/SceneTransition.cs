using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
    public string sceneToLoad;
    public Vector2 spawnPosition;
    public string destinationSpawnId;
    public SpawnPoint destinationSpawnPoint;

    public FadeController fadeController;

    [Header("Optional Local Fade (per door)")]
    public Animator localFadeAnimator;
    public string localFadeOutState = "End";
    public float localFadeDuration = 1f;

    [Header("Trigger Behavior")]
    public bool autoTransitionOnPlayerEnter = true;
    public bool blockAutoTransitionWhenDoorScriptsPresent = true;
    public bool ignoreSpawnForThisTransition = false;
    public bool resetPersistentObjectsForThisTransition = false;

    private bool isTransitioning;

    private void Awake()
    {
        if (fadeController == null)
        {
            fadeController = Object.FindFirstObjectByType<FadeController>();
        }
    }

    private void OnValidate()
    {
        ApplySpawnPointOverride();
    }

    private void ApplySpawnPointOverride()
    {
        if (destinationSpawnPoint == null)
        {
            return;
        }

        // Keep editor convenience only. Runtime door routing should rely on destinationSpawnId.
        spawnPosition = destinationSpawnPoint.transform.position;
        if (!string.IsNullOrEmpty(destinationSpawnPoint.spawnId))
        {
            destinationSpawnId = destinationSpawnPoint.spawnId;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!autoTransitionOnPlayerEnter)
        {
            return;
        }

        if (blockAutoTransitionWhenDoorScriptsPresent)
        {
            DoorItemLock itemLock = GetComponent<DoorItemLock>();
            if (itemLock != null && itemLock.enabled)
            {
                return;
            }

            DoorInteraction doorInteraction = GetComponent<DoorInteraction>();
            if (doorInteraction != null && doorInteraction.enabled)
            {
                return;
            }
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        TriggerTransition(other.GetComponent<PlayerController>());
    }

    public void TriggerTransition()
    {
        TriggerTransition(FindFirstObjectByType<PlayerController>());
    }

    private void TriggerTransition(PlayerController player)
    {
        if (isTransitioning)
        {
            return;
        }

        isTransitioning = true;

        if (player != null)
        {
            player.StopMovement();
        }

        StartCoroutine(Transition());
    }

    IEnumerator Transition()
    {
        if (fadeController == null)
        {
            fadeController = Object.FindFirstObjectByType<FadeController>();
        }

        if (fadeController != null)
        {
            yield return fadeController.FadeOut();
            fadeController.RequestFadeInOnNextScene();
        }
        else if (localFadeAnimator != null)
        {
            localFadeAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
            localFadeAnimator.enabled = true;
            localFadeAnimator.Play(localFadeOutState, 0, 0f);
            yield return new WaitForSecondsRealtime(localFadeDuration);
        }

        if (ignoreSpawnForThisTransition)
        {
            PlayerPrefs.SetInt("HasPendingSpawn", 0);
            PlayerPrefs.DeleteKey("SpawnPointId");
            PlayerPrefs.DeleteKey("SpawnX");
            PlayerPrefs.DeleteKey("SpawnY");
        }
        else
        {
            PlayerPrefs.SetString("SpawnPointId", destinationSpawnId ?? string.Empty);
            PlayerPrefs.SetFloat("SpawnX", spawnPosition.x);
            PlayerPrefs.SetFloat("SpawnY", spawnPosition.y);
            PlayerPrefs.SetInt("HasPendingSpawn", 1);
        }

        if (resetPersistentObjectsForThisTransition)
        {
            InventoryToggleUI.ResetPersistentInstance();
            CanvasFollowPlayerPersist.ResetPersistentInstance();
            PlayerPersist.ResetPersistentInstance();
            EnemyAI.ResetPersistentInstance();
        }

        SceneManager.LoadScene(sceneToLoad);
    }
}