using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class LoadingUICtrl : MonoBehaviour
{
    protected static LoadingUICtrl instance;

    public static LoadingUICtrl Instance
    {
        get
        {
            if (instance == null)
            {
                var obj = FindObjectOfType<LoadingUICtrl>();
                if (obj != null)
                {
                    instance = obj;
                }
                else
                {
                    instance = Create();
                }
            }
            return instance;
        }

        private set
        {
            instance = value;
        }
    }

    private string loadSceneName;

    public static LoadingUICtrl Create()
    {
        var SceneLoaderPrefab = Resources.Load<LoadingUICtrl>("LoadingUI");
        return Instantiate(SceneLoaderPrefab);
    }

    private void Awake()
    {
        if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    public void LoadScene(string sceneName)
    {
        gameObject.SetActive(true);
        SceneManager.sceneLoaded += LoadSceneEnd;
        loadSceneName = sceneName;
        if (loadSceneName == "GameScene")
        {
            GameManager.GenerationComplete += HandleGenerationComplete;
            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, 0);
        }
        else if (loadSceneName == "MainMenuScene")
        {
            SceneManager.LoadScene("MainMenuScene");
        }
    }

    private void LoadSceneEnd(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.name == loadSceneName)
        {
            if (loadSceneName != "GameScene")
            {
                gameObject.SetActive(false);
                SceneManager.sceneLoaded -= LoadSceneEnd;
            }
        }
    }

    private void HandleGenerationComplete()
    {
        gameObject.SetActive(false);
        SceneManager.sceneLoaded -= LoadSceneEnd;
        GameManager.GenerationComplete -= HandleGenerationComplete;
        Debug.Log("HandleGenerationComplete");
    }
}