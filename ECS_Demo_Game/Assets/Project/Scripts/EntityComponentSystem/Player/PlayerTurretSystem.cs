using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct PlayerTurretSystem : ISystem
{
    private float measureTime;

    private Entity poolEntity;
    private Entity bulletDataEntity;

    private LocalTransform poolTransform; 

    private BulletDataTAG bulletData;

    private PrefabData prefabData;

    private BulletParamsData bulletParamsData;

    private DynamicBuffer<BulletBuffer> bulletBuffer;

    private EntityCommandBuffer entityCommandBuffer;

    private const float FiringInterval = 0.025f;

    private void OnCreate(ref SystemState state)
    {
        JobsUtility.JobWorkerCount = 12;
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<PrefabData>();
        state.RequireForUpdate<PlayerParamsData>();
    }

    private void OnStartRunning(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        entityCommandBuffer = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        bulletData = SystemAPI.GetSingleton<BulletDataTAG>();
        bulletDataEntity = SystemAPI.GetSingletonEntity<BulletDataTAG>();
        bulletBuffer = state.EntityManager.GetBuffer<BulletBuffer>(bulletDataEntity);
    }

    [BurstCompile]
    private void OnUpdate(ref SystemState state)
    {
        // ���񏈗�����܂ł��Ȃ����ׂ̂Ȃ�������entityManager������
        // ���񏈗����s������entityCommandBuffer�ŏ���������

        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        entityCommandBuffer = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

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
        //var transform = SystemAPI.GetComponentRW<LocalTransform>(bulletEntity);

        //foreach (var (bullet, entity) in SystemAPI.Query<RefRO<BulletTAG>>().WithEntityAccess().WithAll<Disabled>())
        //{
        //    if (!IsEntityInBulletBuffer(entity))
        //    {
        //        // ��������Ԃ̒e��Buffer�ɑ�����܂�
        //        bulletBuffer.Add(new BulletBuffer() { entity = entity });
        //    }
        //}

        prefabData = SystemAPI.GetSingleton<PrefabData>();
        //SpawnBullet(state);

        if (!bulletBuffer.IsEmpty)
        {
            poolEntity = bulletBuffer[0].entity;
            poolTransform = SystemAPI.GetComponent<LocalTransform>(poolEntity);
            bulletParamsData = SystemAPI.GetComponent<BulletParamsData>(poolEntity);
        }


        var firingBulletJob = new FiringBulletJob
        {
            isExistPoolEntity = bulletBuffer.IsEmpty,
            poolTransform = poolTransform,
            entityCommandBuffer = GetEntityCommandBuffer(ref state),
            bulletBuffer = bulletBuffer,
            prefabData = prefabData,
            bulletParamsData = bulletParamsData,
        };
        state.Dependency = firingBulletJob.Schedule(state.Dependency);

        JobHandle.ScheduleBatchedJobs();
    }

    // ���̃A�v���[�`��entityCommandBuffer�̎蓮�ōĐ����Ĕj������K�v���Ȃ�
    /// <summary> EntityCommandBuffer���擾���鏈�����s���܂� </summary>
    [BurstCompile]
    private EntityCommandBuffer GetEntityCommandBuffer(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        return ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
    }

    // Job�ō\���ύX�̃X�P�W���[�����쐬���AJob������Ƀ��C���X���b�h�ɓK�p���܂�
    // Entity�̔j���␶���A�܂��̓R���|�[�l���g�̒ǉ��A�폜�͍\���ύX�Ƃ݂Ȃ����
    // EntityCommandBuffer���g�p����ƃX���b�h���Z�[�t��Ԃǂ����������ۏ؂����
    [BurstCompile]
    private partial struct FiringBulletJob : IJobEntity
    {
        public bool isExistPoolEntity;

        public LocalTransform poolTransform;

        public DynamicBuffer<BulletBuffer> bulletBuffer;

        public EntityCommandBuffer entityCommandBuffer;

        public PrefabData prefabData;
        public BulletParamsData bulletParamsData;


        // in �ǂݎ��̂݋��@ref �ǂݎ��/�������ݗ���������
        private void Execute(in GunPortTag gunPort, in LocalToWorld worldTransform)
        {
            // ��A�N�e�B�u�G���e�B�e�B�����݂��邩
            if (!bulletBuffer.IsEmpty)
            {
                // ��A�N�e�B�u�G���e�B�e�B���X�g�̈�ԍŏ��̃G���e�B�e�B���g�p����
                var poolEntity = bulletBuffer[0].entity;

                // �A�N�e�B�u��Ԃɂ���BulletBuffer����폜
                entityCommandBuffer.RemoveComponent<Disabled>(poolEntity);
                bulletBuffer.RemoveAt(0);

                // �������W�ݒ�
                entityCommandBuffer.SetComponent(poolEntity, new LocalTransform
                {
                    Position = worldTransform.Position,
                    Rotation = quaternion.EulerXYZ(new float3(0, math.radians(90), 0)),
                    Scale = 1.0f
                });

                // �e�ۂ̕�����ݒ�
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

                // �������W�ݒ�
                entityCommandBuffer.SetComponent(instance, new LocalTransform
                {
                    Position = worldTransform.Position,
                    Rotation = quaternion.EulerXYZ(new float3(0, math.radians(90), 0)),
                    Scale = 1.0f
                });

                // �e�ۂ̕�����ݒ�
                var firingAngle = math.normalize
                (
                    new float3(worldTransform.Position.x, 0.0f, worldTransform.Position.z) - new float3(0, 0, 0)
                );

                // �R���|�[�l���g�ɒl����
                entityCommandBuffer.SetComponent(instance, new BulletParamsData
                {
                    bulletSpeed = 20.0f,
                    direction = firingAngle
                });
            }
        }
    }

    /// <summary> �e�̐����������s���܂� </summary>
    //private void SpawnBullet(SystemState state)
    //{
    //    foreach (var worldTransform in SystemAPI.Query<RefRO<LocalToWorld>>().WithAll<GunPortTag>())
    //    {
    //        if (!bulletBuffer.IsEmpty)
    //        {
    //            var poolEntity = bulletBuffer[0].entity;

    //            // �A�N�e�B�u��Ԃɂ���BulletBuffer����폜
    //            entityCommandBuffer.RemoveComponent<Disabled>(poolEntity);
    //            bulletBuffer.RemoveAt(0);

    //            InitializedBulletPosition(state, poolEntity, worldTransform);
    //            continue;
    //        }

    //        // �e�𐶐��i��A�N�e�B�u��Ԃ�entity����̂����Ȃ��ꍇ�̂݁j
    //        var instance = state.EntityManager.Instantiate(prefabData.bulletPrefabEntity);
    //        InitializedBulletPosition(state, instance, worldTransform);
    //    }
    //}

    ///// <summary> �G���e�B�e�B�̏������W��ݒ肵�܂� </summary>
    //private void InitializedBulletPosition(SystemState state, Entity entity, RefRO<LocalToWorld> worldTransform)
    //{
    //    // �������W�ݒ�
    //    var instanceTransform = SystemAPI.GetComponentRW<LocalTransform>(entity);
    //    instanceTransform.ValueRW.Position = worldTransform.ValueRO.Position;
    //    instanceTransform.ValueRW.Rotation = quaternion.EulerXYZ(new float3(0, math.radians(90), 0));

    //    // �e�ۂ̕�����ݒ�
    //    var direction = math.normalize(worldTransform.ValueRO.Position - new float3(0, 0, 0));
    //    var instanceEntityData = SystemAPI.GetComponentRW<BulletParamsData>(entity);
    //    instanceEntityData.ValueRW.direction = new float3(direction.x, 0.0f, direction.z);
    //}

    /// <summary> bulletBuffer ���ɓ����G���e�B�e�B�����݂��邩�m�F���܂� </summary>
    private bool IsEntityInBulletBuffer(Entity entity)
    {
        for (int i = 0; i < bulletBuffer.Length; i++)
        {
            if (bulletBuffer[i].entity == entity)
            {
                return true;
            }
        }
        return false;
    }
}
