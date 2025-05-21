using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsMenuController : MonoBehaviour
{
    public void GoToMainMenuScene()
    {
        SceneManager.LoadSceneAsync("MainMenu");
        //BootstrapLoader.SceneToLoad = "MainMenu";
        //SceneManager.LoadSceneAsync("LoadingScene");
    }
}
