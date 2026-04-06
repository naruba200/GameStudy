using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CollectibleAudio : MonoBehaviour
{
    [Header("Collectible Audio")]
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private float collectVolume = 0.8f;
    [SerializeField] private bool destroyAfterSound = true;
    [SerializeField] private float destroyDelay = 0.5f;

    private AudioSource audioSource;
    private bool hasBeenCollected;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayCollectSound()
    {
        if (hasBeenCollected) return;

        hasBeenCollected = true;

        if (collectSound != null && audioSource != null)
        {
            // Disable collider and visuals
            GetComponent<Collider2D>().enabled = false;
            
            if (TryGetComponent<SpriteRenderer>(out var spriteRenderer))
            {
                spriteRenderer.enabled = false;
            }

            // Play sound
            audioSource.clip = collectSound;
            audioSource.volume = collectVolume;
            audioSource.Play();

            // Destroy after sound finishes
            if (destroyAfterSound)
            {
                Destroy(gameObject, collectSound.length + destroyDelay);
            }
        }
    }

    // Optional: Auto-collect on trigger
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayCollectSound();
        }
    }
}
