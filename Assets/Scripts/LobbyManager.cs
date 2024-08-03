using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : NetworkBehaviour
{    
    [SerializeField]
    Button HostBtn;
    [SerializeField]
    Button ClientBtn;

    // Start is called before the first frame update
    void Start()
    {
        HostBtn.onClick.AddListener(() => HostBtnFunc());
        ClientBtn.onClick.AddListener(() => LoadGameBtnFunc());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void HostBtnFunc()
    {
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene("MergeScene_09", 0);
    }

    void LoadGameBtnFunc()
    {
        NetworkManager.Singleton.StartClient();
        NetworkManager.Singleton.SceneManager.LoadScene("MergeScene_09", 0);
    }
}
