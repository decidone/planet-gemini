using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainPanelsManager : MonoBehaviour
{
    MainGameSetting gameSetting;

    [SerializeField]
    GameObject newGamePanel;
    [SerializeField]
    Dropdown mapSizeDropdown;
    [SerializeField]
    Dropdown difficultyLevelDropdown;
    [SerializeField]
    Button gameStartBtn;
    [SerializeField]
    Button backBtn;

    SaveLoadMenu saveLoadPanel;

    // Start is called before the first frame update
    void Start()
    {
        gameSetting = MainGameSetting.instance;
        saveLoadPanel = SaveLoadMenu.instance;
        gameStartBtn.onClick.AddListener(() => GameStartBtnFunc());
        backBtn.onClick.AddListener(() => NewGamePanelSet(false));
        mapSizeDropdown.onValueChanged.AddListener(delegate { MapSizeDropdownFunc(mapSizeDropdown); });
        difficultyLevelDropdown.onValueChanged.AddListener(delegate { DifficultyLevelDropdownFunc(difficultyLevelDropdown); });
    }

    public void NewGamePanelSet(bool state)
    {
        newGamePanel.SetActive(state);
        if (state)
            MainManager.instance.OpenedUISet(gameObject);
        else
            MainManager.instance.ClosedUISet();
    }

    void GameStartBtnFunc()
    {
        gameSetting.NewGameState(true);
        NetworkManager.Singleton.StartHost();
        LoadingUICtrl.Instance.LoadScene("GameScene", true);
        //SteamManager.instance.HostLobby();
        //LoadingSceneManager.LoadScene("LobbyScene");
    }

    void MapSizeDropdownFunc(Dropdown dropdown)
    {
        gameSetting.MapSizeSet(dropdown.value);
    }

    void DifficultyLevelDropdownFunc(Dropdown dropdown)
    {
        gameSetting.DifficultylevelSet(dropdown.value);
    }

    public void SaveLoadPanelSet()
    {
        saveLoadPanel.MenuOpen(false);
    }
}
