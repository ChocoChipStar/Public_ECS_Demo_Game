using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial struct BulletMovementSystem : ISystem
{
    private float3 playerPos;

    private const float DestroyBoarder = 50.0f;

    private EnemySpawnSystem enemySpawnSystem;

    private void OnCreate(ref SystemState state)
    {
        // ��̃R���|�[�l���g�f�[�^�����݂��Ȃ���OnUpdate()�����s����Ȃ�
        state.RequireForUpdate<PlayerParamsData>();
        state.RequireForUpdate<BulletParamsData>();
    }

    [BurstCompile]
    private void OnUpdate(ref SystemState state)
    {
        foreach (var (transform,paramsData) in SystemAPI.Query<RefRW<LocalTransform>,RefRO<PlayerParamsData>>())
        {
            playerPos = transform.ValueRO.Position;
        }

        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var entityCommandBuffer = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        var deltaTime = SystemAPI.Time.DeltaTime;
        foreach (var (localToWorld, transform, bulletData, entity) in SystemAPI.Query<RefRO<LocalToWorld>, RefRW<LocalTransform>, RefRO<BulletParamsData>>().WithEntityAccess())
        {
            var position = transform.ValueRO.Position;
            position += bulletData.ValueRO.direction * bulletData.ValueRO.bulletSpeed * deltaTime;
            transform.ValueRW.Position = position;

            if (math.distance(playerPos, transform.ValueRO.Position) >= DestroyBoarder)
            {
                // ��苗���ȏ㗣�ꂽ��e���A�N�e�B�u�ɂ���
                entityCommandBuffer.AddComponent<Disabled>(entity);
            }
        }
    }
}
