using Steamworks;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyController : MonoBehaviour
{
    public static LobbyController Instance;
    public TextMeshProUGUI LobbyNameText;
    public GameObject PlayerListViewContent;
    public GameObject PlayerListItemPrefab;
    public GameObject LocalPlayerObject;
    public ulong CurrentLobbyID;
    public bool PlayerItemCreated = false;
    public PlayerObjectController LocalPlayerController;
    public Button StartGameButton;
    public TextMeshProUGUI ReadyButtonText;

    private List<PlayerListItem> playerListItems = new List<PlayerListItem>();
    private CustomNetworkManager manager;
    private CustomNetworkManager Manager
    {
        get
        {
            if (manager != null)
            {
                return manager;
            }

            return manager = CustomNetworkManager.singleton as CustomNetworkManager;
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void ReadyPlayer()
    {
        LocalPlayerController.ChangeReady();
    }

    public void UpdateButton()
    {
        if(LocalPlayerController.Ready)
        {
            ReadyButtonText.text = "unready";
        }
        else
        {
            ReadyButtonText.text = "ready";
        }
    }

    public void CheckIfAllReady()
    {
        bool allReady = false;

        foreach( PlayerObjectController player in Manager.GamePlayers)
        {
            if (player.Ready)
            {
                allReady = true;
            }
            else
            {
                allReady = false;
                break;
            }
        }

        if(allReady)
        {
            if(LocalPlayerController?.PlayerIdNumer == 1)
            {
                StartGameButton.interactable = true;
            }
            else
            {
                StartGameButton.interactable = false;
            }
        }
        else
        {
            StartGameButton.interactable = false;
        }
    }

    public void UpdateLobbyName()
    {
        CurrentLobbyID = Manager.GetComponent<SteamLobby>().CurrentLobbyID;
        LobbyNameText.text = SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), "name");
    }

    public void UpdatePlayerList()
    {
        if (!PlayerItemCreated)
        {
            CreateHostPlayerItem();
        }

        if (playerListItems.Count < Manager.GamePlayers.Count)
        {
            CreateClientPlayerItem();
        }

        if (playerListItems.Count > Manager.GamePlayers.Count)
        {
            RemovePlayerItem();
        }

        if (playerListItems.Count == Manager.GamePlayers.Count)
        {
            UpdatePlayerItem();
        }
    }

    public void FindLocalPlayer()
    {
        LocalPlayerObject = GameObject.Find("LocalGamePlayer");
        LocalPlayerController = LocalPlayerObject.GetComponent<PlayerObjectController>();
    }

    public void CreateHostPlayerItem()
    {
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            GameObject newPlayerItem = Instantiate(PlayerListItemPrefab) as GameObject;
            PlayerListItem newPlayerItemScript = newPlayerItem.GetComponent<PlayerListItem>();

            newPlayerItemScript.PlayerName = player.PlayerName;
            newPlayerItemScript.ConectionID = player.ConnectionID;
            newPlayerItemScript.PlayerSteamID = player.PlayerSteamID;
            newPlayerItemScript.Ready = player.Ready;

            newPlayerItemScript.SetPlayerValues();
            newPlayerItem.transform.SetParent(PlayerListViewContent.transform);
            newPlayerItem.transform.localScale = Vector3.one;

            playerListItems.Add(newPlayerItemScript);
        }
        PlayerItemCreated = true;
    }

    public void CreateClientPlayerItem()
    {
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            if (!playerListItems.Any(x => x.ConectionID == player.ConnectionID))
            {
                GameObject newPlayerItem = Instantiate(PlayerListItemPrefab) as GameObject;
                PlayerListItem newPlayerItemScript = newPlayerItem.GetComponent<PlayerListItem>();

                newPlayerItemScript.PlayerName = player.PlayerName;
                newPlayerItemScript.ConectionID = player.ConnectionID;
                newPlayerItemScript.PlayerSteamID = player.PlayerSteamID;
                newPlayerItemScript.Ready = player.Ready;

                newPlayerItemScript.SetPlayerValues();
                newPlayerItem.transform.SetParent(PlayerListViewContent.transform);
                newPlayerItem.transform.localScale = Vector3.one;

                playerListItems.Add(newPlayerItemScript);
            }
        }
    }

    public void UpdatePlayerItem()
    {
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            foreach (PlayerListItem playerListItemScript in playerListItems)
            {
                if (playerListItemScript.ConectionID == player.ConnectionID)
                {
                    playerListItemScript.PlayerName = player.PlayerName;
                    playerListItemScript.Ready = player.Ready;
                    playerListItemScript.SetPlayerValues();

                    if(player == LocalPlayerController)
                    {
                        UpdateButton();
                    }
                }
            }

            CheckIfAllReady();
        }
    }

    public void RemovePlayerItem()
    {
        // List to hold items we want to remove
        List<PlayerListItem> playerListItemsToRemove = new List<PlayerListItem>();

        foreach (PlayerListItem playerListItem in playerListItems)
        {
            if (playerListItem == null || playerListItem.gameObject == null)
            {
                // Already destroyed, safe to remove
                playerListItemsToRemove.Add(playerListItem);
                continue;
            }

            if (!Manager.GamePlayers.Any(x => x.ConnectionID == playerListItem.ConectionID))
            {
                playerListItemsToRemove.Add(playerListItem);
            }
        }

        // Now safely remove and destroy them
        foreach (PlayerListItem itemToRemove in playerListItemsToRemove)
        {
            if (itemToRemove != null && itemToRemove.gameObject != null)
            {
                Destroy(itemToRemove.gameObject);
            }

            playerListItems.Remove(itemToRemove);
        }
    }


    public void StartGame(string sceneName)
    {
        LocalPlayerController.CanStartGame(sceneName);
    }
}
