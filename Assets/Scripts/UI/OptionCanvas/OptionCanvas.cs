using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionCanvas : MonoBehaviour
{
    [SerializeField]
    Button SettingsBtn;
    [SerializeField]
    Button SaveBtn;
    [SerializeField]
    Button LoadBtn;
    [SerializeField]
    Button quitBtn;

    public GameObject mainPanel;

    #region Singleton
    public static OptionCanvas instance;

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
        DontDestroyOnLoad(gameObject);

        SettingsBtn.onClick.AddListener(() => SettingsBtnFunc());
        SaveBtn.onClick.AddListener(() => SaveBtnFunc());
        LoadBtn.onClick.AddListener(() => LoadBtnFunc());
        quitBtn.onClick.AddListener(() => QuitBtnFunc());
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

    void QuitBtnFunc()
    {
        LoadingSceneManager.LoadScene("MainMenuScene");
        //SceneManager.LoadScene("LobbyScene");
    }
}
