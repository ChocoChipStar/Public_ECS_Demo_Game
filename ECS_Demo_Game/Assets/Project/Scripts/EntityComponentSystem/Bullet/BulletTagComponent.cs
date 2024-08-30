using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class BulletTagComponent : MonoBehaviour
{
    public float bulletSpeed;

    private class Baker : Baker<BulletTagComponent>
    {
        public override void Bake(BulletTagComponent authoring)
        {
            var bulletTAG = new BulletTAG();
            AddComponent(GetEntity(TransformUsageFlags.Dynamic), bulletTAG);

            var paramsData = new BulletParamsData
            {
                bulletSpeed = authoring.bulletSpeed,
            };
            AddComponent(GetEntity(TransformUsageFlags.Dynamic), paramsData);
        }
    }
}

public struct BulletTAG : IComponentData { }

public struct BulletParamsData : IComponentData 
{
    public float bulletSpeed;
    public float3 direction;
};
