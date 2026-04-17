using Steamworks;
using Steamworks.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static SteamFriendLobbyFetcher;

public class LobbyDataEntry : MonoBehaviour
{
    [SerializeField] Text lobbyNameText;
    [SerializeField] RawImage profileImage;
    [SerializeField] Button joinBtn;

    public Lobby lobby;
    public float cooldownTime = 1f;

    public void SetLobbyData(Lobby _lobby, FriendProfile profile)
    {
        lobby = _lobby;
        lobbyNameText.text = profile.name;
        profileImage.texture = profile.avatar;
        profileImage.uvRect = new Rect(0, 1, 1, -1);

        joinBtn.onClick.AddListener(() => SteamManager.instance.JoinLobby(lobby));
        joinBtn.onClick.AddListener(() => StartCoroutine(ButtonCooldownRoutine()));
    }

    private IEnumerator ButtonCooldownRoutine()
    {
        joinBtn.interactable = false; // 비활성화
        yield return new WaitForSeconds(cooldownTime);
        joinBtn.interactable = true;  // 다시 활성화
    }
}
