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

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    IEnumerator WaitForSteamLobby()
    {
        HostButton.interactable = false;

        float timeout = 5f;
        float timer = 0f;

        while (SteamLobby.Instance == null && timer < timeout)
        {
            Debug.Log("Waiting for SteamLobby.Instance...");
            timer += Time.deltaTime;
            yield return null;
        }

        if (SteamLobby.Instance == null)
        {
            Debug.LogError("❌ Failed to get SteamLobby.Instance. Attempting recovery...");

            var found = GameObject.FindObjectOfType<SteamLobby>();
            if (found != null)
            {
                Debug.Log("✅ Recovered SteamLobby from scene.");
                SteamLobby.Instance = found;
            }
            else
            {
                Debug.LogError("❌ SteamLobby still not found in scene.");
                yield break;
            }
        }

        Debug.Log("✅ SteamLobby.Instance is ready.");

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

    public void StartSinglePlayer()
    {
        BootstrapLoader.SceneToLoad = "Game";
        SceneManager.LoadSceneAsync("LoadingScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
