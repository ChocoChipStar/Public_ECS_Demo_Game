using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public const string PreviousSceneKey = "PreviousScene";

    private void Start()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString(PreviousSceneKey, currentScene);
    }

    public string GetPreviousScene()
    {
        return PlayerPrefs.GetString(PreviousSceneKey, "No previous scene found");
    }
}
