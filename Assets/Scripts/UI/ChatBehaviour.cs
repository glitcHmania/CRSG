using Mirror;
using Steamworks;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChatBehaviour : NetworkBehaviour
{
    public static ChatBehaviour Instance { get; private set; }

    private GameObject chatUI = null;
    private ScrollRect scrollRect = null;
    private TMP_InputField inputField = null;
    private TMP_Text chatText = null;
    private Graphic placeholderText = null;
    private Image scrollRectImage = null;
    private Image inputFieldImage = null;

    private Color scrollRectOpenColor = new Color(0, 0, 0, 0.4f);
    private Color scrollRectClosedColor;
    private Color inputFieldOpenColor = new Color(0, 0, 0, 0.7f);
    private Color inputFieldClosedColor;

    private static event Action<string> OnMessage;

    private bool isInputActive = false;
    public bool IsInputActive => isInputActive;

    private bool initialized = false;

    public override void OnStartAuthority()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (Instance == this) Instance = null;
        if (isOwned) OnMessage -= HandleMessage;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!isOwned || scene.name != "Game" || initialized) return;
        InitializeChat();
    }

    public void InitializeChat()
    {
        chatUI = GameObject.FindWithTag("ChatUI");
        if (chatUI == null)
        {
            Debug.LogWarning("ChatUI not found in the scene.");
            return;
        }

        inputField = chatUI.GetComponentInChildren<TMP_InputField>(true);
        scrollRect = chatUI.GetComponentInChildren<ScrollRect>(true);
        chatText = scrollRect.GetComponentInChildren<TMP_Text>();

        placeholderText = inputField.placeholder;
        scrollRectImage = scrollRect.GetComponent<Image>();
        inputFieldImage = inputField.GetComponent<Image>();

        scrollRectClosedColor = scrollRectImage.color;
        inputFieldClosedColor = inputFieldImage.color;

        chatUI.SetActive(true);
        inputField.interactable = false;

        OnMessage += HandleMessage;
        initialized = true;
    }

    private void Update()
    {
        if (!isOwned || !initialized) return;

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
                placeholderText.gameObject.SetActive(false);
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
                placeholderText.gameObject.SetActive(true);
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

        string playerName = SteamFriends.GetPersonaName();
        CmdSendMessage(playerName, message);
    }

    [Command]
    private void CmdSendMessage(string playerName, string message)
    {
        RpcHandleMessage($"[{playerName}]: {message}");
    }

    [ClientRpc]
    private void RpcHandleMessage(string message)
    {
        OnMessage?.Invoke(message);
    }

    private void HandleMessage(string message)
    {
        chatText.text += $"\n{message}";
    }
}
