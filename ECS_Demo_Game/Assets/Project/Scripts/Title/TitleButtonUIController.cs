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

    /// <summary> ���͏����ɍ��킹��UI��\�� </summary>
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

    /// <summary> �I���{�^����ݒ肵�܂� </summary>
    private void SetSelectButton()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(buttons[displayButtonNum]);
    }

    /// <summary> ���͕����̎��Ƀ{�^�������邩�𒲂ׂ܂� </summary>
    /// <returns> True->�{�^������ False->���� </returns>
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
