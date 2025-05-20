using Mirror;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomNetworkManager : NetworkManager
{
    [SerializeField] private List<GameObject> characterPrefabs; // Indexed by characterId

    public List<PlayerObjectController> GamePlayers { get; } = new List<PlayerObjectController>();

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        if (SceneManager.GetActiveScene().name == "Lobby")
        {
            var customization = CustomizationManager.LoadCustomization();
            int characterId = Mathf.Clamp(customization.characterId, 0, characterPrefabs.Count - 1);

            GameObject selectedPrefab = characterPrefabs[characterId];
            PlayerObjectController gamePlayerInstance = Instantiate(selectedPrefab).GetComponent<PlayerObjectController>();

            gamePlayerInstance.ConnectionID = conn.connectionId;
            gamePlayerInstance.PlayerIdNumer = GamePlayers.Count + 1;
            gamePlayerInstance.PlayerSteamID = (ulong)SteamMatchmaking.GetLobbyMemberByIndex(
                (CSteamID)SteamLobby.Instance.CurrentLobbyID, GamePlayers.Count);

            gamePlayerInstance.CharacterId = customization.characterId;
            gamePlayerInstance.HatId = customization.hatId;
            gamePlayerInstance.BeltId = customization.beltId;

            NetworkServer.AddPlayerForConnection(conn, gamePlayerInstance.gameObject);
        }
    }


    public void StartGame(string sceneName)
    {
        ServerChangeScene(sceneName);
    }
}
