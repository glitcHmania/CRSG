using Mirror;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyMenuController : MonoBehaviour
{
    public void LeaveLobbyAndReturnToMainMenu()
    {
        if (NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopClient(); // if client
        }

        if (NetworkServer.active)
        {
            NetworkManager.singleton.StopHost(); // if host
        }

        SteamMatchmaking.LeaveLobby(new CSteamID(SteamLobby.Instance.CurrentLobbyID)); // optional

        BootstrapLoader.SceneToLoad = "MainMenu";
        SceneManager.LoadSceneAsync("LoadingScene");

    }

}
