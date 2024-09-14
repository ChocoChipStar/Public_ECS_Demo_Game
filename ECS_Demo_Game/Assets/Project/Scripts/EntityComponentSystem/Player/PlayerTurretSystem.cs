using Mono.Cecil;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct PlayerTurretSystem : ISystem
{
    private float measureTime;

    private Entity poolEntity;
    private LocalTransform poolTransform;

    private BulletParamsData bulletParamsData;

    private DynamicBuffer<BulletBuffer> bulletBuffer;

    private EntityCommandBuffer.ParallelWriter parallelCommandBuffer;

    private const float FiringInterval = 0.025f;

    private void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<BulletDataTag>();
        state.RequireForUpdate<PlayerParamsData>();
    }

    private void OnUpdate(ref SystemState state)
    {
        // entityCommandBufferは、エンティティ状態の変更データが入っており、
        // Job処理中に実行できない命令を保持するバッファのこと
        var entityCommandBuffer = GetEntityCommandBuffer(ref state);

        // AsParallelWriterでECBを並列ECBに変換する
        parallelCommandBuffer = entityCommandBuffer.AsParallelWriter();

        // PlayerParamsData内のデータはGetSingleton<>で再度取得しないと値が更新されない
        // 毎フレーム状態を確認するためには毎フレーム取得操作をしなければならない
        var playerData = SystemAPI.GetSingleton<PlayerParamsData>();
        if (!playerData.isPressedSpace)
        {
            return;
        }

        measureTime += SystemAPI.Time.DeltaTime;
        if (measureTime <= FiringInterval)
        {
            return;
        }
        measureTime = 0.0f;

        var bulletDataEntity = SystemAPI.GetSingletonEntity<BulletDataTag>();
        bulletBuffer = state.EntityManager.GetBuffer<BulletBuffer>(bulletDataEntity);

        // 弾エンティティが範囲外に出て非アクティブ状態になったものをバッファ内に追加する処理を実行します
        foreach (var (bulletEntity, disabledEntity) in SystemAPI.Query<RefRO<BulletTag>>().WithEntityAccess().WithAll<Disabled>())
        {
            if (!IsEntityInBulletBuffer(disabledEntity))
            {
                // 無効化状態の弾をBufferに代入します
                bulletBuffer.Add(new BulletBuffer() { entity = disabledEntity });
            }
        }

        var prefabData = SystemAPI.GetSingleton<PrefabData>();
        //SpawnBullet(state);

        if (!bulletBuffer.IsEmpty)
        {
            poolEntity = bulletBuffer[0].entity; // 再利用するエンティティを設定
            poolTransform = SystemAPI.GetComponent<LocalTransform>(poolEntity);
            bulletParamsData = SystemAPI.GetComponent<BulletParamsData>(poolEntity);
        }

        var firingBulletJob = new FiringBulletJob
        {
            isExistPoolEntity = bulletBuffer.IsEmpty,
            poolTransform = poolTransform,
            parallelCommandBuffer = parallelCommandBuffer,
            bulletBuffer = bulletBuffer,
            prefabData = prefabData,
            bulletParamsData = bulletParamsData,
        };
        state.Dependency = firingBulletJob.Schedule(state.Dependency);

        JobHandle.ScheduleBatchedJobs();
    }

    /// <summary> EntityCommandBufferを取得する処理を行います </summary>
    [BurstCompile]
    private EntityCommandBuffer GetEntityCommandBuffer(ref SystemState state)
    {
        // 実行タイミングは全部で3つその内の2つ（最初｛Begin｝と最後｛End｝）を実行する
        // EndSimulationEntityCommandBufferSystemは実行タイミング3つの内一番最後のタイミング
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();

        return ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
    }

    /// <summary> バッファ内に同じエンティティが存在するか確認します </summary>
    /// <returns> true -> 存在する false -> 存在しない </returns>
    private bool IsEntityInBulletBuffer(Entity disabledEntity)
    {
        for (int i = 0; i < bulletBuffer.Length; i++)
        {
            if (bulletBuffer[i].entity == disabledEntity)
            {
                return true;
            }
        }
        return false;
    }

    // Jobで構造変更のスケジュールを作成し、Job完了後にメインスレッドに適用します
    // Entityの破棄や生成、またはコンポーネントの追加、削除は構造変更とみなされる
    // EntityCommandBufferを使用するとスレッドがセーフ状態どういう事が保証される
    [BurstCompile]
    private partial struct FiringBulletJob : IJobEntity
    {
        public bool isExistPoolEntity;

        public LocalTransform poolTransform;

        public DynamicBuffer<BulletBuffer> bulletBuffer;

        public EntityCommandBuffer.ParallelWriter parallelCommandBuffer;

        public PrefabData prefabData;
        public BulletParamsData bulletParamsData;


        // in 読み取りのみ許可　ref 読み取り/書き込み両方を許可
        private void Execute(in GunPortTag gunPort, in LocalToWorld worldTransform)
        {
            // 非アクティブエンティティが存在するか
            if (!isExistPoolEntity)
            {
                // 非アクティブエンティティリストの一番最初のエンティティを使用する
                var poolEntity = bulletBuffer[0].entity;

                // アクティブ状態にしてBulletBufferから削除
                parallelCommandBuffer.RemoveComponent<Disabled>(0,poolEntity);
                bulletBuffer.RemoveAt(0);

                // 初期座標設定
                entityCommandBuffer.SetComponent(0,poolEntity, new LocalTransform
                {
                    Position = worldTransform.Position,
                    Rotation = quaternion.EulerXYZ(new float3(0, math.radians(90), 0)),
                    Scale = 1.0f
                });

                // 弾丸の方向を設定
                var firingAngle = math.normalize
                (
                    new float3(worldTransform.Position.x, 0.0f, worldTransform.Position.z) - new float3(0, 0, 0)
                );

                entityCommandBuffer.SetComponent(poolEntity, new BulletParamsData
                {
                    bulletSpeed = 20.0f,
                    direction = firingAngle
                });
            }
            else
            {
                var instance = entityCommandBuffer.Instantiate(prefabData.bulletPrefabEntity);

                // 初期座標設定
                entityCommandBuffer.SetComponent(instance, new LocalTransform
                {
                    Position = worldTransform.Position,
                    Rotation = quaternion.EulerXYZ(new float3(0, math.radians(90), 0)),
                    Scale = 1.0f
                });

                // 弾丸の方向を設定
                var firingAngle = math.normalize
                (
                    new float3(worldTransform.Position.x, 0.0f, worldTransform.Position.z) - new float3(0, 0, 0)
                );

                // コンポーネントに値を代入
                entityCommandBuffer.SetComponent(instance, new BulletParamsData
                {
                    bulletSpeed = 20.0f,
                    direction = firingAngle
                });
            }
        }
    }

    /// <summary> 弾の生成処理を行います </summary>
    //private void SpawnBullet(SystemState state)
    //{
    //    foreach (var worldTransform in SystemAPI.Query<RefRO<LocalToWorld>>().WithAll<GunPortTag>())
    //    {
    //        if (!bulletBuffer.IsEmpty)
    //        {
    //            var poolEntity = bulletBuffer[0].entity;

    //            // アクティブ状態にしてBulletBufferから削除
    //            entityCommandBuffer.RemoveComponent<Disabled>(poolEntity);
    //            bulletBuffer.RemoveAt(0);

    //            InitializedBulletPosition(state, poolEntity, worldTransform);
    //            continue;
    //        }

    //        // 弾を生成（非アクティブ状態のentityが一体もいない場合のみ）
    //        var instance = state.EntityManager.Instantiate(prefabData.bulletPrefabEntity);
    //        InitializedBulletPosition(state, instance, worldTransform);
    //    }
    //}

    ///// <summary> エンティティの初期座標を設定します </summary>
    //private void InitializedBulletPosition(SystemState state, Entity entity, RefRO<LocalToWorld> worldTransform)
    //{
    //    // 初期座標設定
    //    var instanceTransform = SystemAPI.GetComponentRW<LocalTransform>(entity);
    //    instanceTransform.ValueRW.Position = worldTransform.ValueRO.Position;
    //    instanceTransform.ValueRW.Rotation = quaternion.EulerXYZ(new float3(0, math.radians(90), 0));

    //    // 弾丸の方向を設定
    //    var direction = math.normalize(worldTransform.ValueRO.Position - new float3(0, 0, 0));
    //    var instanceEntityData = SystemAPI.GetComponentRW<BulletParamsData>(entity);
    //    instanceEntityData.ValueRW.direction = new float3(direction.x, 0.0f, direction.z);
    //}
}
