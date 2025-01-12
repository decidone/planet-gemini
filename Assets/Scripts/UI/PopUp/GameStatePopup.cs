using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameStatePopup : MonoBehaviour
{
    [SerializeField] GameObject obj;
    [SerializeField] Text stateText;
    [SerializeField] Button btn;
    [SerializeField] Text btnText;

    #region Singleton
    public static GameStatePopup instance;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }
    #endregion

    public void OpenUI()
    {
        obj.SetActive(true);
        btn.onClick.RemoveAllListeners();
    }

    public void CloseUI()
    {
        obj.SetActive(false);
    }

    public void SetRespawnUI()
    {
        OpenUI();
        InputManager.instance.WaitingRespawn();
        stateText.text = "Waiting for respawn";
        btn.interactable = false;
        //btnText.text = "Respawn";
        StartCoroutine(RespawnCount());
    }

    public IEnumerator RespawnCount()
    {
        int count = 5;
        while (true)
        {
            SetBtnCount(count);
            if (count == 0)
                yield break;
            yield return new WaitForSeconds(1f);
            count--;
        }
    }

    public void SetBtnCount(int count)
    {
        if (count > 0)
        {
            btnText.text = count + "";
        }
        else
        {
            btn.interactable = true;
            btn.onClick.AddListener(RespawnBtn);
            btnText.text = "Respawn";
        }
    }

    void RespawnBtn()
    {
        CloseUI();
        InputManager.instance.Respawn();
        GameManager.instance.PlayerRespawn();
    }

    public void SetGameOverUI()
    {
        OpenUI();
        InputManager.instance.CommonDisableControls();
        stateText.text = "Game Over";
        btn.onClick.AddListener(GameOverBtn);
        btnText.text = "Quit";
    }

    void GameOverBtn()
    {
        CloseUI();
        InputManager.instance.CommonEnableControls();
        GameManager.instance.GameOver();
    }
}
