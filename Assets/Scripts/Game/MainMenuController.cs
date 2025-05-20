using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    public Button hostButton;

    void Start()
    {
        hostButton.onClick.RemoveAllListeners();
        hostButton.onClick.AddListener(() =>
        {
            Debug.Log("Host clicked");
            SteamLobby.Instance.HostLobby();
        });
    }

    public void GoToCustomizationScene()
    {
        SceneManager.LoadScene("Character");
    }
}
