using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    public void QuitGame()
    {
        Application.Quit();
    }

    public void SwitchToScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public static void SwitchToScene_Static(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
