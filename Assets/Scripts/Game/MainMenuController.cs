using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    public Button HostButton;

    void Start()
    {
        StartCoroutine(WaitForSteamLobby());
    }

    IEnumerator WaitForSteamLobby()
    {
        HostButton.interactable = false;

        while (SteamLobby.Instance == null)
        {
            Debug.Log("Waiting for SteamLobby.Instance...");
            yield return null;
        }

        if (SteamLobby.Instance != null)
            Debug.Log("SteamLobby.Instance is ready.");


        HostButton.interactable = true;

        HostButton.onClick.RemoveAllListeners();
        HostButton.onClick.AddListener(() =>
        {
            Debug.Log("Host clicked");
            SteamLobby.Instance.HostLobby();
        });
    }



    public void GoToCustomizationScene()
    {
        SceneManager.LoadSceneAsync("Character");
        //BootstrapLoader.SceneToLoad = "Character";
        //SceneManager.LoadSceneAsync("LoadingScene");

    }

    public void GoToCreditsScene()
    {
        SceneManager.LoadSceneAsync("Credits");
        //BootstrapLoader.SceneToLoad = "Credits";
        //SceneManager.LoadSceneAsync("LoadingScene");
    }
}
