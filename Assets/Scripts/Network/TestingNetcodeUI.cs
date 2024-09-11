using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TestingNetcodeUI : MonoBehaviour
{
    [SerializeField]
    private Button startHostButton;
    [SerializeField]
    private Button startClientButton;

    void Start()
    {
        startHostButton.onClick.AddListener(() =>
        {
            Debug.Log("Host");
            MainGameSetting.instance.NewGameState(true);
            NetworkManager.Singleton.StartHost();
            LoadingUICtrl.Instance.LoadScene("GameScene");
            Hide();
        });
        startClientButton.onClick.AddListener(() =>
        {
            Debug.Log("Client");
            NetworkManager.Singleton.StartClient();
            LoadingUICtrl.Instance.LoadScene("GameScene");
            Hide();
        });
    }

    void Hide()
    {
        gameObject.SetActive(false);
    }
}
