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
    InputManager inputManager;

    int zoomLevel;
    float scrollWheelInput;

    void Awake()
    {
        zoomLevel = 1;
        pixelPerfectCamera = CameraObj.GetComponent<PixelPerfectCamera>();
    }

    void Start()
    {
        gameManager = GameManager.instance;
        inputManager = InputManager.instance;
        inputManager.controls.MapCamera.ToggleMap.performed += ctx => ToggleMap();
    }

    void ToggleMap()
    {
        if (!gameManager.isMapOpened)
            OpenUI();
        else
            CloseUI();
    }

    void Update()
    {
        if (!gameManager.isMapOpened)
            return;

        scrollWheelInput = inputManager.controls.MapCamera.Zoom.ReadValue<float>();
        if (scrollWheelInput == 0)
            return;

        if (scrollWheelInput < 0)
        {
            zoomLevel -= 1;
            zoomLevel = Mathf.Clamp(zoomLevel, 1, 7);
            pixelPerfectCamera.refResolutionX = Mathf.FloorToInt(Screen.width / zoomLevel);
            pixelPerfectCamera.refResolutionY = Mathf.FloorToInt(Screen.height / zoomLevel);
        }
        else if (scrollWheelInput > 0)
        {
            zoomLevel += 1;
            zoomLevel = Mathf.Clamp(zoomLevel, 1, 7);
            pixelPerfectCamera.refResolutionX = Mathf.FloorToInt(Screen.width / zoomLevel);
            pixelPerfectCamera.refResolutionY = Mathf.FloorToInt(Screen.height / zoomLevel);
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
