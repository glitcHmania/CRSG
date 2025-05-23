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

    //quit game
    public void QuitGame()
    {
        Application.Quit();
    }
}
