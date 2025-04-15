using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ChatManager : MonoBehaviour
{
    [SerializeField] private GameObject chatUI;
    [SerializeField] private TMP_Text chatText;
    [SerializeField] private TMP_InputField chatInput;

    private static ChatManager instance;
    private ChatBehaviour localChatPlayer;
    private bool isInputActive = false;

    private void Awake()
    {
        instance = this;
        chatUI.SetActive(true);
        chatInput.interactable = false;
    }

    private void Update()
    {
        if (localChatPlayer == null) return;

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (!isInputActive)
            {
                ActivateInput();
            }
            else
            {
                string message = chatInput.text.Trim();
                if (!string.IsNullOrEmpty(message))
                {
                    localChatPlayer.Send(message);
                }

                DeactivateInput();
            }
        }
    }

    private void ActivateInput()
    {
        isInputActive = true;
        chatInput.interactable = true;
        chatInput.text = string.Empty;
        chatInput.ActivateInputField();
        chatInput.Select();
        EventSystem.current.SetSelectedGameObject(chatInput.gameObject);
    }

    private void DeactivateInput()
    {
        isInputActive = false;
        chatInput.DeactivateInputField();
        chatInput.interactable = false;
        EventSystem.current.SetSelectedGameObject(null);
    }

    public static void RegisterLocalPlayer(ChatBehaviour player)
    {
        if (instance != null)
        {
            instance.localChatPlayer = player;
        }
    }

    public static void DisplayMessage(string message)
    {
        if (instance != null)
        {
            instance.chatText.text += $"\n{message}";
        }
    }
}
