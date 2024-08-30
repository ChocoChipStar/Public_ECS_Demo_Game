using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultButtonUIController : MonoBehaviour
{
    public void OnContinueButton()
    {
        SceneManager.LoadScene(PlayerPrefs.GetString(SceneController.PreviousSceneKey));
    }

    public void OnTitleButton()
    {
        SceneManager.LoadScene("TitleScene");
    }
}
