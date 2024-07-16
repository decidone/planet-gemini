using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmPanel : MonoBehaviour
{
    [SerializeField]
    GameObject confirmPanel;
    [SerializeField]
    Text contentText;
    [SerializeField]
    Button OkBtn;
    [SerializeField]
    Button CanelBtn;
    [SerializeField]
    GameObject inputObj;
    [SerializeField]
    InputField inputField;
    SaveLoadBtn saveLoadBtn;

    #region Singleton
    public static ConfirmPanel instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of DataManager found!");
            return;
        }

        instance = this;
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        OkBtn.onClick.AddListener(() => OkBtnFunc());
        CanelBtn.onClick.AddListener(() => CanelBtnFunc());
    }

    public void CallConfirm(SaveLoadBtn btn, bool saveLoadState, int slotNum)
    {
        OkBtn.gameObject.SetActive(true);
        CanelBtn.gameObject.SetActive(true);
        confirmPanel.SetActive(true);
        saveLoadBtn = btn;
        if (saveLoadState)
        {
            contentText.text = "Do you want to save to slot " + slotNum + "?";
            inputObj.SetActive(true);
        }
        else
        {
            contentText.text = "Do you want to load to slot " + slotNum + "?";
        }
        GameManager.instance.onUIChangedCallback?.Invoke(confirmPanel);
    }

    public void KeyBindingCallConfirm()
    {
        OkBtn.gameObject.SetActive(false);
        CanelBtn.gameObject.SetActive(false);
        confirmPanel.SetActive(true);
        contentText.text = "Waiting For Input";
    }

    void OkBtnFunc()
    {
        if(saveLoadBtn)
        {
            saveLoadBtn.BtnConfirm(true, inputField.text);
        }
        UIClose();
    }

    void CanelBtnFunc()
    {
        if (saveLoadBtn)
        {
            saveLoadBtn.BtnConfirm(false, inputField.text);
        }
        UIClose();
    }

    public void UIClose()
    {
        confirmPanel.SetActive(false);
        inputObj.SetActive(false);
        GameManager.instance.onUIChangedCallback?.Invoke(confirmPanel);
    }
}
