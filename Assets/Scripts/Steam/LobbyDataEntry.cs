using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Steamworks.Data;
using Steamworks;

public class LobbyDataEntry : MonoBehaviour
{
    [SerializeField] Text lobbyNameText;
    [SerializeField] Text lobbyUserCount;
    [SerializeField] Button joinBtn;

    public Lobby lobby;
    public float cooldownTime = 1f;
    public bool isFriendLobby = false;

    public bool SetLobbyData(Lobby _lobby)
    {
        bool state;
        lobby = _lobby;
        if (lobby.GetData("ownerName") != string.Empty)
        {
            lobbyNameText.text = lobby.GetData("ownerName") + "' Game";
            state = true;
        }
        else
        {
            lobbyNameText.text = "failed to get the lobby data";
            state = false;
        }
        lobbyUserCount.text = lobby.MemberCount + " / " + lobby.MaxMembers;

        string accessLevel = lobby.GetData("access");
        string owner = lobby.GetData("owner");
        if (owner != string.Empty)
        {
            state = false;
            if (accessLevel == "1")
            {
                var friends = SteamFriends.GetFriends();
                foreach (var friend in friends)
                {
                    if (friend.Id.ToString() == owner)
                    {
                        isFriendLobby = true;
                        lobbyNameText.color = UnityEngine.Color.green;
                        state = true;
                        break;
                    }
                }
            }
        }

        joinBtn.onClick.AddListener(() => SteamManager.instance.JoinLobby(lobby));
        joinBtn.onClick.AddListener(() => StartCoroutine(ButtonCooldownRoutine()));
        return state;
    }

    private IEnumerator ButtonCooldownRoutine()
    {
        joinBtn.interactable = false; // 비활성화
        yield return new WaitForSeconds(cooldownTime);
        joinBtn.interactable = true;  // 다시 활성화
    }
}
