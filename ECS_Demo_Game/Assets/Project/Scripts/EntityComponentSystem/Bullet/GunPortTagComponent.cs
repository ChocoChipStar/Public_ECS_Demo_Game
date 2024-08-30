using Unity.Entities;
using UnityEngine;

public class GunPortTagComponent : MonoBehaviour 
{
    private class Baker : Baker<GunPortTagComponent>
    {
        public override void Bake(GunPortTagComponent authoring)
        {
            AddComponent(GetEntity(TransformUsageFlags.Dynamic), new GunPortTag());
        }
    }
}

public struct GunPortTag : IComponentData { };
