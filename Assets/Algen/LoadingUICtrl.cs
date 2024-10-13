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
    bool isTimerOn;
    float timer;
    float timeoutLimit;
    [SerializeField] float sceneLoadTimeoutLimit;
    [SerializeField] float gameSetTimeoutLimit;
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

        Animator[] animator = GetComponentsInChildren<Animator>();
        foreach (Animator ani in animator)
        {
            ani.updateMode = AnimatorUpdateMode.UnscaledTime;
        }
    }

    private void Update()
    {
        if (isTimerOn)
            timer += Time.deltaTime;
        if (timer > timeoutLimit)
        {
            timer = 0;
            isTimerOn = false;
            Debug.Log("timeout, limit: " + timeoutLimit);
        }
    }

    public void LoadScene(string sceneName, bool isHost)
    {
        timer = 0;
        timeoutLimit = sceneLoadTimeoutLimit;
        isTimerOn = true;

        gameObject.SetActive(true);
        SceneManager.sceneLoaded += LoadSceneEnd;
        loadSceneName = sceneName;
        if (loadSceneName == "GameScene")
        {
            GameManager.GenerationComplete += HandleGenerationComplete;

            if (isHost)
            {
                NetworkManager.Singleton.SceneManager.LoadScene(sceneName, 0);
            }
            else
            {
                SceneManager.LoadScene(sceneName, 0);
            }
        }
        else if (loadSceneName == "MainMenuScene")
        {
            SceneManager.LoadScene("MainMenuScene");
        }
    }

    public void LoadScene(string sceneName)
    {
        timer = 0;
        timeoutLimit = sceneLoadTimeoutLimit;
        isTimerOn = true;

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
        Debug.Log("LoadSceneEnd");
        if (scene.name == loadSceneName)
        {
            //SteamManager.instance.ClientConnectSend();

            if (loadSceneName != "GameScene")
            {
                timer = 0;
                isTimerOn = false;
                gameObject.SetActive(false);
                SceneManager.sceneLoaded -= LoadSceneEnd;
            }
            else
            {
                timer = 0;
                timeoutLimit = gameSetTimeoutLimit;
            }
        }
    }

    private void HandleGenerationComplete()
    {
        timer = 0;
        isTimerOn = false;

        gameObject.SetActive(false);
        SceneManager.sceneLoaded -= LoadSceneEnd;
        GameManager.GenerationComplete -= HandleGenerationComplete;
        Debug.Log("HandleGenerationComplete");
    }
}