using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TitleButtonUIController : MonoBehaviour
{
    [SerializeField]
    private ConfigUIController configUIController;

    [SerializeField]
    private GameObject[] buttons = new GameObject[0];

    private int displayButtonNum = 0;

    private const int TopEdge = 0;
    private const int BottomEdge = 2;

    private void Update()
    {
        if(configUIController.IsCanvasActive)
        {
            return;
        }

        DisplaySelectedUI();
    }

    /// <summary> 入力処理に合わせてUIを表示 </summary>
    private void DisplaySelectedUI()
    {
        var keyCurrent = Keyboard.current;
        if (keyCurrent.wKey.wasPressedThisFrame || keyCurrent.upArrowKey.wasPressedThisFrame)
        {
            if (SearchNextButton(TopEdge))
            {
                return;
            }

            --displayButtonNum;
            SetSelectButton();
        }

        if (keyCurrent.sKey.wasPressedThisFrame || keyCurrent.downArrowKey.wasPressedThisFrame)
        {
            if (SearchNextButton(BottomEdge))
            {
                return;
            }

            ++displayButtonNum;
            SetSelectButton();
        }
    }

    /// <summary> 選択ボタンを設定します </summary>
    private void SetSelectButton()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(buttons[displayButtonNum]);
    }

    /// <summary> 入力方向の次にボタンがあるかを調べます </summary>
    /// <returns> True->ボタンあり False->無し </returns>
    private bool SearchNextButton(int edge)
    {
        return displayButtonNum == edge;
    }

    public void OnClickMonoBehaviourButton()
    {
        SceneManager.LoadScene("MonoBehaviourScene");
    }

    public void OnClickEntityComponentSystem()
    {
        SceneManager.LoadScene("EntityComponentSystemScene");
    }
}
