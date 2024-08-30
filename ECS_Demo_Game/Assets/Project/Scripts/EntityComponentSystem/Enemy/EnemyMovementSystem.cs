using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public partial struct EnemyMovementSystem : ISystem
{
    private void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerParamsData>();
        state.RequireForUpdate<EnemyParamsData>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var playerEntity = SystemAPI.GetSingletonEntity<PlayerParamsData>();
        var playerPos = SystemAPI.GetComponentRW<LocalTransform>(playerEntity).ValueRO.Position;

        foreach (var (paramsData, transform, velocity) in SystemAPI.Query<RefRO<EnemyParamsData>, RefRW<LocalTransform>, RefRW<PhysicsVelocity>>())
        {
            var enemyPos = transform.ValueRO.Position;
            var velocityLinear = velocity.ValueRO.Linear;

            var direction = playerPos - enemyPos;
            var radian = Mathf.Atan2(-direction.z, direction.x);
            var degree = radian * Mathf.Rad2Deg - 90.0f;

            velocityLinear = new Vector3(direction.x, 0.0f, direction.z).normalized * paramsData.ValueRO.moveSpeed;

            velocity.ValueRW.Linear = velocityLinear;
            velocity.ValueRW.Angular = float3.zero;

            if(enemyPos.y < 0.0f)
            {
                enemyPos.y = 0.0f;
            }

            transform.ValueRW.Position = enemyPos;
            transform.ValueRW.Rotation = Quaternion.Euler(0.0f, degree, 0.0f);
        }
    }
}
