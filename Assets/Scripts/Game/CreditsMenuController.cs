using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsMenuController : MonoBehaviour
{
    public void GoToMainMenuScene()
    {
        BootstrapLoader.SceneToLoad = "MainMenu";
        SceneManager.LoadSceneAsync("LoadingScene");
    }
}
