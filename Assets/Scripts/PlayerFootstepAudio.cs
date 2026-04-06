using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerFootstepAudio : MonoBehaviour
{
    [Header("Footstep Audio")]
    [SerializeField] private AudioClip[] footstepSounds = new AudioClip[3];
    [SerializeField] private float footstepVolume = 0.5f;
    [SerializeField] private float volumeVariation = 0.1f; // Random volume variation
    [SerializeField] private float footstepDistance = 0.4f; // Distance walked before playing footstep

    [Header("Movement Detection")]
    [SerializeField] private float minMovementSpeed = 0.1f; // Minimum speed to trigger footstep
    [SerializeField] private bool useDistanceBasedFootsteps = true; // Use distance instead of velocity

    private AudioSource audioSource;
    private Vector3 lastFootstepPosition;
    private Rigidbody2D rigidBody;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        rigidBody = GetComponent<Rigidbody2D>();
        lastFootstepPosition = transform.position;
    }

    private void Update()
    {
        if (useDistanceBasedFootsteps)
        {
            CheckMovementByDistance();
        }
        else
        {
            CheckMovementByVelocity();
        }
    }

    // Check movement based on distance traveled
    private void CheckMovementByDistance()
    {
        float distanceMoved = Vector3.Distance(transform.position, lastFootstepPosition);

        if (distanceMoved >= footstepDistance)
        {
            PlayRandomFootstep();
            lastFootstepPosition = transform.position;
        }
    }

    // Check movement based on velocity
    private void CheckMovementByVelocity()
    {
        if (rigidBody != null)
        {
            float speed = rigidBody.linearVelocity.magnitude;
            if (speed >= minMovementSpeed)
            {
                float distanceMoved = Vector3.Distance(transform.position, lastFootstepPosition);
                if (distanceMoved >= footstepDistance)
                {
                    PlayRandomFootstep();
                    lastFootstepPosition = transform.position;
                }
            }
        }
    }

    private void PlayRandomFootstep()
    {
        if (footstepSounds == null || footstepSounds.Length == 0)
        {
            Debug.LogWarning("No footstep sounds assigned to PlayerFootstepAudio!");
            return;
        }

        // Select random footstep sound from 3
        int randomIndex = Random.Range(0, footstepSounds.Length);
        AudioClip footstep = footstepSounds[randomIndex];

        if (footstep != null && audioSource != null)
        {
            // Add subtle volume variation
            float volumeWithVariation = footstepVolume + Random.Range(-volumeVariation, volumeVariation);
            volumeWithVariation = Mathf.Clamp01(volumeWithVariation);

            audioSource.PlayOneShot(footstep, volumeWithVariation);
        }
    }

    // Method to play footstep manually (useful for animation events)
    public void PlayFootstep()
    {
        PlayRandomFootstep();
        lastFootstepPosition = transform.position;
    }

    public void SetFootstepVolume(float volume)
    {
        footstepVolume = Mathf.Clamp01(volume);
    }

    public void SetFootstepDistance(float distance)
    {
        footstepDistance = Mathf.Max(0.1f, distance);
    }
}
