using UnityEngine;

public class FlashlightRotate4Dir : MonoBehaviour
{
    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        Vector2 dir = new Vector2(x, y);

        if (dir == Vector2.zero) return;

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            transform.rotation = (dir.x > 0)
                ? Quaternion.Euler(0, 0, 0)
                : Quaternion.Euler(0, 0, 180);
        }
        else
        {
            transform.rotation = (dir.y > 0)
                ? Quaternion.Euler(0, 0, 90)
                : Quaternion.Euler(0, 0, -90);
        }
    }
}