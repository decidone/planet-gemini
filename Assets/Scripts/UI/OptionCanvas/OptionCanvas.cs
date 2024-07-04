using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionCanvas : MonoBehaviour
{
    [SerializeField]
    Button SettingsBtn;
    [SerializeField]
    Button SaveBtn;
    [SerializeField]
    Button LoadBtn;

    // Start is called before the first frame update
    void Start()
    {
        SettingsBtn.onClick.AddListener(() => SettingsBtnFunc());
        SaveBtn.onClick.AddListener(() => SaveBtnFunc());
        LoadBtn.onClick.AddListener(() => LoadBtnFunc());
    }

    void SettingsBtnFunc()
    {
        SettingsMenu.instance.MenuOpen();
    }

    void SaveBtnFunc()
    {
        SaveLoadMenu.instance.MenuOpen(true);
    }

    void LoadBtnFunc()
    {
        SaveLoadMenu.instance.MenuOpen(false);
    }
}
