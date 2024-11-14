using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BasicUIBtns : MonoBehaviour
{
    [SerializeField]
    GameObject playerBtns;
    [SerializeField]
    GameObject unitBtns;
    [SerializeField]
    Button swapBtn;
    bool isPlayerBtnOn;
    [HideInInspector]
    public bool mouseOnBtn;
    public bool isSwapBtn;
    ItemInfoWindow itemInfoWindow;
    InputManager inputManager;
    [SerializeField]
    BUIBtn[] btns;

    #region Singleton
    public static BasicUIBtns instance;

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

    // Start is called before the first frame update
    void Start()
    {
        itemInfoWindow = GameManager.instance.inventoryUiCanvas.GetComponent<ItemInfoWindow>();
        SwapFunc(true);
        KeyValueSet();
        swapBtn.onClick.AddListener(() => SwapBtn());
    }

    void OnEnable()
    {
        inputManager = InputManager.instance;
        inputManager.controls.HotKey.BasicUIBtnsSwap.performed += SwapBtn;
    }

    void OnDisable()
    {
        inputManager.controls.HotKey.BasicUIBtnsSwap.performed -= SwapBtn;
    }

    public void KeyValueSet()
    {
        Dictionary<string, InputAction> actions = SettingsMenu.instance.inputActions;
        foreach (BUIBtn btn in btns)
        {
            if (!btn.isStickyKey)
            {
                foreach (var data in actions)
                {
                    if (data.Key == btn.OptionName)
                    {
                        var playerInvenAction = data.Value.bindings[0].effectivePath;
                        string key = InputControlPath.ToHumanReadableString(playerInvenAction, InputControlPath.HumanReadableStringOptions.OmitDevice);
                        btn.KeyValueSet(key);
                        break;
                    }
                }
            }
            else
                continue;
        }
    }

    void SwapBtnFunc()
    {
        if (playerBtns.activeSelf)
        {
            SwapFunc(false);
        }
        else
        {
            SwapFunc(true);
        }
    }

    void SwapBtn(InputAction.CallbackContext ctx)
    {
        SwapBtn();
    }

    void SwapBtn()
    {
        SwapBtnFunc();
        if (mouseOnBtn && isSwapBtn)
        {
            itemInfoWindow.CloseWindow();
        }
    }

    public void UnitCtrlSwapBtn()
    {
        if (isPlayerBtnOn)
        {
            SwapFunc(false);
        }
    }

    public void SwapFunc(bool playerBtnOn)
    {
        isPlayerBtnOn = playerBtnOn;

        playerBtns.SetActive(isPlayerBtnOn);
        unitBtns.SetActive(!isPlayerBtnOn);
    }

    public void OnExit()
    {
        mouseOnBtn = false;
        isSwapBtn = false;
        itemInfoWindow.CloseWindow();
    }
}
