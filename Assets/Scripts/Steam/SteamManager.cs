using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SteamManager : MonoBehaviour
{
    public string userName;
    //[SerializeField] GameObject MainMenu;
    //[SerializeField] GameObject InLobbyMenu;

    #region Singleton
    public static SteamManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
    #endregion

    private void OnEnable()
    {
        SteamMatchmaking.OnLobbyCreated += LobbyCreated;
        SteamMatchmaking.OnLobbyEntered += LobbyEntered;
        SteamMatchmaking.OnLobbyMemberDisconnected += LobbyMemberLeft;
        SteamMatchmaking.OnLobbyMemberLeave += LobbyMemberLeft;
        SteamFriends.OnGameLobbyJoinRequested += GameLobbyJoinRequested;
    }

    private void OnDisable()
    {
        SteamMatchmaking.OnLobbyCreated -= LobbyCreated;
        SteamMatchmaking.OnLobbyEntered -= LobbyEntered;
        SteamFriends.OnGameLobbyJoinRequested -= GameLobbyJoinRequested;
    }

    private void LobbyCreated(Result result, Lobby lobby)
    {
        if (result == Result.OK)
        {
            lobby.SetPublic();
            lobby.SetJoinable(true);
            lobby.SetData("owner", lobby.Owner.Name);
            NetworkManager.Singleton.StartHost();
        }
        else
        {
            Debug.Log("Creat Lobby Error");
        }
    }

    private void LobbyEntered(Lobby lobby)
    {
        LobbySaver.instance.currentLobby = lobby;
        userName = SteamClient.Name;
        Debug.Log("Entered");

        if (!NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.gameObject.GetComponent<FacepunchTransport>().targetSteamId = lobby.Owner.Id;
            NetworkManager.Singleton.StartClient();
            LoadingUICtrl.Instance.LoadScene("GameScene");
        }
        else
        {
            LoadingUICtrl.Instance.LoadScene("GameScene");
        }
    }

    private async void GameLobbyJoinRequested(Lobby lobby, SteamId SteamId)
    {
        await lobby.Join();
    }

    public async void HostLobby()
    {
        await SteamMatchmaking.CreateLobbyAsync(2);
    }

    //public async void JoinLobbyWithID()
    //{
    //    ulong Id;
    //    if (!ulong.TryParse(LobbyIdInputField.text, out Id))
    //        return;

    //    Lobby[] lobbies = await SteamMatchmaking.LobbyList.WithSlotsAvailable(1).RequestAsync();

    //    foreach (Lobby lobby in lobbies)
    //    {
    //        if (lobby.Id == Id)
    //        {
    //            await lobby.Join();
    //            return;
    //        }
    //    }
    //}

    public async void JoinLobby(Lobby _lobby)
    {
        Lobby[] lobbies = await SteamMatchmaking.LobbyList.WithSlotsAvailable(1).RequestAsync();

        foreach (Lobby lobby in lobbies)
        {
            if (lobby.Id == _lobby.Id)
            {
                await lobby.Join();
                return;
            }
        }
    }

    public void LeaveLobby()
    {
        LobbySaver.instance.currentLobby?.Leave();
        LobbySaver.instance.currentLobby = null;
    }

    private void LobbyMemberLeft(Lobby lobby, Friend friend)
    {
        if (!GameManager.instance.isHost)
        {
            Debug.Log("Host left");
            if (DisconnectedPopup.instance != null)
            {
                DisconnectedPopup.instance.OpenUI();
            }
            else
            {
                LeaveGame();
            }
        }
        //Debug.Log(lobby.Owner.Id);
        //Debug.Log(friend.Id);
    }

    public void LeaveGame()
    {
        SteamManager.instance.LeaveLobby();
        NetworkManager.Singleton.Shutdown();
        Destroy(NetworkManager.Singleton.gameObject);
        GameManager.instance.DestroyAllDontDestroyOnLoadObjects();
        SceneManager.LoadScene("MainMenuScene");
    }

    public async void GetLobbiesList()
    {
        Lobby[] lobbies = await SteamMatchmaking.LobbyList.RequestAsync();
        LobbiesListManager.instance.OpenUI();
        LobbiesListManager.instance.DestroyLobbies();

        if (lobbies != null)
        {
            foreach (Lobby lobby in lobbies)
            {
                Debug.Log(lobby.Id);
                LobbiesListManager.instance.DisplayLobby(lobby);
            }
        }
    }
}
