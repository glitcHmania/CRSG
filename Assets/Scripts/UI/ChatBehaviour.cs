using Mirror;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ChatBehaviour : NetworkBehaviour
{
    [SerializeField] private GameObject chatUI = null;
    [SerializeField] private TMP_Text chatText = null;
    [SerializeField] private TMP_InputField chatInput = null;

    private static event Action<string> OnMessage;

    private bool isInputActive = false;

    public override void OnStartAuthority()
    {
        chatUI = GameObject.FindWithTag("ChatUI");
        chatText = chatUI.GetComponentInChildren<TMP_Text>();
        chatInput = chatUI.GetComponentInChildren<TMP_InputField>();

        chatUI.SetActive(true);
        OnMessage += HandleMessage;

        // Disable input field until activated
        chatInput.interactable = false;
    }

    [ClientCallback]
    private void OnDestroy()
    {
        if (!isOwned) return;
        OnMessage -= HandleMessage;
    }

    private void HandleMessage(string message)
    {
        chatText.text += $"\n{message}";
    }

    private void Update()
    {
        if (!isOwned || !chatUI.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (!isInputActive)
            {
                // Activate the input field and focus it
                isInputActive = true;
                chatInput.interactable = true;
                chatInput.ActivateInputField();
                chatInput.Select();
                EventSystem.current.SetSelectedGameObject(chatInput.gameObject);

            }
            else
            {
                // Try sending the message
                string message = chatInput.text.Trim();
                if (!string.IsNullOrEmpty(message))
                {
                    Send(message);
                }

                // Deactivate input
                isInputActive = false;
                chatInput.text = string.Empty;
                chatInput.DeactivateInputField();
                chatInput.interactable = false;
                EventSystem.current.SetSelectedGameObject(null);
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
        //string playerName = SteamFriends.GetPersonaName();
        string playerName = connectionToClient.connectionId.ToString();
        RpcHandleMessage($"[Player {playerName}]: {message}");
    }

    [ClientRpc]
    private void RpcHandleMessage(string message)
    {
        OnMessage?.Invoke(message);
    }
}