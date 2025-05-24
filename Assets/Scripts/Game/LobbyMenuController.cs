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
        if (!SteamManager.Initialized)
        {
            Debug.LogWarning("Steam is not initialized.");
            return;
        }

        var lobbyID = SteamLobby.Instance.CurrentLobbyID;

        if (lobbyID == 0)
        {
            Debug.LogWarning("Lobby ID is 0. Are you in a Steam lobby?");
            return;
        }

        var steamID = new CSteamID(lobbyID);
        Debug.Log($"Trying to open invite dialog for lobby ID: {steamID}, Valid: {steamID.IsValid()}");

        SteamFriends.ActivateGameOverlayInviteDialog(steamID);
    }


}


