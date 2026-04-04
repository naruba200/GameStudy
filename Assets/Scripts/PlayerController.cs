using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed;

    private bool isMoving;
    public bool canMove = true;

    private Vector2 input;
    private Animator animator;

    public LayerMask solidObjectsLayer;
    public LayerMask interactableLayer;

    private void Awake() 
    {
        animator = GetComponent<Animator>();
    }

    public void StopMovement()
    {
        StopAllCoroutines();
        canMove = false;
        isMoving = false;
        input = Vector2.zero;

        animator.SetBool("isMoving", false);
    }

    public void ResumeMovement()
    {
        StopAllCoroutines();
        canMove = true;
        isMoving = false;
        input = Vector2.zero;

        animator.SetBool("isMoving", false);
    }

    private void Update()
    {
        if (!canMove)
        {
            isMoving = false;
            animator.SetBool("isMoving", false);
            return;
        }

        if (!isMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            // ❌ OLD: This line forced 4-direction movement
            // if (input.x != 0)
            // {
            //     input.y = 0;
            // }

            // ✅ FIX: Allow diagonal movement (8 directions)

            if (input != Vector2.zero)
            {
                animator.SetFloat("moveX", input.x);
                animator.SetFloat("moveY", input.y);

                var targetPos = transform.position;
                targetPos.x += input.x;
                targetPos.y += input.y;

                if (IsWalkable(targetPos))
                    StartCoroutine(Move(targetPos));
            }
        }
        animator.SetBool("isMoving", isMoving);
    }

    IEnumerator Move(Vector3 targetPos)
    {
        isMoving = true;

        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPos;

        isMoving = false;
    }

    private bool IsWalkable(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, 0.2f, solidObjectsLayer | interactableLayer) != null)
        {
            return false;
        }
        return true;
    }
}