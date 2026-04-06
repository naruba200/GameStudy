using UnityEngine;
using System.Collections;

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance;

    [Header("References")]
    public GameObject monster;           // kéo monster prefab/gameobject vào
    public Transform monsterSpawnPoint;  // empty object bên trái màn hình
    public PlayerController player;

    [Header("Cutscene Settings")]
    public float monsterMoveSpeed = 4f;  // tốc độ monster chạy vào
    public float playerRunSpeed = 3f;    // tốc độ player chạy phải
    public float cutsceneDuration = 3f;  // bao lâu trước khi fade
    public float fadeDuration = 1f;
    private bool isPlaying;

          // CanvasGroup trên panel đen

    private void Awake() => Instance = this;

    public void PlayDoorCutscene()
    {
        if (isPlaying)
        {
            return;
        }

        StartCoroutine(DoorCutsceneRoutine());
    }

    private bool TryResolvePlayer(out PlayerController resolvedPlayer)
    {
        resolvedPlayer = player;

        if (resolvedPlayer == null)
        {
            resolvedPlayer = PlayerPersist.GetPlayerController();
        }

        if (resolvedPlayer == null)
        {
            resolvedPlayer = FindFirstObjectByType<PlayerController>();
        }

        player = resolvedPlayer;
        return resolvedPlayer != null;
    }

    IEnumerator DoorCutsceneRoutine()
    {
        isPlaying = true;

        if (!TryResolvePlayer(out PlayerController resolvedPlayer) || monster == null || monsterSpawnPoint == null)
        {
            isPlaying = false;
            yield break;
        }

        resolvedPlayer.StopMovement();

        monster.transform.position = monsterSpawnPoint.position;
        monster.SetActive(true);
        yield return null;


        // Đảm bảo tắt AI trước khi SetActive
        EnemyAI ai = monster.GetComponent<EnemyAI>();
        if (ai != null) ai.enabled = false;

        monster.transform.position = monsterSpawnPoint.position;
        monster.SetActive(true);

        // Chờ 1 frame để monster khởi tạo xong
        yield return null;

        float elapsed = 0f;
        while (elapsed < cutsceneDuration)
        {
            elapsed += Time.deltaTime;

            if (resolvedPlayer == null || monster == null)
            {
                break;
            }

            monster.transform.position += Vector3.right * monsterMoveSpeed * Time.deltaTime;
            resolvedPlayer.transform.position += Vector3.right * playerRunSpeed * Time.deltaTime;

            resolvedPlayer.SetMovementAnimation(Vector2.right, true);

            yield return null;
        }



        if (ai != null) ai.enabled = true;
        if (resolvedPlayer != null)
        {
            resolvedPlayer.ResumeMovement();
        }

        isPlaying = false;
    }


}