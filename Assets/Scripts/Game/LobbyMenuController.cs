using Mirror;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyMenuController : MonoBehaviour
{
    public void LeaveLobbyAndReturnToMainMenu()
    {
        if (NetworkClient.isConnected)
            NetworkManager.singleton.StopClient();

        if (NetworkServer.active)
            NetworkManager.singleton.StopHost();

        SteamMatchmaking.LeaveLobby(new CSteamID(SteamLobby.Instance.CurrentLobbyID));

        // 💡 Reset the static BootstrapLoader.initialized flag
        typeof(BootstrapLoader)
            .GetField("initialized", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            ?.SetValue(null, false);

        BootstrapLoader.SceneToLoad = "MainMenu";
        SceneManager.LoadSceneAsync("LoadingScene");
    }

}
