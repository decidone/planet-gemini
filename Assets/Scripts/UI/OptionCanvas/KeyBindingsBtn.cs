using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.IO;

public class KeyBindingsBtn : MonoBehaviour
{
    public string setKey;
    public string currentKey;
    public string keyName;
    [SerializeField]
    Text inputText;
    [SerializeField]
    Text nameText;
    [HideInInspector]
    public InputAction inputAction;
    [SerializeField]
    private Text bindingDisplayNameText = null;
    bool keybindingFalse;

    private InputActionRebindingExtensions.RebindingOperation rebindingOperation;

    // Start is called before the first frame update
    void Start()
    {
        if (setKey != "F")
            GetComponent<Button>().onClick.AddListener(() => BtnFunc());
        else
            GetComponent<Image>().color = Color.gray;
    }

    private void SaveRebindings()
    {
        if (inputAction == null)
        {
            return;
        }

        // 리바인딩 데이터를 JSON 형식으로 저장
        string rebinds = inputAction.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString(keyName, rebinds);
        PlayerPrefs.Save();
        Debug.Log("Rebindings saved.");
    }

    public void BtnSetting(InputAction action, string name, string keyValue)
    {
        inputAction = action;
        setKey = keyValue;
        keyName = name;
        nameText.text = keyName;
        inputText.text = keyValue;
    }

    public void BtnFunc()
    {
        Debug.Log("Starting rebinding operation...");
        inputAction.Disable();

        ConfirmPanel.instance.KeyBindingCallConfirm();
        if (keybindingFalse)
            ConfirmPanel.instance.KeyBindingDuplication();

        rebindingOperation = inputAction.PerformInteractiveRebinding()
            .WithControlsExcluding("Mouse")
            .WithCancelingThrough("<Keyboard>/escape")
            .OnPotentialMatch(operation =>
            {
                int bindingIndex = inputAction.GetBindingIndexForControl(inputAction.controls[0]);
                if (IsControlAlreadyBound(operation.selectedControl, inputAction, bindingIndex))
                {
                    Debug.Log("Control is already bound to another action.");
                    rebindingOperation.Cancel();
                    inputAction.Enable();
                    keybindingFalse = true;
                    BtnFunc();
                }
            })
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(operation =>
            {
                Debug.Log("Rebinding complete.");
                RebindComplete();
            })
            .OnCancel(operation =>
            {
                inputAction.Enable();
                rebindingOperation.Dispose();
                ConfirmPanel.instance.UIClose();
            })
            .Start();

        keybindingFalse = false;
    }

    private bool IsControlAlreadyBound(InputControl control, InputAction action, int bindingIndex)
    {
        InputBinding newBinding = action.bindings[bindingIndex];
        string newBindingPath = InputControlPath.ToHumanReadableString(control.path, InputControlPath.HumanReadableStringOptions.OmitDevice);
        Debug.Log(newBindingPath.ToUpper());

        foreach (var actionMap in InputManager.instance.controls.asset.actionMaps)
        {
            foreach (var binding in actionMap.bindings)
            {
                if (binding.action == newBinding.action)
                    continue;
                string bindingPath = InputControlPath.ToHumanReadableString(binding.effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);
                Debug.Log(bindingPath);

                if (newBindingPath.ToUpper() == bindingPath)
                {

                    return true;
                }
            }
        }

        return false;
    }

    private void RebindComplete()
    {
        InputTextSet();

        // Dispose of the rebinding operation to free up resources
        inputAction.Enable();

        rebindingOperation.Dispose();
        ConfirmPanel.instance.UIClose();

        SaveRebindings();
    }

    public void InputTextSet()
    {
        // Get the index of the binding for the first control of the action
        int bindingIndex = inputAction.GetBindingIndexForControl(inputAction.controls[0]);

        // Update the display name text with the new binding
        currentKey = InputControlPath.ToHumanReadableString(
            inputAction.bindings[bindingIndex].effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice);

        inputText.text = currentKey;
    }

    public void ResetToDefault()
    {
        int bindingIndex = inputAction.GetBindingIndexForControl(inputAction.controls[0]);

        if (inputAction.bindings[bindingIndex].isComposite)
        {
            for (int i = 0; i < inputAction.bindings.Count && inputAction.bindings[i].isComposite; i++)
            {
                inputAction.RemoveBindingOverride(i);
            }
        }
        else
            inputAction.RemoveBindingOverride(bindingIndex);

        InputTextSet();
        SaveRebindings();
    }
}
