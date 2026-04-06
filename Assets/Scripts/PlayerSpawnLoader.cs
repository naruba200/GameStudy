using UnityEngine;

public class PlayerSpawnLoader : MonoBehaviour
{
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            bool hasPendingSpawn = PlayerPrefs.GetInt("HasPendingSpawn", 0) == 1;
            if (!hasPendingSpawn)
            {
                PlayerController playerController = player.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.ResumeMovement();
                }

                return;
            }

            bool usedSpawnPoint = false;
            string spawnPointId = PlayerPrefs.GetString("SpawnPointId", string.Empty);

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