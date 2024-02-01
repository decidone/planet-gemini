using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TestingNetcodeUI : NetworkBehaviour
{
    [SerializeField]
    private Button startHostButton;
    [SerializeField]
    private Button startClientButton;

    void Awake()
    {
        startHostButton.onClick.AddListener(() =>
        {
            Debug.Log("Host");
            NetworkManager.Singleton.StartHost();
            GameManager.instance.HostConnected();
            Hide();
        });
        startClientButton.onClick.AddListener(() =>
        {
            Debug.Log("Client");
            NetworkManager.Singleton.StartClient();
            GameManager.instance.ClientConnected();
            Hide();
        });
    }

    void Hide() {
        gameObject.SetActive(false);
    }
}
