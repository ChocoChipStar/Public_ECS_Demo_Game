using UnityEngine;

public class BulletMovement : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed;

    private const float DestroyBoarder = 50.0f;

    private void FixedUpdate()
    {
        transform.position += transform.forward * moveSpeed * Time.deltaTime;

        if (Vector3.Distance(transform.position, PlayerManager.instance.transform.position) >= DestroyBoarder)
        {
            MonoBehaviourCountUp.instance.SubtractBulletCount();
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Enemy"))
        {
            MonoBehaviourCountUp.instance.SubtractEnemyCount();
            MonoBehaviourCountUp.instance.SubtractBulletCount();

            other.gameObject.SetActive(false);
            this.gameObject.SetActive(false);
        }
    }
}