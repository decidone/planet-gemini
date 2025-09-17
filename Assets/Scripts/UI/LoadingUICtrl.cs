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
    public RectTransform target;
    public float forwardAngle = 18f;    // 앞으로 돌리는 각도
    public float reboundAngle = -3f;    // 반동 각도
    public float speed = 720f;          // 회전 속도 (도/초)
    public float pauseTime = 0.1f;      // 멈추는 시간

    private Coroutine rotateRoutine;

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
        else
            return;

        if (timer > timeoutLimit)
        {
            timer = 0;
            isTimerOn = false;
            Debug.Log("timeout, limit: " + timeoutLimit);

            if (DisconnectedPopup.instance != null)
            {
                DisconnectedPopup.instance.OpenUI("Connection Timeout.");
            }
        }
    }

    private void OnEnable()
    {
        rotateRoutine = StartCoroutine(RotateLoop());
    }

    private void OnDisable()
    {
        if (rotateRoutine != null)
            StopCoroutine(rotateRoutine);
    }

    public void LoadScene(string sceneName, bool isHost)
    {
        MainGameSetting.instance.StartStopwatch();
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
        MainGameSetting.instance.StartStopwatch();
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

    private IEnumerator RotateLoop()
    {
        while (true)
        {
            // 1. 앞으로 18도
            yield return RotateBy(forwardAngle, speed);

            // 2. 반동 -3도
            yield return RotateBy(reboundAngle, speed * 1.5f);

            // 3. 잠깐 멈춤
            yield return new WaitForSeconds(pauseTime);
        }
    }

    private IEnumerator RotateBy(float delta, float spd)
    {
        Quaternion startRot = target.localRotation;
        Quaternion endRot = startRot * Quaternion.Euler(0, 0, delta);

        while (Quaternion.Angle(target.localRotation, endRot) > 0.1f)
        {
            target.localRotation = Quaternion.RotateTowards(
                target.localRotation,
                endRot,
                spd * Time.deltaTime
            );
            yield return null;
        }
        target.localRotation = endRot;
    }
}