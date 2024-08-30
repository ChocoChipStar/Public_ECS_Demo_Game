using Unity.Entities;
using UnityEngine;

public class TitleConfigDataAuthoring : MonoBehaviour
{
    private int spawnCount;
    private bool isUseEffects;
    private bool isDisplayEntitiesCount;
    private bool isDisplayFPS;
    private bool isSpawnLoop;
    private bool isPlayerHP;

    private class Baker : Baker<TitleConfigDataAuthoring>
    {
        public override void Bake(TitleConfigDataAuthoring authoring)
        {
            var titleConfigData = new TitleConfigData
            {
                spawnCount = authoring.spawnCount,
                isUseEffects = authoring.isUseEffects,
                isDisplayEntitiesCount = authoring.isDisplayEntitiesCount,
                isDisplayFPS = authoring.isDisplayFPS,
                isSpawnLoop = authoring.isSpawnLoop,
                isPlayerHP = authoring.isPlayerHP
            };
            AddComponent(GetEntity(TransformUsageFlags.Dynamic), titleConfigData);
        }
    }
}

public struct TitleConfigData : IComponentData
{
    public int spawnCount;

    public bool isUseEffects;
    public bool isDisplayEntitiesCount;
    public bool isDisplayFPS;
    public bool isSpawnLoop;
    public bool isPlayerHP;
}
