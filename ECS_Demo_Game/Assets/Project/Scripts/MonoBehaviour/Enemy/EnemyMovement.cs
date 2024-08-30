using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public float moveSpeed;

    private void FixedUpdate()
    {
        var direction = PlayerManager.instance.transform.position - this.transform.position;
        var radian = Mathf.Atan2(-direction.z, direction.x);
        var degree = radian * Mathf.Rad2Deg + 90.0f;

        transform.position += new Vector3(direction.x, 0.0f, direction.z).normalized * moveSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Euler(90.0f, degree, 0.0f);
    }
}
