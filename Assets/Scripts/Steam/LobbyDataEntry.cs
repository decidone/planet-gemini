using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Steamworks.Data;

public class LobbyDataEntry : MonoBehaviour
{
    [SerializeField] Text lobbyNameText;
    [SerializeField] Text lobbyUserCount;
    [SerializeField] Button joinBtn;

    public Lobby lobby;
    
    public void SetLobbyData(Lobby _lobby)
    {
        lobby = _lobby;
        if (lobby.GetData("owner") != string.Empty)
            lobbyNameText.text = lobby.GetData("owner") + "' Game";
        else
            lobbyNameText.text = "failed to get the lobby data";
        lobbyUserCount.text = lobby.MemberCount + " / " + lobby.MaxMembers;

        joinBtn.onClick.AddListener(() => SteamManager.instance.JoinLobby(lobby));
    }
}
