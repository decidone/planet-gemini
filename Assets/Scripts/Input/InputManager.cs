using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public InputControls controls;
    public bool ctrl;
    public bool shift;
    public bool alt;

    //Input State Control <- 따로 인풋 제어가 필요한 경우 해당 주석으로 표시
    public bool isMapOpened;    //여기로 옮기기
    public bool hoverInfo;

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

        isMapOpened = false;
        hoverInfo = true;

        controls.Hold.Ctrl.performed += ctx => CtrlHold();
        controls.Hold.Shift.performed += ctx => ShiftHold();
        controls.Hold.Alt.performed += ctx => AltHold();
    }

    void OnEnable() { controls.Enable(); }
    void OnDisable() { controls.Disable(); }

    void CtrlHold() { ctrl = !ctrl; }
    void ShiftHold() { shift = !shift; }
    void AltHold() { alt = !alt; }
}
