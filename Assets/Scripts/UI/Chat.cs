using System;
using System.Collections;
using System.Collections.Generic;
using QFSW.QC.Actions;
using QFSW.QC.UI;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Chat : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI chat;
    [SerializeField] private TMP_InputField input;
    [SerializeField] RectTransform panel;
    [SerializeField] GameObject controls;
    [SerializeField] GameObject resizeAnchor;
    [SerializeField] GameObject scrollHandle;
    [SerializeField] Image background;
    [SerializeField] Button configBtn;
    [SerializeField] Button timestampBtn;
    [SerializeField] TextMeshProUGUI configText;
    [SerializeField] ZoomUIController zoom;
    float defaultChatX = 1014;  // 기본 채팅창 사이즈
    float defaultChatY = 240;

    List<(DateTime, string)> chatLog = new List<(DateTime, string)>();
    InputManager inputManager;
    public bool isChatOpened;
    bool isConfigMode;
    bool isDrag;
    bool isTimestampOn;

    bool isChatDisplay;
    float chatDisplayTime = 15f;
    float chatDisplayTimer;

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
        chat.enabled = false;
        isChatOpened = false;
        configBtn.onClick.AddListener(Config);
        timestampBtn.onClick.AddListener(TimestampBtnClicked);
        float panelSizeX = PlayerPrefs.GetFloat("ChatX", defaultChatX);
        float panelSizeY = PlayerPrefs.GetFloat("ChatY", defaultChatY);
        zoom.SetZoom(PlayerPrefs.GetFloat("Zoom", 1));
        isTimestampOn = Convert.ToBoolean(PlayerPrefs.GetInt("Timestamp"));
        if (isTimestampOn)
            timestampBtn.GetComponent<Image>().color = new Color(255, 255, 255, 1f);
        else
            timestampBtn.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
        panel.sizeDelta = new Vector2(panelSizeX, panelSizeY);
        SetNormalMode();
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
        if (isChatDisplay)
        {
            if (!isChatOpened && !isConfigMode)
                chatDisplayTimer += Time.deltaTime;
            if (chatDisplayTimer > chatDisplayTime)
            {
                HideChat();
            }
        }

        if (GameManager.instance.popUpUIOpen)
            return;

        if (isDrag) return;

        if (input.isFocused && input.gameObject == EventSystem.current.currentSelectedGameObject)
        {
            if (!isChatOpened)
            {
                isChatOpened = true;
                DisplayChat();
                if (!isConfigMode)
                {
                    background.enabled = true;
                    scrollHandle.SetActive(true);
                }

                if (!GameManager.instance.isGameOver && !GameManager.instance.isWaitingForRespawn)
                    inputManager.OpenChat();
            }
        }
        else
        {
            if (isChatOpened)
            {
                isChatOpened = false;
                if (!isConfigMode)
                {
                    background.enabled = false;
                    scrollHandle.SetActive(false);
                }

                if (!GameManager.instance.isGameOver && !GameManager.instance.isWaitingForRespawn)
                    inputManager.CloseChat();
            }
        }
    }

    void DisplayChat()
    {
        isChatDisplay = true;
        chat.enabled = true;
        chatDisplayTimer = 0;
    }

    void HideChat()
    {
        isChatDisplay = false;
        chat.enabled = false;
    }

    public void Enter(InputAction.CallbackContext ctx)
    {
        if (GameManager.instance.popUpUIOpen)
            return;

        if (input.gameObject != EventSystem.current.currentSelectedGameObject)
        {
            input.Select();
        }
        else
        {
            Submit();
        }
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
                    case "/energy regroup":
                        NetworkObjManager.instance.InitConnectors();
                        break;
                    case "/wave":
                        GameManager.instance.WaveForcedOperation();
                        break;
                    case "/test":
                        QuestManager.instance.QuestCompCheck(50);
                        break;
                    case "/overall":
                        Overall.instance.OverallConsumption(ItemList.instance.itemDic["Coal"], 12000);
                        break;
                }

                if (message != "")
                {
                    chatLog.Add((DateTime.MinValue, message));
                    chat.text += "\n" + message;
                }
                input.text = string.Empty;
                return;
            }

            userInput = SteamManager.instance.userName + ": " + input.text;
            SendMessageServerRpc(userInput.Trim());
        }

        input.text = string.Empty;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendMessageServerRpc(string message)
    {
        SendMessageClientRpc(message);
    }

    [ClientRpc]
    public void SendMessageClientRpc(string message)
    {
        DisplayChat();
        chatLog.Add((DateTime.Now, message));
        if (isTimestampOn)
            chat.text += "\n" + DateTime.Now.ToString("[HH:mm:ss] ") + message;
        else
            chat.text += "\n" + message;
    }

    void Config()
    {
        if (isConfigMode)
        {
            SetNormalMode();
        }
        else
        {
            SetConfigMode();
            DisplayChat();
        }
    }

    public void DragStart()
    {
        //Chat프리펩 Scrollbar에서 사용함
        isDrag = true;
    }

    public void DragEnd()
    {
        //Chat프리펩 Scrollbar에서 사용함
        isDrag = false;
        input.Select();
    }

    public void TimestampBtnClicked()
    {
        Image image = timestampBtn.GetComponent<Image>();

        if (isTimestampOn)
        {
            isTimestampOn = false;
            image.color = new Color(255, 255, 255, 0.5f);
        }
        else
        {
            isTimestampOn = true;
            image.color = new Color(255, 255, 255, 1f);
        }

        RefrestChat();
    }

    void RefrestChat()
    {
        chat.text = string.Empty;

        if (isTimestampOn)
        {
            foreach (var chatData in chatLog)
            {
                if (chatData.Item1 != DateTime.MinValue)
                    chat.text += "\n" + chatData.Item1.ToString("[HH:mm:ss] ") + chatData.Item2;
                else
                    chat.text += "\n" + chatData.Item2;
            }
        }
        else
        {
            foreach (var chatData in chatLog)
            {
                chat.text += "\n" + chatData.Item2;
            }
        }
    }

    void SetNormalMode()
    {
        isConfigMode = false;
        controls.SetActive(false);
        timestampBtn.gameObject.SetActive(false);
        resizeAnchor.SetActive(false);
        background.enabled = false;
        scrollHandle.SetActive(false);
        configText.text = "Config";

        // 채팅창 사이즈 저장(화면 해상도보다 크게 설정했을 경우 문제가 생기므로 조정)
        if (panel.sizeDelta.x < CameraController.instance.width)
            PlayerPrefs.SetFloat("ChatX", panel.sizeDelta.x);
        else
            PlayerPrefs.SetFloat("ChatX", CameraController.instance.width - 100);

        if (panel.sizeDelta.y < CameraController.instance.height)
            PlayerPrefs.SetFloat("ChatY", panel.sizeDelta.y);
        else
            PlayerPrefs.SetFloat("ChatY", CameraController.instance.height - 100);

        PlayerPrefs.SetFloat("Zoom", zoom.GetZoom());
        PlayerPrefs.SetInt("Timestamp", System.Convert.ToInt16(isTimestampOn));
    }

    void SetConfigMode()
    {
        isConfigMode = true;
        controls.SetActive(true);
        timestampBtn.gameObject.SetActive(true);
        resizeAnchor.SetActive(true);
        background.enabled = true;
        scrollHandle.SetActive(true);
        configText.text = "Done";
    }
}
