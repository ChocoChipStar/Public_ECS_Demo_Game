using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfigUIController : MonoBehaviour
{
    [SerializeField]
    private GameObject configCanvas = null;

    [SerializeField]
    private Slider spawnCountSlider = null;

    [SerializeField]
    private TextMeshProUGUI sliderValueText = null;

    private float spawnCount = 5000.0f;

    private bool isCanvasActive = false;
    private bool isUseEffects = false;
    private bool isDisplayEntitiesCount = true;
    private bool isDisplayFPS = true;
    private bool isSpawnLoop = true;
    private bool isPlayerHP = true;

    private const int SpawnCountMax = 15000;

    public bool IsCanvasActive { get { return isCanvasActive; } private set { isCanvasActive = value; } }

    public static int SpawnCount { get; private set; }
    public static bool IsUseEffects { get; private set; }
    public static bool IsDisplayEntitiesCount { get; private set; }
    public static bool IsDisplayFPS { get; private set; }
    public static bool IsSpawnLoop { get; private set; }
    public static bool IsPlayerHP { get; private set; }

    private void Start()
    {
        OnClickApplyButton();
    }

    private void Update()
    {
        if(!IsCanvasActive)
        {
            return;
        }

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            OnBackButton();
        }

        SetSliderValue();
    }

    private void SetSliderValue()
    {
        spawnCount = SpawnCountMax * spawnCountSlider.value;
        sliderValueText.SetText("" + (int)spawnCount);
    }

    public void OnClickApplyButton()
    {
        SpawnCount = (int)spawnCount;
        IsUseEffects = isUseEffects;
        IsDisplayEntitiesCount = isDisplayEntitiesCount;
        IsDisplayFPS = isDisplayFPS;
        IsSpawnLoop = isSpawnLoop;
        IsPlayerHP = isPlayerHP;

        OnBackButton();
    }

    public void OnBackButton()
    {
        isCanvasActive = false;
        configCanvas.SetActive(false);
    }

    public void OnClickConfigButton()
    {
        isCanvasActive = true;
        configCanvas.SetActive(true);
    }

    public void OnClickUseEffectsToggle()
    {
        isUseEffects = !isUseEffects;
    }

    public void OnClickDisplayEntitiesCountToggle()
    {
        isDisplayEntitiesCount = !isDisplayEntitiesCount;
    }

    public void OnClickDisplayFPSToggle()
    {
        isDisplayFPS = !isDisplayFPS;
    }

    public void OnClickSpawnLoopToggle()
    {
        isSpawnLoop = !isSpawnLoop; 
    }
    
    public void OnClickPlayerHPToggle()
    {
        isPlayerHP = !isPlayerHP;
    }
}