using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisconnectedPopup : MonoBehaviour
{
    [SerializeField] GameObject obj;
    [SerializeField] Button btn;
    [SerializeField] Text text;

    #region Singleton
    public static DisconnectedPopup instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    void Start()
    {
        btn.onClick.AddListener(BtnClicked);
    }

    public void OpenUI(string message)
    {
        GameManager.instance.CloseAllOpenedUI();
        text.text = message;
        obj.SetActive(true);
    }

    public void CloseUI()
    {
        obj.SetActive(false);
    }

    void BtnClicked()
    {
        SteamManager.instance.LeaveGame();
    }
}
