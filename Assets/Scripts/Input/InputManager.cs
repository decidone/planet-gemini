using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public InputControls controls;
    public bool ctrl;
    public bool shift;
    public bool alt;
    public bool mouseLeft;
    public bool mouseRight;

    //Input State Control <- 따로 인풋 제어가 필요한 경우 해당 주석으로 표시
    public bool isMapOpened;

    #region Singleton
    public static InputManager instance;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        instance = this;
        controls = new InputControls();
    }
    #endregion

    void Start()
    {
        ctrl = false;
        shift = false;
        alt = false;
        mouseLeft = false;
        mouseRight = false;

        isMapOpened = false;

        EnableControls();
        controls.MapCamera.Disable();
    }

    void OnEnable()
    {
        controls.Enable();
        controls.Hold.Ctrl.performed += CtrlHold;
        controls.Hold.Shift.performed += ShiftHold;
        controls.Hold.Alt.performed += AltHold;
        controls.Hold.MouseLeft.performed += MouseLeftHold;
        controls.Hold.MouseRight.performed += MouseRightHold;
    }
    void OnDisable()
    {
        if(controls != null)
        {
            controls.Disable();
            controls.Hold.Ctrl.performed -= CtrlHold;
            controls.Hold.Shift.performed -= ShiftHold;
            controls.Hold.Alt.performed -= AltHold;
            controls.Hold.MouseLeft.performed -= MouseLeftHold;
            controls.Hold.MouseRight.performed -= MouseRightHold;
        }
    }

    void CtrlHold(InputAction.CallbackContext ctx)
    {
        ctrl = !ctrl;
        if (GameManager.instance != null)
        {
            if (!UpgradeRemoveBtn.instance.clickBtn)
            {
                if (ctrl)
                    MouseSkin.instance.DragCursorSet(false);
                else
                    MouseSkin.instance.ResetCursor();
            }
        }
    }

    void ShiftHold(InputAction.CallbackContext ctx)
    {
        shift = !shift;
        if (GameManager.instance != null)
        {
            if (!UpgradeRemoveBtn.instance.clickBtn)
            {
                if (shift)
                    MouseSkin.instance.DragCursorSet(true);
                else
                    MouseSkin.instance.ResetCursor();
            }
        }
    }

    void AltHold(InputAction.CallbackContext ctx) { alt = !alt; }
    void MouseLeftHold(InputAction.CallbackContext ctx) { mouseLeft = !mouseLeft; }
    void MouseRightHold(InputAction.CallbackContext ctx) { mouseRight = !mouseRight; }

    void EnableControls()
    {
        controls.MainCamera.Enable();
        controls.MapCamera.Enable();
        controls.Building.Enable();
        controls.Player.Enable();
        controls.Unit.Enable();
        controls.Structure.Enable();
        controls.Inventory.Enable();
        controls.HotKey.Enable();
        controls.State.Enable();
    }

    void DisableControls()
    {
        controls.MainCamera.Disable();
        controls.MapCamera.Disable();
        controls.Building.Disable();
        controls.Player.Disable();
        controls.Unit.Disable();
        controls.Structure.Disable();
        controls.Inventory.Disable();
        controls.HotKey.Disable();
        controls.State.Disable();
    }

    public void OpenMap()
    {
        isMapOpened = true;
        DisableControls();

        controls.State.Enable();
        controls.MapCamera.Enable();
        controls.HotKey.Escape.Enable();
    }

    public void CloseMap()
    {
        isMapOpened = false;
        controls.State.Disable();
        controls.HotKey.Escape.Disable();

        EnableControls();
        controls.MapCamera.Disable();
    }

    public void InMarket()
    {
        DisableControls();

        controls.MainCamera.Enable();
        controls.Player.Enable();
        controls.Inventory.Enable();
        controls.HotKey.Enable();
        controls.Hold.Enable();
        controls.Chat.Enable();
    }

    public void OutMarket()
    {
        controls.MainCamera.Disable();
        controls.Player.Disable();
        controls.Inventory.Disable();
        controls.HotKey.Disable();
        //controls.Hold.Disable();
        //controls.Chat.Disable();

        EnableControls();
        controls.MapCamera.Disable();
    }

    public void OpenChat()
    {
        DisableControls();
    }

    public void CloseChat()
    {
        EnableControls();
    }

    public void OpenConsole()
    {
        DisableControls();
        controls.Chat.Disable();
    }

    public void CloseConsole()
    {
        EnableControls();
        controls.Chat.Enable();
    }
}
