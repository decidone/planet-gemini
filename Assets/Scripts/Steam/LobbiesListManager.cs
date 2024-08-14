using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks.Data;

public class LobbiesListManager : MonoBehaviour
{
    public GameObject lobbiesList;
    public GameObject lobbyDataItemPrefab;
    public GameObject lobbyListContent;

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
    }
    #endregion

    public void OpenUI()
    {
        lobbiesList.SetActive(true);
    }

    public void CloseUI()
    {
        lobbiesList.SetActive(false);
    }

    public void DisplayLobby(Lobby lobby)
    {
        GameObject createdItem = Instantiate(lobbyDataItemPrefab);
        createdItem.GetComponent<LobbyDataEntry>().SetLobbyData(lobby);
        createdItem.transform.SetParent(lobbyListContent.transform);
        createdItem.transform.localScale = Vector3.one;

        listOfLobbies.Add(createdItem);
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
