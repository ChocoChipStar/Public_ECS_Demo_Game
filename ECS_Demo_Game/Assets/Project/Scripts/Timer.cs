using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Timer : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI timerText;

    private float measureTime = 0.0f;

    private const float LimitTime = 60.0f;

    private void Awake()
    {
        if (!ConfigUIController.IsSpawnLoop)
        {
            timerText.enabled = false;
            return;
        }

        measureTime = LimitTime;
    }

    private void Update()
    {
        if(!ConfigUIController.IsSpawnLoop)
        {
            return;
        }

        measureTime += -1.0f * Time.deltaTime;
        if(measureTime <= 0.0f)
        {
            measureTime = 0.0f;
            SetTimerText((int)measureTime);

            SceneManager.LoadScene("GameClearScene");
        }

        SetTimerText((int)measureTime);
    }

    private void SetTimerText(int value)
    {
        timerText.SetText("TIME:" + value.ToString());
    }
}
