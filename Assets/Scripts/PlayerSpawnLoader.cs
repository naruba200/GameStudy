using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawnLoader : MonoBehaviour
{
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        ApplySpawn();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(ApplySpawnNextFrame());
    }

    private IEnumerator ApplySpawnNextFrame()
    {
        yield return null;
        ApplySpawn();
    }

    private void ApplySpawn()
    {
        PlayerController playerController = PlayerPersist.GetPlayerController();
        GameObject player = playerController != null ? playerController.gameObject : GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            bool hasPendingSpawn = PlayerPrefs.GetInt("HasPendingSpawn", 0) == 1;
            string spawnPointId = PlayerPrefs.GetString("SpawnPointId", string.Empty);
            if (!hasPendingSpawn && string.IsNullOrEmpty(spawnPointId))
            {
                PlayerController playerController = player.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.ResumeMovement();
                }

                return;
            }

            bool usedSpawnPoint = false;

            if (!string.IsNullOrEmpty(spawnPointId))
            {
                SpawnPoint[] points = Object.FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
                foreach (SpawnPoint point in points)
                {
                    if (point != null && point.spawnId == spawnPointId)
                    {
                        player.transform.position = point.transform.position;
                        usedSpawnPoint = true;
                        break;
                    }
                }
            }

            if (!usedSpawnPoint)
            {
                float x = PlayerPrefs.GetFloat("SpawnX", player.transform.position.x);
                float y = PlayerPrefs.GetFloat("SpawnY", player.transform.position.y);

                player.transform.position = new Vector2(x, y);
            }

            // 🔥 mở lại di chuyển
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.ResumeMovement();
            }

            PlayerPrefs.SetInt("HasPendingSpawn", 0);
        }
    }
}