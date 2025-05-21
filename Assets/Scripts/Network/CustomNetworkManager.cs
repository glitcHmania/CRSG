using Mirror;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;

public class CustomNetworkManager : NetworkManager
{
    public struct CustomAddPlayerMessage : NetworkMessage
    {
        public int characterId;
        public int hatId;
        public int beltId;
    }

    [SerializeField] private List<GameObject> characterPrefabs; // Indexed by characterId
    public List<PlayerObjectController> GamePlayers { get; } = new List<PlayerObjectController>();

    private static CustomNetworkManager _instance;

    public override void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("Duplicate CustomNetworkManager found. Destroying the new one.");
            Destroy(this.gameObject);  // Prevent duplicates
            return;
        }

        _instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        NetworkServer.RegisterHandler<CustomAddPlayerMessage>(OnServerAddCustomPlayer);
    }

    private void OnServerAddCustomPlayer(NetworkConnectionToClient conn, CustomAddPlayerMessage msg)
    {
        int characterId = Mathf.Clamp(msg.characterId, 0, characterPrefabs.Count - 1);
        GameObject prefab = characterPrefabs[characterId];

        GameObject playerInstance = Instantiate(prefab);
        var controller = playerInstance.GetComponent<PlayerObjectController>();

        controller.ConnectionID = conn.connectionId;
        controller.PlayerIdNumer = GamePlayers.Count + 1;
        controller.PlayerSteamID = (ulong)SteamMatchmaking.GetLobbyMemberByIndex(
            (CSteamID)SteamLobby.Instance.CurrentLobbyID, GamePlayers.Count);

        controller.CharacterId = msg.characterId;
        controller.HatId = msg.hatId;
        controller.BeltId = msg.beltId;

        NetworkServer.AddPlayerForConnection(conn, playerInstance);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        NetworkClient.OnConnectedEvent += OnClientConnected;
    }

    private void OnClientConnected()
    {
        var customization = CustomizationManager.LoadCustomization();

        CustomAddPlayerMessage msg = new CustomAddPlayerMessage
        {
            characterId = customization.characterId,
            hatId = customization.hatId,
            beltId = customization.beltId
        };

        NetworkClient.Send(msg);
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {

    }

    public void StartGame(string sceneName)
    {
        ServerChangeScene(sceneName);
    }
}
