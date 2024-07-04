using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            Debug.LogWarning("More than one instance of InputManager found!");
            return;
        }
        controls = new InputControls();
        instance = this;
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

        controls.Hold.Ctrl.performed += ctx => CtrlHold();
        controls.Hold.Shift.performed += ctx => ShiftHold();
        controls.Hold.Alt.performed += ctx => AltHold();
        controls.Hold.MouseLeft.performed += ctx => MouseLeftHold();
        controls.Hold.MouseRight.performed += ctx => MouseRightHold();

        EnableControls();
        controls.MapCamera.Disable();
    }

    void OnEnable() { controls.Enable(); }
    void OnDisable() { controls.Disable(); }

    void CtrlHold() { ctrl = !ctrl; }
    void ShiftHold() { shift = !shift; }
    void AltHold() { alt = !alt; }
    void MouseLeftHold() { mouseLeft = !mouseLeft; }
    void MouseRightHold() { mouseRight = !mouseRight; }

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
