using UnityEngine;
using System.Collections;

public class PickupEnemyActivator : MonoBehaviour
{
    [Header("Enemy Reference")]
    [SerializeField] private GameObject targetEnemy;
    
    [Header("Timing")]
    [SerializeField] private float delayBeforeActivation = 2f;

    private bool hasBeenPickedUp = false;

    public void OnCollected(MonoBehaviour coroutineRunner)
    {
        if (hasBeenPickedUp)
        {
            return;
        }

        hasBeenPickedUp = true;

        if (coroutineRunner != null)
        {
            coroutineRunner.StartCoroutine(ActivateEnemyAfterDelay());
            return;
        }

        StartCoroutine(ActivateEnemyAfterDelay());
    }

    private IEnumerator ActivateEnemyAfterDelay()
    {
        // Chờ theo delay time
        yield return new WaitForSeconds(delayBeforeActivation);
        
        // Active enemy
        if (targetEnemy != null)
        {
            targetEnemy.SetActive(true);
            Debug.Log("Enemy đã được active!");
        }
        else
        {
            Debug.LogWarning("PickupEnemyActivator: Không tìm thấy Enemy reference!");
        }
    }

    // Hàm để reset nếu cần
    public void ResetPickup()
    {
        hasBeenPickedUp = false;
        gameObject.SetActive(true);
    }
}
