using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

// UTF-8 설정
public class MapCameraController : MonoBehaviour
{
    [SerializeField]
    Transform target;
    [SerializeField]
    Vector3 offset;
    [SerializeField]
    GameObject CameraObj;

    PixelPerfectCamera pixelPerfectCamera;
    GameManager gameManager;
    int zoomLevel;

    void Awake()
    {
        zoomLevel = 1;
        pixelPerfectCamera = CameraObj.GetComponent<PixelPerfectCamera>();
    }

    void Start()
    {
        gameManager = GameManager.instance;
    }

    void Update()
    {
        InputCheck();
    }

    void InputCheck()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (!gameManager.isMapOpened)
                OpenUI();
            else
                CloseUI();
        }

        if (gameManager.isMapOpened)
        {
            float scrollWheelInput = Input.GetAxis("Mouse ScrollWheel");
            if (scrollWheelInput != 0)
            {
                zoomLevel += Mathf.RoundToInt(scrollWheelInput * 10);
                zoomLevel = Mathf.Clamp(zoomLevel, 1, 7);
                pixelPerfectCamera.refResolutionX = Mathf.FloorToInt(Screen.width / zoomLevel);
                pixelPerfectCamera.refResolutionY = Mathf.FloorToInt(Screen.height / zoomLevel);
            }
        }
    }

    void OpenUI()
    {
        gameManager.isMapOpened = true;
        transform.position = target.position - offset;
        CameraObj.SetActive(true);
    }

    void CloseUI()
    {
        gameManager.isMapOpened = false;
        CameraObj.SetActive(false);
    }
}
