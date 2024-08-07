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
    Button gameStartBtn;
    [SerializeField]
    Button backBtn;

    [SerializeField]
    SaveLoadMenu saveLoadPanel;

    // Start is called before the first frame update
    void Start()
    {
        gameSetting = MainGameSetting.instance;
        gameStartBtn.onClick.AddListener(() => GameStartBtnFunc());
        backBtn.onClick.AddListener(() => NewGamePanelSet(false));
        mapSizeDropdown.onValueChanged.AddListener(delegate { MapSizeDropdownFunc(mapSizeDropdown); });
    }

    public void NewGamePanelSet(bool state)
    {
        newGamePanel.SetActive(state);
    }

    void GameStartBtnFunc()
    {
        gameSetting.NewGameState(true);
        SteamManager.instance.HostLobby();
        //LoadingSceneManager.LoadScene("LobbyScene");
    }

    void MapSizeDropdownFunc(Dropdown dropdown)
    {
        MapSizeSet(dropdown.value);
    }

    void MapSizeSet(int dropdownIndex)
    {
        gameSetting.MapSizeSet(dropdownIndex);
    }

    public void SaveLoadPanelSet()
    {
        saveLoadPanel.MenuOpen(false);
    }
}
