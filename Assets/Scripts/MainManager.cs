using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainManager : MonoBehaviour
{
    [SerializeField]
    GameObject mainBtns;
    [SerializeField]
    Button hostBtn;
    [SerializeField]
    Button joinBtn;
    [SerializeField]
    Button settingsBtn;
    [SerializeField]
    Button quitBtn;

    [SerializeField]
    GameObject hostBtns;
    [SerializeField]
    Button newGameBtn;
    [SerializeField]
    Button loadBtn;
    [SerializeField]
    Button backBtn;

    [SerializeField]
    MainPanelsManager panelsManager;

    // Start is called before the first frame update
    void Start()
    {
        hostBtn.onClick.AddListener(() => HostBtnFunc());
        joinBtn.onClick.AddListener(() => JoinBtnFunc());
        settingsBtn.onClick.AddListener(() => SettingsBtnFunc());
        quitBtn.onClick.AddListener(() => QuitBtnFunc());
        newGameBtn.onClick.AddListener(() => NewGameBtnFunc());
        loadBtn.onClick.AddListener(() => LoadBtnFunc());
        backBtn.onClick.AddListener(() => BackBtnFunc());


    }

    void HostBtnFunc()
    {
        mainBtns.SetActive(false);
        hostBtns.SetActive(true);
    }

    void JoinBtnFunc()
    {
        SteamManager.instance.GetLobbiesList();
        //LoadingSceneManager.LoadScene("LobbyScene");
    }

    void SettingsBtnFunc()
    {

    }

    void QuitBtnFunc()
    {

    }

    void NewGameBtnFunc()
    {
        panelsManager.NewGamePanelSet(true);
    }

    void LoadBtnFunc()
    {
        panelsManager.SaveLoadPanelSet();
    }

    void BackBtnFunc()
    {
        mainBtns.SetActive(true);
        hostBtns.SetActive(false);
    }
}
