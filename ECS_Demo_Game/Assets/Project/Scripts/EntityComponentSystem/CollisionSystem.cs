using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;

[UpdateInGroup(typeof(AfterPhysicsSystemGroup))]
public partial struct CollisionSystem : ISystem
{
    private void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerParamsData>();
        state.RequireForUpdate<EnemyParamsData>();
    }

    [BurstCompile]
    private void OnUpdate(ref SystemState state)
    {
        var simulation = SystemAPI.GetSingleton<SimulationSingleton>();
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();

        var collisionJob = new CollisionJob
        {
            playerGroup = SystemAPI.GetComponentLookup<PlayerParamsData>(),
            playerData = SystemAPI.GetSingleton<PlayerParamsData>(),
            bulletsGroup = SystemAPI.GetComponentLookup<BulletParamsData>(),
            enemyGroup = SystemAPI.GetComponentLookup<EnemyParamsData>(),
            entityCommandBuffer = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged)
        };
        state.Dependency = collisionJob.Schedule(simulation, state.Dependency);

        JobHandle.ScheduleBatchedJobs();
    }

    [BurstCompile]
    private struct CollisionJob : ITriggerEventsJob
    {
        [ReadOnly] public ComponentLookup<PlayerParamsData> playerGroup;
        public ComponentLookup<BulletParamsData> bulletsGroup;
        public ComponentLookup<EnemyParamsData> enemyGroup;

        public PlayerParamsData playerData;

        public EntityCommandBuffer entityCommandBuffer;

        [BurstCompile]
        public void Execute(TriggerEvent triggerEvent)
        {
            // EntityA��B�ɂ͏Փ˂�����̃G���e�B�e�B���i�[����Ă���
            // ������ EntityA�͓��������Ώە� EntityB�͓��������������g

            var aIsEnemy = enemyGroup.HasComponent(triggerEvent.EntityA);
            var bIsEnemy = enemyGroup.HasComponent(triggerEvent.EntityB);

            // A��B�ǂ�������G�l�~�[�ł͂Ȃ��ꍇ���^�[������
            if (!(aIsEnemy ^ bIsEnemy))
            {
                return;
            }

            var aIsPlayer = playerGroup.HasComponent(triggerEvent.EntityA);
            var bIsPlayer = playerGroup.HasComponent(triggerEvent.EntityB);

            // A��B�ǂ��炩���v���C���[�̏ꍇ
            if (aIsPlayer ^ bIsPlayer)
            {
                var enemyEntity = aIsEnemy ? triggerEvent.EntityA : triggerEvent.EntityB;
                var playerEntity = bIsPlayer ? triggerEvent.EntityB : triggerEvent.EntityA;

                entityCommandBuffer.AddComponent<Disabled>(enemyEntity);

                // HP�_���[�W����
                playerData.TakenDamage();
                entityCommandBuffer.SetComponent(playerEntity, playerData);

                return;
            }

            var aIsBullets = bulletsGroup.HasComponent(triggerEvent.EntityA);
            var bIsBullets = bulletsGroup.HasComponent(triggerEvent.EntityB);

            // A��B�ǂ��炩���e�̏ꍇ
            if (aIsBullets ^ bIsBullets)
            {
                // EntityA���G�l�~�[�̏ꍇ�AenemyEntity��EntityA��������
                var enemyEntity = aIsEnemy ? triggerEvent.EntityA : triggerEvent.EntityB;
                var bulletEntity = bIsBullets ? triggerEvent.EntityB : triggerEvent.EntityA;

                // Disabled�܂���Destroy���g���B
                // ��x�ɔ�\���ɂ�����@��������Ȃ��ׁA�ċA������Disabled���Destroy�̕����y��
                entityCommandBuffer.AddComponent<Disabled>(enemyEntity);
                entityCommandBuffer.AddComponent<Disabled>(bulletEntity);
            }
        }
    }
}
