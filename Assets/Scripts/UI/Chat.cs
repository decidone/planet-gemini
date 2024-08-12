using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Chat : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI chat;
    [SerializeField] private TMP_InputField input;

    InputManager inputManager;
    public bool isChatOpened;

    #region Singleton
    public static Chat instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of Chat found!");
            return;
        }

        instance = this;
    }
    #endregion

    void Start()
    {
        isChatOpened = false;
    }
    void OnEnable()
    {
        inputManager = InputManager.instance;
        inputManager.controls.Chat.Enter.performed += Enter;
    }
    void OnDisable()
    {
        inputManager.controls.Chat.Enter.performed -= Enter;
    }
    void Update()
    {
        if (input.isFocused && input.gameObject == EventSystem.current.currentSelectedGameObject)
        {
            if (!isChatOpened)
            {
                inputManager.OpenChat();
                isChatOpened = true;
            }
        }
        else
        {
            if (isChatOpened)
            {
                inputManager.CloseChat();
                isChatOpened = false;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendMessageServerRpc(string message)
    {
        SendMessageClientRpc(message);
    }

    [ClientRpc]
    public void SendMessageClientRpc(string message)
    {
        chat.text += "\n" + message;
    }

    public void Submit()
    {
        string userInput = input.text;
        if (!string.IsNullOrWhiteSpace(userInput))
        {
            userInput = SteamManager.instance.userName + ": " + input.text;
            SendMessageServerRpc(userInput.Trim());
        }

        input.text = string.Empty;
    }

    public void Enter(InputAction.CallbackContext ctx)
    {
        if (input.gameObject != EventSystem.current.currentSelectedGameObject)
        {
            input.Select();
        }
        else
        {
            Submit();
        }
    }
}
