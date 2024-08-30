using Unity.Entities;
using UnityEngine;

public class PlayerAuthoring : MonoBehaviour
{
    [SerializeField]
    private int health;

    [SerializeField]
    private float rotationSpeed;

    private class Baker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            var data = new PlayerParamsData()
            {
                health = authoring.health,
                rotationSpeed = authoring.rotationSpeed,
            };
            AddComponent(GetEntity(TransformUsageFlags.Dynamic), data);
        }
    }
}

public struct PlayerParamsData : IComponentData
{
    public int health;
    
    public float rotationSpeed;
    public float horizontal;
    
    public bool isPressedSpace;

    public void TakenDamage()
    {
        health--;
    }

    public void SetHealthText()
    {
        TextDrawer.instance.SetPlayerHealthText(health);
    }
}
