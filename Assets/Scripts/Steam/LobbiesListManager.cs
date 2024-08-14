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
            Debug.LogWarning("More than one instance of LobbiesListManager found!");
            return;
        }

        instance = this;
    }
    #endregion

    public void OpenUI()
    {
        lobbiesList.SetActive(true);
        if (listOfLobbies.Count > 0)
            DestroyLobbies();
        MainManager.instance.OpenedUISet(gameObject);
    }

    public void CloseUI()
    {
        lobbiesList.SetActive(false);
        MainManager.instance.ClosedUISet();
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
        foreach (GameObject lobbyItem in listOfLobbies)
        {
            Destroy(lobbyItem);
        }
        listOfLobbies.Clear();
    }
}
