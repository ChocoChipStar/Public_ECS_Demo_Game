using Unity.Entities;
using UnityEngine;

public class BulletDataAuthoring : MonoBehaviour
{
    public GameObject bulletPrefab;

    private class Baker : Baker<BulletDataAuthoring>
    {
        public override void Bake(BulletDataAuthoring authoring)
        {
            AddComponent(GetEntity(TransformUsageFlags.None), new BulletDataTag());

            var prefabData = new PrefabData
            {
                bulletPrefabEntity = GetEntity(authoring.bulletPrefab, TransformUsageFlags.Dynamic),
            };
            AddComponent(GetEntity(TransformUsageFlags.Dynamic), prefabData);

            AddBuffer<BulletBuffer>(GetEntity(TransformUsageFlags.None));
        }
    }
}

public struct BulletDataTag : IComponentData { }

public struct PrefabData : IComponentData 
{
    public Entity bulletPrefabEntity;
};

[InternalBufferCapacity(1000)]
public struct BulletBuffer : IBufferElementData
{
    public Entity entity;
}
