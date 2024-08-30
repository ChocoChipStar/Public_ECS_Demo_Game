using System;
using TMPro;
using UnityEngine;
using UnityEngine.Profiling;

public class FPSDisplay : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI fpsText;

    private int frameCount = 0;
    
    private float fps = 0.0f;
    private float deltaTime = 0.0f;

    private void Awake()
    {
        if (!ConfigUIController.IsDisplayFPS)
        {
            fpsText.enabled = false;
        }
    }

    void Update()
    {
        if(!ConfigUIController.IsDisplayFPS)
        {
            return;
        }

        deltaTime += 1.0f * Time.deltaTime;
        frameCount++;

        if (deltaTime > 1.0f)
        {
            fpsText.SetText("FPS: {0:0.}", frameCount / deltaTime);
            deltaTime = 0.0f;
            frameCount = 0;
        }
    }
}
