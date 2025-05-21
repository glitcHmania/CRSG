using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterMenuController : MonoBehaviour
{
    public void GoToMainMenu()
    {
        SceneManager.LoadSceneAsync("MainMenu");
        //BootstrapLoader.SceneToLoad = "MainMenu";
        //SceneManager.LoadSceneAsync("LoadingScene");

    }
}
