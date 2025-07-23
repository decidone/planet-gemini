using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks.Data;

public class LobbiesListManager : MonoBehaviour
{
    public GameObject lobbiesList;
    public GameObject lobbyDataItemPrefab;
    public GameObject lobbyListContent;
    SoundManager soundManager;
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
        Debug.Log("lobbie awake");
    }
    #endregion

    public void OpenUI()
    {
        lobbiesList.SetActive(true);
        MainManager.instance.OpenedUISet(gameObject);
    }

    public void CloseUI()
    {
        Debug.Log("CloseUI");
        lobbiesList.SetActive(false);
        MainManager.instance.ClosedUISet();
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
}
