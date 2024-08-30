using UnityEngine;
using UnityEngine.SceneManagement;

public class ForcedQuitScene : MonoBehaviour
{
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("TitleScene");
        }
    }
}
