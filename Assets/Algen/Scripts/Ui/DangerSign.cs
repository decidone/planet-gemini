using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DangerSign : MonoBehaviour
{
    [SerializeField]
    GameObject signObj;
    Image signImg;

    Camera mainCamera;

    Vector3 targetPos;

    bool isWaveOn = false;

    GameManager gameManager;
    GameObject player;

    #region Singleton
    public static DangerSign instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of GameManager found!");
            return;
        }

        instance = this;
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        signImg = signObj.GetComponent<Image>();
        signImg.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        //if (isWaveOn)
        //{
        //    ImgMove();
        //}
    }

    public void WaveStart(Vector3 _targetPos)
    {
        gameManager = GameManager.instance;
        player = gameManager.player;
        targetPos = _targetPos;
        signImg.gameObject.transform.position = mainCamera.WorldToViewportPoint(targetPos);
        signImg.enabled = true;
        isWaveOn = true;
    }

    public void WaveEnd()
    {
        signImg.enabled = false;
        isWaveOn = false;
    }

    public void ImgMove()
    {
        if (CheckObjectIsInCamera(targetPos))
        {
            signImg.gameObject.transform.position = mainCamera.WorldToViewportPoint(targetPos);
        }
        else
        {
            signImg.gameObject.transform.position = mainCamera.WorldToViewportPoint(player.transform.position) - mainCamera.WorldToViewportPoint(targetPos);
        }
    }

    public bool CheckObjectIsInCamera(Vector3 _targetPos)
    {
        Vector3 screenPoint = mainCamera.WorldToViewportPoint(_targetPos);
        bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;

        return onScreen;
    }
}
