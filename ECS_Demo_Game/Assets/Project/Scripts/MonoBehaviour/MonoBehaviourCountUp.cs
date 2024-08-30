using TMPro;
using UnityEngine;

public class MonoBehaviourCountUp : MonoBehaviour
{
    public static MonoBehaviourCountUp instance = new MonoBehaviourCountUp();

    [SerializeField]
    private TextMeshProUGUI enemyCountText = null;

    [SerializeField]
    private TextMeshProUGUI bulletCountText = null;

    private int enemyCount = 0;
    private int bulletCount = 0;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(instance);
        }
    }

    private void Start()
    {
        if(!ConfigUIController.IsDisplayEntitiesCount)
        {
            enemyCountText.enabled = false;
            bulletCountText.enabled = false;
        }
    }

    public void AddEnemyCount()
    {
        enemyCount++;
        enemyCountText.SetText("ENEMIES:" + enemyCount);
    }

    public void SubtractEnemyCount()
    {
        enemyCount--;
        enemyCountText.SetText("ENEMIES:" + enemyCount);
    }

    public void AddBulletCount()
    {
        bulletCount++;
        bulletCountText.SetText("BULLETS:" + bulletCount);
    }

    public void SubtractBulletCount()
    {
        bulletCount--;
        bulletCountText.SetText("BULLETS:" + bulletCount);
    }
}
