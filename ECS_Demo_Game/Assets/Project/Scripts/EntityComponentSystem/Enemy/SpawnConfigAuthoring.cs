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

/// <summary> ���������ɕK�v�ȃf�[�^ </summary>
public struct SpawnConfigData : IComponentData
{
    public Entity enemyPrefab;
    public int maxSpawnRadius;
    public int minSpawnRadius;
}

/// <summary> ��A�N�e�B�u��Ԃ̃G�l�~�[��ۊǂ���o�b�t�@ </summary>
[InternalBufferCapacity(1000)]
public struct DisabledEnemyBuffer : IBufferElementData
{
    public Entity entity;
}

