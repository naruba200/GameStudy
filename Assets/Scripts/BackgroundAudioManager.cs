using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BackgroundAudioManager : MonoBehaviour
{
    public static BackgroundAudioManager Instance { get; private set; }

    [Header("Background Audio")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private float backgroundVolume = 0.3f;
    [SerializeField] private bool loopMusic = true;

    private AudioSource audioSource;
    private bool isMusicPlaying;
    private bool isMusicPaused; // Track if music is paused

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        audioSource = GetComponent<AudioSource>();

        // Setup audio source
        audioSource.clip = backgroundMusic;
        audioSource.volume = backgroundVolume;
        audioSource.loop = loopMusic;
        audioSource.playOnAwake = true;
    }

    private void Start()
    {
        if (backgroundMusic != null && !audioSource.isPlaying)
        {
            audioSource.Play();
            isMusicPlaying = true;
            isMusicPaused = false;
        }
    }

    public void PlayBackgroundMusic()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
            isMusicPlaying = true;
            isMusicPaused = false;
        }
    }

    public void StopBackgroundMusic()
    {
        if (audioSource != null && (audioSource.isPlaying || audioSource.timeSamples > 0))
        {
            audioSource.Stop();
        }

        isMusicPlaying = false;
        isMusicPaused = false;
    }

    public void PauseBackgroundMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Pause();
            isMusicPlaying = false;
            isMusicPaused = true;
        }
    }

    public void ResumeBackgroundMusic()
    {
        if (audioSource == null || audioSource.clip == null || audioSource.isPlaying)
        {
            return;
        }

        if (isMusicPaused && audioSource.timeSamples > 0)
        {
            audioSource.UnPause();
        }
        else
        {
            audioSource.Play();
        }

        isMusicPlaying = true;
        isMusicPaused = false;
    }

    public void SetBackgroundVolume(float volume)
    {
        backgroundVolume = Mathf.Clamp01(volume);
        if (audioSource != null)
        {
            audioSource.volume = backgroundVolume;
        }
    }

    public bool IsMusicPlaying => isMusicPlaying;
}
