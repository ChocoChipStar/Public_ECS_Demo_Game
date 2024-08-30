using UnityEngine;

public class EnemyGenerator : MonoBehaviour
{
    [SerializeField]
    private GameObject enemyPrefab = null;

    [SerializeField]
    private int maxGenerateRadius = 0;

    [SerializeField]
    private int minGenerateRadius = 0;

    [SerializeField]
    private int maxMoveSpeed = 0;

    [SerializeField]
    private int minMoveSpeed = 0;

    private float measureTime = 0.0f;

    private const float GenerateInterval = 0.01f;

    private void Start()
    {
        if (!ConfigUIController.IsSpawnLoop)
        {
            for (int i = 0; i < ConfigUIController.SpawnCount; ++i)
            {
                Generator();
            }
        }
    }

    private void Update()
    {
        if (!ConfigUIController.IsSpawnLoop)
        {
            return;
        }

        //measureTime += Time.deltaTime;
        //if (measureTime <= GenerateInterval)
        //{
        //    return;
        //}
        //measureTime = 0.0f;

        Generator();
    }

    private void Generator()
    {
        var maxRadius = Mathf.Pow(maxGenerateRadius, 2);
        var minRadius = Mathf.Pow(minGenerateRadius, 2);

        var randomPos = new Vector3();
        for(int i = 0; i < 100; i++)
        {
            var randomX = Random.Range(-maxGenerateRadius, maxGenerateRadius);
            var randomZ = Random.Range(-maxGenerateRadius, maxGenerateRadius);

            var xAbs = Mathf.Pow(randomX, 2);
            var zAbs = Mathf.Pow(randomZ, 2);

            if(maxRadius > xAbs + zAbs && xAbs + zAbs > minRadius)
            {
                randomPos = new Vector3(randomX, 0.0f, randomZ);
                break;
            }
        }

        var instance = Instantiate(enemyPrefab,randomPos,Quaternion.identity);
        var enemyMove = instance.GetComponent<EnemyMovement>();
        enemyMove.moveSpeed = Random.Range(minMoveSpeed,maxMoveSpeed);
        MonoBehaviourCountUp.instance.AddEnemyCount();
    }
}
