using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class KeyBindingsBtn : MonoBehaviour
{
    public string setKey;
    [SerializeField]
    Text inputText;
    [SerializeField]
    Text nameText;
    [HideInInspector]
    public InputAction inputAction;
    [SerializeField]
    private Text bindingDisplayNameText = null;

    private InputActionRebindingExtensions.RebindingOperation rebindingOperation;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => BtnFunc());
    }

    public void BtnSetting(InputAction action, string name, string keyValue)
    {
        inputAction = action;
        setKey = keyValue;
        nameText.text = name;
        inputText.text = keyValue;
    }

    public void BtnFunc()
    {
        Debug.Log("Starting rebinding operation...");
        inputAction.Disable();
        rebindingOperation = inputAction.PerformInteractiveRebinding()
            .WithControlsExcluding("Mouse")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(operation => {
                Debug.Log("Rebinding complete.");
                RebindComplete();
            })
            .Start();
    }

    private void RebindComplete()
    {
        // Get the index of the binding for the first control of the action
        int bindingIndex = inputAction.GetBindingIndexForControl(inputAction.controls[0]);

        // Update the display name text with the new binding
        inputText.text = InputControlPath.ToHumanReadableString(
            inputAction.bindings[bindingIndex].effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice);

        // Dispose of the rebinding operation to free up resources
        inputAction.Enable();

        rebindingOperation.Dispose();
    }
}
