using Mirror;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyMenuController : MonoBehaviour
{
    public void LeaveLobbyAndReturnToMainMenu()
    {
        if (SteamLobby.Instance != null && SteamLobby.Instance.CurrentLobbyID != 0)
        {
            SteamMatchmaking.LeaveLobby(new CSteamID(SteamLobby.Instance.CurrentLobbyID));
            SteamLobby.Instance = null;
        }
        var transport = NetworkManager.singleton.transport;

        if (transport != null)
        {
            transport.Shutdown();
            Destroy(transport.gameObject); // kills static clients like FizzySteamworks.client
        }

        if (NetworkClient.isConnected || NetworkClient.active)
            NetworkClient.Shutdown();

        if (NetworkServer.active)
            NetworkServer.Shutdown();

        if (NetworkManager.singleton != null)
            Destroy(NetworkManager.singleton.gameObject);

        var bootstrap = GameObject.FindObjectOfType<BootstrapLoader>();
        if (bootstrap != null)
            Destroy(bootstrap.gameObject);

        BootstrapLoader.ForceReset();
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


