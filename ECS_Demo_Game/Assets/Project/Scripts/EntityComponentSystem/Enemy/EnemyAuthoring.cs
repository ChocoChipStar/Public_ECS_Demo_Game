using Unity.Entities;
using UnityEngine;

public class EnemyAuthoring : MonoBehaviour
{
    public float moveMaxSpeed;
    public float moveMinSpeed;

    private class Baker : Baker<EnemyAuthoring>
    {
        public override void Bake(EnemyAuthoring authoring)
        {
            var paramsData = new EnemyParamsData
            {
                moveMaxSpeed = authoring.moveMaxSpeed,
                moveMinSpeed = authoring.moveMinSpeed
            };
            AddComponent(GetEntity(TransformUsageFlags.Dynamic), paramsData);
        }
    }
}

public struct EnemyParamsData : IComponentData
{
    public float moveSpeed;
    public float moveMaxSpeed;
    public float moveMinSpeed;
}