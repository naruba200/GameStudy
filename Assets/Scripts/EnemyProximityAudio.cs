using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EnemyProximityAudio : MonoBehaviour
{
    [Header("Enemy Detection")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float proximityDistance = 5f; // Distance to trigger alert
    [SerializeField] private float checkInterval = 0.5f; // How often to check distance

    [Header("Alert Audio")]
    [SerializeField] private AudioClip alertSound;
    [SerializeField] private float alertVolume = 0.7f;
    [SerializeField] private float alertCooldown = 2f; // Time between alert sounds
    [SerializeField] private bool loopAlertSound = false; // No loop for alert

    [Header("Kill Sound")]
    [SerializeField] private AudioClip killSound;
    [SerializeField] private float killVolume = 1f;

    private AudioSource audioSource;
    private float nextAlertTime;
    private float nextCheckTime;
    private bool isEnemyNear;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = loopAlertSound; // Tắt loop

        // Find player if not assigned
        if (playerTransform == null)
        {
            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
    }

    private void Update()
    {
        if (playerTransform == null) return;

        // Check distance at intervals
        if (Time.time >= nextCheckTime)
        {
            CheckPlayerProximity();
            nextCheckTime = Time.time + checkInterval;
        }
    }

    private void CheckPlayerProximity()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= proximityDistance)
        {
            if (!isEnemyNear)
            {
                // Enemy just got close
                isEnemyNear = true;
                TriggerAlarm();
                PauseBackgroundMusic();
            }
            else if (Time.time >= nextAlertTime)
            {
                // Repeat alert sound if enemy stays close
                TriggerAlarm();
            }
        }
        else
        {
            if (isEnemyNear)
            {
                // Enemy moved away
                isEnemyNear = false;
                ResumeBackgroundMusic();
            }
        }
    }

    private void TriggerAlarm()
    {
        if (alertSound != null && audioSource != null && Time.time >= nextAlertTime)
        {
            audioSource.PlayOneShot(alertSound, alertVolume);
            nextAlertTime = Time.time + alertCooldown;
        }
    }

    private void StopBackgroundMusic()
    {
        BackgroundAudioManager backgroundManager = BackgroundAudioManager.Instance;
        if (backgroundManager != null)
        {
            backgroundManager.PauseBackgroundMusic();
        }
    }
    private void PauseBackgroundMusic()
    {
        BackgroundAudioManager backgroundManager = BackgroundAudioManager.Instance;
        if (backgroundManager != null)
        {
            backgroundManager.PauseBackgroundMusic();
        }
    }
    private void ResumeBackgroundMusic()
    {
        BackgroundAudioManager backgroundManager = BackgroundAudioManager.Instance;
        if (backgroundManager != null)
        {
            backgroundManager.ResumeBackgroundMusic();
        }
    }

    // Visualize proximity range in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, proximityDistance);
    }

    public void PlayKillSound()
    {
        if (killSound != null && audioSource != null)
        {
            // Stop background music
            BackgroundAudioManager backgroundManager = BackgroundAudioManager.Instance;
            if (backgroundManager != null)
            {
                backgroundManager.StopBackgroundMusic();
            }

            audioSource.PlayOneShot(killSound, killVolume);
        }
    }

    public bool IsEnemyNear => isEnemyNear;
    public float DistanceToPlayer => playerTransform != null ? 
        Vector3.Distance(transform.position, playerTransform.position) : Mathf.Infinity;
}
