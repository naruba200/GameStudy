using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject spawn = GameObject.Find("SpawnPoint");

        if (player != null && spawn != null)
        {
            player.transform.position = spawn.transform.position;
        }
    }
}