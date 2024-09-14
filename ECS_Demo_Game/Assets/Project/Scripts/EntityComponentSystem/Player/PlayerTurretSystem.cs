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
        // entityCommandBuffer�́A�G���e�B�e�B��Ԃ̕ύX�f�[�^�������Ă���A
        // Job�������Ɏ��s�ł��Ȃ����߂�ێ�����o�b�t�@�̂���
        var entityCommandBuffer = GetEntityCommandBuffer(ref state);

        // AsParallelWriter��ECB�����ECB�ɕϊ�����
        parallelCommandBuffer = entityCommandBuffer.AsParallelWriter();

        // PlayerParamsData���̃f�[�^��GetSingleton<>�ōēx�擾���Ȃ��ƒl���X�V����Ȃ�
        // ���t���[����Ԃ��m�F���邽�߂ɂ͖��t���[���擾��������Ȃ���΂Ȃ�Ȃ�
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

        // �e�G���e�B�e�B���͈͊O�ɏo�Ĕ�A�N�e�B�u��ԂɂȂ������̂��o�b�t�@���ɒǉ����鏈�������s���܂�
        foreach (var (bulletEntity, disabledEntity) in SystemAPI.Query<RefRO<BulletTag>>().WithEntityAccess().WithAll<Disabled>())
        {
            if (!IsEntityInBulletBuffer(disabledEntity))
            {
                // ��������Ԃ̒e��Buffer�ɑ�����܂�
                bulletBuffer.Add(new BulletBuffer() { entity = disabledEntity });
            }
        }

        var prefabData = SystemAPI.GetSingleton<PrefabData>();
        //SpawnBullet(state);

        if (!bulletBuffer.IsEmpty)
        {
            poolEntity = bulletBuffer[0].entity; // �ė��p����G���e�B�e�B��ݒ�
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

    /// <summary> EntityCommandBuffer���擾���鏈�����s���܂� </summary>
    [BurstCompile]
    private EntityCommandBuffer GetEntityCommandBuffer(ref SystemState state)
    {
        // ���s�^�C�~���O�͑S����3���̓���2�i�ŏ��oBegin�p�ƍŌ�oEnd�p�j�����s����
        // EndSimulationEntityCommandBufferSystem�͎��s�^�C�~���O3�̓���ԍŌ�̃^�C�~���O
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();

        return ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
    }

    /// <summary> �o�b�t�@���ɓ����G���e�B�e�B�����݂��邩�m�F���܂� </summary>
    /// <returns> true -> ���݂��� false -> ���݂��Ȃ� </returns>
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

    // Job�ō\���ύX�̃X�P�W���[�����쐬���AJob������Ƀ��C���X���b�h�ɓK�p���܂�
    // Entity�̔j���␶���A�܂��̓R���|�[�l���g�̒ǉ��A�폜�͍\���ύX�Ƃ݂Ȃ����
    // EntityCommandBuffer���g�p����ƃX���b�h���Z�[�t��Ԃǂ����������ۏ؂����
    [BurstCompile]
    private partial struct FiringBulletJob : IJobEntity
    {
        public bool isExistPoolEntity;

        public LocalTransform poolTransform;

        public DynamicBuffer<BulletBuffer> bulletBuffer;

        public EntityCommandBuffer.ParallelWriter parallelCommandBuffer;

        public PrefabData prefabData;
        public BulletParamsData bulletParamsData;


        // in �ǂݎ��̂݋��@ref �ǂݎ��/�������ݗ���������
        private void Execute(in GunPortTag gunPort, in LocalToWorld worldTransform)
        {
            // ��A�N�e�B�u�G���e�B�e�B�����݂��邩
            if (!isExistPoolEntity)
            {
                // ��A�N�e�B�u�G���e�B�e�B���X�g�̈�ԍŏ��̃G���e�B�e�B���g�p����
                var poolEntity = bulletBuffer[0].entity;

                // �A�N�e�B�u��Ԃɂ���BulletBuffer����폜
                parallelCommandBuffer.RemoveComponent<Disabled>(0,poolEntity);
                bulletBuffer.RemoveAt(0);

                // �������W�ݒ�
                entityCommandBuffer.SetComponent(0,poolEntity, new LocalTransform
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
}
