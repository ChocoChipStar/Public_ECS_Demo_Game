using Unity.Entities;
using UnityEngine;

public class CountUpAuthoring : MonoBehaviour
{
    private int enemyCountValue;
    private int bulletCountValue;

    private class Baker : Baker<CountUpAuthoring>
    {
        public override void Bake(CountUpAuthoring scoreAuthoring)
        {
            var data = new CountUpData
            {
                enemyCountValue = scoreAuthoring.enemyCountValue,
                bulletCountValue = scoreAuthoring.bulletCountValue
            };
            AddComponent(GetEntity(TransformUsageFlags.None), data);
        }
    }
}

public struct CountUpData : IComponentData
{
    public int enemyCountValue;
    public int bulletCountValue;

    public bool isKilledAllEnemy;

    public void SetEnemyCountText()
    {
        TextDrawer.instance.SetEnemyCountText(enemyCountValue);
    }

    public void SetBulletCountText()
    {
        TextDrawer.instance.SetBulletCountText(bulletCountValue);
    }
}
