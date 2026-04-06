using UnityEngine;
using System.Collections;

public class DoorCon : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip openSound;
    public AudioClip closeSound;
    private AudioSource audioSource;

    [Header("Sprites")]
    public GameObject doorClosed;
    public GameObject doorOpen;
    public bool isOpen = false;

    [Header("Collider")]
    public Collider2D doorCollider;

    [Header("Enemy Fade")]
    public GameObject enemy;
    public float fadeDuration = 1f;

    private bool enemyHidden = false;
    private int playerCount = 0;

    void Start()
{
    audioSource = GetComponent<AudioSource>();

    doorClosed.SetActive(!isOpen);
    doorOpen.SetActive(isOpen);
    doorCollider.enabled = !isOpen;

    if (enemy != null)
    {
        // Ẩn enemy lúc đầu nhưng KHÔNG set enemyHidden = true
        enemy.SetActive(false);
        EnemyAI ai = enemy.GetComponent<EnemyAI>();
        if (ai != null) ai.enabled = false;
    }
}

    void Update()
    {
        if (playerCount > 0 && Input.GetKeyDown(KeyCode.E))
            ToggleDoor();
    }

    void ToggleDoor()
{
    isOpen = !isOpen;
    Debug.Log($"ToggleDoor | isOpen: {isOpen} | enemyHidden: {enemyHidden} | enemy: {enemy}");
    
    doorClosed.SetActive(!isOpen);
    doorOpen.SetActive(isOpen);
    doorCollider.enabled = !isOpen;

    if (audioSource != null)
    {
        AudioClip clip = isOpen ? openSound : closeSound;
        if (clip != null) audioSource.PlayOneShot(clip);
    }

    if (!isOpen && enemy != null && enemy.activeSelf && !enemyHidden)
{
    enemyHidden = true;
    StartCoroutine(FadeOutEnemy());
}
}

    IEnumerator FadeOutEnemy()
    {
        SpriteRenderer sr = enemy.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        EnemyAI ai = enemy.GetComponent<EnemyAI>();
        if (ai != null) ai.enabled = false;

        float elapsed = 0f;
        Color startColor = sr.color;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        enemy.SetActive(false);
    }

    public void OnPlayerEnter() => playerCount++;
    public void OnPlayerExit() => playerCount = Mathf.Max(0, playerCount - 1);
}