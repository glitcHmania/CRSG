using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI References")]
    public Canvas MainCanvas;
    public Canvas TutorialCanvas;
    public GameObject BulletUI;
    public TextMeshProUGUI ReloadUI;

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
        BulletUI.SetActive(false); // Disable bullet UI by default
        ReloadUI.enabled = false; // Disable reload UI by default
    }
}
