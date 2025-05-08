using Mirror;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChatBehaviour : NetworkBehaviour
{
    public static ChatBehaviour Instance { get; private set; }

    private GameObject chatUI = null;
    private ScrollRect scrollRect = null;
    private TMP_InputField inputField = null;

    private TMP_Text chatText = null;
    private Image scrollRectImage = null;
    private Image inputFieldImage = null;
    private Color scrollRectOpenColor = new Color(0, 0, 0, 0.4f);
    private Color scrollRectClosedColor = new Color(0, 0, 0, 0);
    private Color inputFieldOpenColor = new Color(0, 0, 0, 0.7f);
    private Color inputFieldClosedColor = new Color(0, 0, 0, 0);

    private static event Action<string> OnMessage;

    private bool isInputActive = false;
    public bool IsInputActive => isInputActive;

    public override void OnStartAuthority()
    {
        Instance = this;

        chatUI = GameObject.FindWithTag("ChatUI");
        inputField = chatUI.GetComponentInChildren<TMP_InputField>(true);
        scrollRect = chatUI.GetComponentInChildren<ScrollRect>(true);
        chatText = scrollRect.gameObject.GetComponentInChildren<TMP_Text>();

        scrollRectImage = scrollRect.gameObject.GetComponent<Image>();
        inputFieldImage = inputField.gameObject.GetComponent<Image>();

        chatUI.SetActive(true);
        OnMessage += HandleMessage;

        inputField.interactable = false;
    }

    [ClientCallback]
    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (!isOwned) return;
        OnMessage -= HandleMessage;
    }

    private void HandleMessage(string message)
    {
        chatText.text += $"\n{message}";
    }

    private void Update()
    {
        if (!isOwned) return;

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (!isInputActive)
            {
                scrollRectImage.color = scrollRectOpenColor;
                inputFieldImage.color = inputFieldOpenColor;

                isInputActive = true;
                inputField.interactable = true;
                inputField.ActivateInputField();
                inputField.Select();
                EventSystem.current.SetSelectedGameObject(inputField.gameObject);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                string message = inputField.text.Trim();
                if (!string.IsNullOrEmpty(message))
                {
                    Send(message);
                }

                isInputActive = false;
                inputField.text = string.Empty;
                inputField.DeactivateInputField();
                inputField.interactable = false;
                EventSystem.current.SetSelectedGameObject(null);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                scrollRectImage.color = scrollRectClosedColor;
                inputFieldImage.color = inputFieldClosedColor;
            }
        }
    }

    [Client]
    public void Send(string message)
    {
        if (!isOwned)
        {
            Debug.LogWarning("Tried to send a message from a non-owned object.");
            return;
        }

        CmdSendMessage(message);
    }

    [Command]
    private void CmdSendMessage(string message)
    {
        string playerName = connectionToClient.connectionId.ToString();
        RpcHandleMessage($"[Player {playerName}]: {message}");
    }

    [ClientRpc]
    private void RpcHandleMessage(string message)
    {
        OnMessage?.Invoke(message);
    }
}
