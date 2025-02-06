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
            Destroy(gameObject);
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
                isChatOpened = true;
                if (!GameManager.instance.isGameOver && !GameManager.instance.isWaitingForRespawn)
                    inputManager.OpenChat();
            }
        }
        else
        {
            if (isChatOpened)
            {
                isChatOpened = false;
                if (!GameManager.instance.isGameOver && !GameManager.instance.isWaitingForRespawn)
                    inputManager.CloseChat();
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
            if (userInput[0] == '/')
            {
                // '/'로 시작하는 경우 명령어 체크
                string message = "";
                switch (userInput)
                {
                    case "/debug":
                        GameManager.instance.DebugMode();
                        break;
                    case "/seed":
                        message = "seed: " + MapGenerator.instance.seed;
                        break;
                    case "/respawn":
                        GameManager.instance.SetRespawnUI();
                        break;
                    case "/gameover":
                        GameManager.instance.SetGameOverUI();
                        break;
                    case "/test":
                        NetworkObjManager.instance.InitConnectors();
                        break;
                }

                if (message != "")
                    chat.text += "\n" + message;
                input.text = string.Empty;
                return;
            }

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
