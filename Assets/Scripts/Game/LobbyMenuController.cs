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

    public void OpenSteamInviteDialog()
    {
        if (SteamManager.Initialized)
        {
            var lobbyID = SteamLobby.Instance.CurrentLobbyID;
            if (lobbyID != 0)
            {
                SteamFriends.ActivateGameOverlayInviteDialog(new CSteamID(lobbyID));
            }
            else
            {
                Debug.LogWarning("No valid Steam lobby ID. Are you in a lobby?");
            }
        }
        else
        {
            Debug.LogWarning("Steam is not initialized. Cannot open invite dialog.");
        }
    }

}


