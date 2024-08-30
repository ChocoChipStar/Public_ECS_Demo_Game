using Unity.Burst;
using Unity.Entities;

public partial struct CountUpSystem : ISystem
{
    private const int EnemyPrefabInstanceCount = 1;

    private void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<CountUpData>();
        state.RequireForUpdate<EnemyParamsData>();
        state.RequireForUpdate<TitleConfigData>();
    }

    [BurstCompile]
    private void OnUpdate(ref SystemState state)
    {
        var countData = SystemAPI.GetSingleton<CountUpData>();

        countData.enemyCountValue = 0;
        foreach (var (enemyData, entity) in SystemAPI.Query<RefRO<EnemyParamsData>>().WithEntityAccess())
        {
            countData.enemyCountValue++;
        }

        var titleData = SystemAPI.GetSingleton<TitleConfigData>();
        if (!titleData.isSpawnLoop && countData.enemyCountValue == 0 + EnemyPrefabInstanceCount)
        {
            countData.isKilledAllEnemy = true;
        }

        countData.bulletCountValue = 0;
        foreach (var (bulletData, entity) in SystemAPI.Query<RefRO<BulletParamsData>>().WithEntityAccess())
        {
            countData.bulletCountValue++;
        }

        SystemAPI.SetSingleton(countData);
    }
}
