using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks.Data;

public class LobbySaver : MonoBehaviour
{
    public Lobby? currentLobby;

    #region Singleton
    public static LobbySaver instance;

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
}
