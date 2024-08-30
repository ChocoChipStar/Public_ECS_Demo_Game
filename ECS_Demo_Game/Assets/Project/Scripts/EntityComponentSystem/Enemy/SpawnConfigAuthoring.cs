using Unity.Entities;
using UnityEngine;

public class SpawnConfigAuthoring : MonoBehaviour
{
    public GameObject enemyPrefab;
    public int maxSpawnRadius;
    public int minSpawnRadius;

    private class Baker : Baker<SpawnConfigAuthoring>
    {
        public override void Bake(SpawnConfigAuthoring authoring)
        {
            var spawnConfigEntity = GetEntity(TransformUsageFlags.None);

            var spawnConfigData = new SpawnConfigData
            {
                enemyPrefab = GetEntity(authoring.enemyPrefab, TransformUsageFlags.Dynamic),
                maxSpawnRadius = authoring.maxSpawnRadius,
                minSpawnRadius = authoring.minSpawnRadius,
            };
            AddComponent(spawnConfigEntity, spawnConfigData);

            var buffer = AddBuffer<DisabledEnemyBuffer>(spawnConfigEntity);
        }
    }
}

/// <summary> 生成処理に必要なデータ </summary>
public struct SpawnConfigData : IComponentData
{
    public Entity enemyPrefab;
    public int maxSpawnRadius;
    public int minSpawnRadius;
}

/// <summary> 非アクティブ状態のエネミーを保管するバッファ </summary>
[InternalBufferCapacity(1000)]
public struct DisabledEnemyBuffer : IBufferElementData
{
    public Entity entity;
}

