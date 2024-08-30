using Unity.Entities;
using UnityEngine.SceneManagement;

public partial struct CountDrawSystem : ISystem
{
    private void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<CountUpData>();
        state.RequireForUpdate<TitleConfigData>();
    }

    private void OnStartRunning(ref SystemState state)
    {
        var countData = SystemAPI.GetSingleton<CountUpData>();
        countData.isKilledAllEnemy = false;
    }

    private void OnUpdate(ref SystemState state)
    {
        var countData = SystemAPI.GetSingleton<CountUpData>();
        if(countData.isKilledAllEnemy)
        {
            SceneManager.LoadScene("GameClearScene");
        }

        var titleData = SystemAPI.GetSingleton<TitleConfigData>();
        if(!titleData.isDisplayEntitiesCount)
        {
            return;
        }

        countData.SetEnemyCountText();
        countData.SetBulletCountText();
    }
}
