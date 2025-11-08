using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks.Data;
using UnityEngine.UI;

public class LobbiesListManager : MonoBehaviour
{
    public GameObject lobbiesList;
    public GameObject lobbyDataItemPrefab;
    public GameObject lobbyListContent;
    SoundManager soundManager;

    public InputField lobbyIdInputField;
    public Button joinBtn;
    //public GameObject lobbiesButton, hostButton;

    public List<GameObject> listOfLobbies = new List<GameObject>();

    #region Singleton
    public static LobbiesListManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        soundManager = SoundManager.instance;
        Debug.Log("lobbies awake");
    }
    #endregion

    private void Start()
    {
        joinBtn.onClick.AddListener(() => JoinWithLobbyID());
    }

    public void OpenUI()
    {
        lobbiesList.SetActive(true);
        MainManager.instance.OpenedUISet(gameObject);
    }

    public void CloseUI()
    {
        lobbiesList.SetActive(false);
        MainManager.instance.ClosedUISet();
        if(soundManager)
            soundManager.PlayUISFX("ButtonClick");
    }

    public void DisplayLobby(Lobby lobby)
    {
        GameObject createdItem = Instantiate(lobbyDataItemPrefab);
        bool hasData = createdItem.GetComponent<LobbyDataEntry>().SetLobbyData(lobby);
        if (hasData)
        {
            createdItem.transform.SetParent(lobbyListContent.transform);
            createdItem.transform.localScale = Vector3.one;

            listOfLobbies.Add(createdItem);
        }
        else
        {
            Destroy(createdItem);
        }
    }

    public void DestroyLobbies()
    {
        if (listOfLobbies.Count == 0) return;

        foreach (GameObject lobbyItem in listOfLobbies)
        {
            Destroy(lobbyItem);
        }
        listOfLobbies.Clear();
    }

    public void JoinWithLobbyID()
    {
        ulong Id;
        if (!ulong.TryParse(lobbyIdInputField.text, out Id))
            return;

        SteamManager.instance.JoinLobbyWithID(Id);
    }
}
