using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Steamworks.Data;

public class LobbyDataEntry : MonoBehaviour
{
    [SerializeField] Text lobbyNameText;
    [SerializeField] Button joinBtn;

    public Lobby lobby;
    public ulong lobbyId;
    public string lobbyName;
    
    public void SetLobbyData(Lobby _lobby)
    {
        lobby = _lobby;
        lobbyId = lobby.Id;
        lobbyName = lobby.Owner.Name;
        lobbyNameText.text = lobbyId.ToString();

        joinBtn.onClick.AddListener(() => SteamManager.instance.JoinLobby(lobby));
    }
}
