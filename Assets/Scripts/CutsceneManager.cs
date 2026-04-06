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

          // CanvasGroup trên panel đen

    private void Awake() => Instance = this;

    public void PlayDoorCutscene()
    {
        
        StartCoroutine(DoorCutsceneRoutine());
    }

    IEnumerator DoorCutsceneRoutine()
{
    player.StopMovement();

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

        monster.transform.position += Vector3.right * monsterMoveSpeed * Time.deltaTime;
        player.transform.position += Vector3.right * playerRunSpeed * Time.deltaTime;
  
        player.animator.SetBool("isMoving", true);
        player.animator.SetFloat("moveX", 1f);
        player.animator.SetFloat("moveY", 0f);

        yield return null;
    }



    if (ai != null) ai.enabled = true;
    player.ResumeMovement();

}


}