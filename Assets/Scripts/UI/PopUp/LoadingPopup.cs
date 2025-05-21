using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingPopup : MonoBehaviour
{
    [SerializeField] GameObject LoadingPanel;
    [SerializeField] Text text;
    string message;
    bool isOpened;
    string[] dots = new string[] { ".", "..", "..." };

    #region Singleton
    public static LoadingPopup instance;

    void Awake()
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

    public void OpenUI(string _message)
    {
        LoadingPanel.SetActive(true);
        isOpened = true;
        message = _message;

        StartCoroutine(LoadingMessage());
    }

    public void CloseUI()
    {
        LoadingPanel.SetActive(false);
        isOpened = false;
        message = string.Empty;
    }

    IEnumerator LoadingMessage()
    {
        float time = 1f;
        int i = 0;
        while (isOpened)
        {
            i = i % 3;
            text.text = message + dots[i];
            i++;
            yield return new WaitForSecondsRealtime(time);
        }
    }
}
