using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public InputControls controls;
    public bool ctrl;
    public bool shift;
    public bool alt;
    public Vector2 mousePos;

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

        controls.Hold.Ctrl.performed += ctx => CtrlHold();
        controls.Hold.Shift.performed += ctx => ShiftHold();
        controls.Hold.Alt.performed += ctx => AltHold();
    }

    void Update()
    {
        mousePos = Input.mousePosition;
    }

    void CtrlHold() { ctrl = !ctrl; }
    void ShiftHold() { shift = !shift; }
    void AltHold() { alt = !alt; }

    void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }
}
