using Mirror;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI References")]
    public Canvas MainCanvas;
    public Canvas TutorialCanvas;
    public Image EscMenu;
    public TextMeshProUGUI BulletUI;
    public TextMeshProUGUI ReloadUI;

    private bool escMenuActive = false;
    public bool EscMenuActive => escMenuActive;
    public bool IsGameFocused => !escMenuActive && !ChatBehaviour.Instance.IsInputActive;

    //event for esc menu activation and deactivation
    public delegate void EscMenuToggle(bool isActive);
    public event EscMenuToggle OnEscMenuToggle;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        MainCanvas.GetComponent<Canvas>().enabled = true;

        // Initialize UI elements
        BulletUI.enabled = false; // Disable bullet UI by default
        ReloadUI.enabled = false; // Disable reload UI by default
    }

    private void Update()
    {
        // Check for Escape key press to toggle the menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleEscMenu();
        }
    }

    private void ToggleEscMenu()
    {
        if (EscMenu.gameObject.activeSelf)
        {
            EscMenu.gameObject.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            escMenuActive = false;

            OnEscMenuToggle?.Invoke(false);
        }
        else
        {
            EscMenu.gameObject.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            escMenuActive = true;

            OnEscMenuToggle?.Invoke(true);
        }
    }

    public void LeaveGameAndReturnToMainMenu()
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


    //quit game
    public void QuitGame()
    {
        Application.Quit();
    }
}
