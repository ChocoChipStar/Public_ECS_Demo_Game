using Unity.Burst;
using Unity.Entities;
using UnityEngine;

public partial struct PlayerInputSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerParamsData>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float horizontal = Input.GetAxis("Horizontal");
        bool isPressedSpace = Input.GetButton("Firing");

        foreach (var playerInput in SystemAPI.Query<RefRW<PlayerParamsData>>())
        {
            playerInput.ValueRW.horizontal = horizontal;
            playerInput.ValueRW.isPressedSpace = isPressedSpace;
        }
    }
}
