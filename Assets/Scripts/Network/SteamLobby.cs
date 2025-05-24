using Mirror;
using Steamworks;
using UnityEngine;

public class SteamLobby : MonoBehaviour
{
    public static SteamLobby Instance { get; set; }

    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> joinRequested;
    protected Callback<LobbyEnter_t> lobbyEnter;

    public ulong CurrentLobbyID;

    private const string HostAddressKey = "HostAddress";
    private CustomNetworkManager manager;

    private CustomNetworkManager Manager
    {
        get
        {
            if (manager == null)
                manager = FindObjectOfType<CustomNetworkManager>();
            return manager;
        }
    }

    private void Awake()
    {
        Debug.Log("SteamLobby.Awake() called.");
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogError("Steamworks not initialized");
            return;
        }

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        joinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequested);
        lobbyEnter = Callback<LobbyEnter_t>.Create(OnLobbyEnter);
    }

    public void HostLobby()
    {
        Debug.Log("Creating lobby");

        if (Manager == null)
        {
            Debug.LogError("SteamLobby: Manager is null.");
            return;
        }

        if (NetworkClient.active || NetworkServer.active)
        {
            Debug.LogWarning("Transport still active, shutting down...");
            NetworkClient.Shutdown();
            NetworkServer.Shutdown();
        }

        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, Manager.maxConnections);
    }


    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            Debug.LogError("Failed to create lobby");
            return;
        }

        if (NetworkClient.active || NetworkServer.active)
        {
            Debug.LogWarning("Mirror is still marked active — forcing cleanup before starting host.");
            NetworkClient.Shutdown();
            NetworkServer.Shutdown();
        }

        Manager.StartHost();

        SteamMatchmaking.SetLobbyData(
            new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey, SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(
            new CSteamID(callback.m_ulSteamIDLobby), "name", SteamFriends.GetPersonaName() + "'s Lobby");
    }

    private void OnJoinRequested(GameLobbyJoinRequested_t callback)
    {
        Debug.Log("Join requested");
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEnter(LobbyEnter_t callback)
    {
        CurrentLobbyID = callback.m_ulSteamIDLobby;

        if (NetworkServer.active) return;

        Manager.networkAddress = SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), HostAddressKey);
        Manager.StartClient();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
