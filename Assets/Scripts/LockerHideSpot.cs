using UnityEngine;

public class LockerHideSpot : MonoBehaviour
{
    [Header("Hide Spot")]
    [SerializeField] private Transform hidePoint;
    [SerializeField] private KeyCode interactKey = KeyCode.F;

    private PlayerController nearbyPlayer;
    private PlayerController hiddenPlayer;
    private Renderer[] hiddenRenderers;
    private Collider2D[] hiddenColliders;

    private void Update()
    {
        if (hiddenPlayer != null)
        {
            if (Input.GetKeyDown(interactKey))
            {
                ExitLocker();
            }

            return;
        }

        if (nearbyPlayer == null)
        {
            return;
        }

        if (Input.GetKeyDown(interactKey))
        {
            EnterLocker(nearbyPlayer);
        }
    }

    private void EnterLocker(PlayerController player)
    {
        if (player == null)
        {
            return;
        }

        hiddenPlayer = player;
        hiddenPlayer.StopMovement();

        Vector3 targetPos = hidePoint != null ? hidePoint.position : transform.position;
        hiddenPlayer.transform.position = targetPos;

        hiddenRenderers = hiddenPlayer.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < hiddenRenderers.Length; i++)
        {
            if (hiddenRenderers[i] != null)
            {
                hiddenRenderers[i].enabled = false;
            }
        }

        hiddenColliders = hiddenPlayer.GetComponents<Collider2D>();
        for (int i = 0; i < hiddenColliders.Length; i++)
        {
            if (hiddenColliders[i] != null)
            {
                hiddenColliders[i].enabled = false;
            }
        }

        PlayerHideState.SetHidden(true, targetPos);
    }

    private void ExitLocker()
    {
        if (hiddenPlayer == null)
        {
            return;
        }

        if (hiddenRenderers != null)
        {
            for (int i = 0; i < hiddenRenderers.Length; i++)
            {
                if (hiddenRenderers[i] != null)
                {
                    hiddenRenderers[i].enabled = true;
                }
            }
        }

        if (hiddenColliders != null)
        {
            for (int i = 0; i < hiddenColliders.Length; i++)
            {
                if (hiddenColliders[i] != null)
                {
                    hiddenColliders[i].enabled = true;
                }
            }
        }

        hiddenPlayer.ResumeMovement();
        PlayerHideState.SetHidden(false, hiddenPlayer.transform.position);

        hiddenPlayer = null;
        hiddenRenderers = null;
        hiddenColliders = null;
    }

    private void OnDisable()
    {
        ExitLocker();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        nearbyPlayer = other.GetComponent<PlayerController>();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null && player == nearbyPlayer)
        {
            nearbyPlayer = null;
        }
    }
}