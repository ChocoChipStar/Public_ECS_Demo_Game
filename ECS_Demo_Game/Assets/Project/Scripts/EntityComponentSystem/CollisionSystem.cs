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
            // EntityAとBには衝突した二つのエンティティが格納されている
            // ※推測 EntityAは当たった対象物 EntityBは当たった自分自身

            var aIsEnemy = enemyGroup.HasComponent(triggerEvent.EntityA);
            var bIsEnemy = enemyGroup.HasComponent(triggerEvent.EntityB);

            // AとBどちらもがエネミーではない場合リターンする
            if (!(aIsEnemy ^ bIsEnemy))
            {
                return;
            }

            var aIsPlayer = playerGroup.HasComponent(triggerEvent.EntityA);
            var bIsPlayer = playerGroup.HasComponent(triggerEvent.EntityB);

            // AとBどちらかがプレイヤーの場合
            if (aIsPlayer ^ bIsPlayer)
            {
                var enemyEntity = aIsEnemy ? triggerEvent.EntityA : triggerEvent.EntityB;
                var playerEntity = bIsPlayer ? triggerEvent.EntityB : triggerEvent.EntityA;

                entityCommandBuffer.AddComponent<Disabled>(enemyEntity);

                // HPダメージ処理
                playerData.TakenDamage();
                entityCommandBuffer.SetComponent(playerEntity, playerData);

                return;
            }

            var aIsBullets = bulletsGroup.HasComponent(triggerEvent.EntityA);
            var bIsBullets = bulletsGroup.HasComponent(triggerEvent.EntityB);

            // AとBどちらかが弾の場合
            if (aIsBullets ^ bIsBullets)
            {
                // EntityAがエネミーの場合、enemyEntityにEntityAを代入する
                var enemyEntity = aIsEnemy ? triggerEvent.EntityA : triggerEvent.EntityB;
                var bulletEntity = bIsBullets ? triggerEvent.EntityB : triggerEvent.EntityA;

                // DisabledまたはDestroyを使う。
                // 一度に非表示にする方法が見つからない為、再帰処理のDisabledよりDestroyの方が軽い
                entityCommandBuffer.AddComponent<Disabled>(enemyEntity);
                entityCommandBuffer.AddComponent<Disabled>(bulletEntity);
            }
        }
    }
}
