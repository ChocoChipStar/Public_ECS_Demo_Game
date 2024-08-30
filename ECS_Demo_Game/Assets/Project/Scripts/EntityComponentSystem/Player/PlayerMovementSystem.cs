using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial struct PlayerMovementSystem : ISystem
{
    private void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerParamsData>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (paramsData, transform) in SystemAPI.Query<RefRW<PlayerParamsData>, RefRW<LocalTransform>>())
        {
            transform.ValueRW.Rotation = math.mul
            (
                transform.ValueRW.Rotation, quaternion.RotateY(paramsData.ValueRO.horizontal * paramsData.ValueRO.rotationSpeed)
            );

            var playerPos = transform.ValueRO.Position;
            if(playerPos.y < 0.0f)
            {
                playerPos.y = 0.0f;
            }
            transform.ValueRW.Position = playerPos;
        }
    }
}
