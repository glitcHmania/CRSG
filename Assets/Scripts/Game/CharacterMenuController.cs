using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterMenuController : MonoBehaviour
{
    public void GoToMainMenu()
    {
        BootstrapLoader.SceneToLoad = "MainMenu";
        SceneManager.LoadSceneAsync("LoadingScene");

    }
}
