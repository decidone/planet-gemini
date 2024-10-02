using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

// UTF-8 설정
public class CameraController : MonoBehaviour
{
    public Transform target;
    [SerializeField] Vector3 offset;
    Vector3 camPos;
    PixelPerfectCamera pixelPerfectCamera;
    public int zoomLevel;
    float scrollWheelInput;

    InputManager inputManager;

    int width = 1920;
    int height = 1080;

    [SerializeField] float borderX;
    [SerializeField] float borderY;

    float mapWidth;
    float mapHeight;
    float mapOffsetY;

    #region Singleton
    public static CameraController instance;

    void Awake()
    {
        pixelPerfectCamera = Camera.main.GetComponent<PixelPerfectCamera>();
        zoomLevel = 1;

        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }
    #endregion

    void Start()
    {
        inputManager = InputManager.instance;
        mapWidth = MapGenerator.instance.width;
        mapHeight = MapGenerator.instance.height;
        mapOffsetY = MapGenerator.instance.clientMapOffsetY;
        
        SettingsMenu settingsMenu = SettingsMenu.instance;
        WindowSizeSet(zoomLevel, settingsMenu.fixedWidth, settingsMenu.fixedHeight);
    }

    void Update()
    {
        if (Time.timeScale == 0)
        {
            return;
        }

        scrollWheelInput = inputManager.controls.MainCamera.Zoom.ReadValue<float>();
        if (scrollWheelInput == 0)
            return;

        if (scrollWheelInput < 0)
        {
            zoomLevel -= 1;
            zoomLevel = Mathf.Clamp(zoomLevel, 1, 5);
            pixelPerfectCamera.refResolutionX = Mathf.FloorToInt(width / zoomLevel);
            pixelPerfectCamera.refResolutionY = Mathf.FloorToInt(height / zoomLevel);
        }
        else if(scrollWheelInput > 0)
        {
            zoomLevel += 1;
            zoomLevel = Mathf.Clamp(zoomLevel, 1, 5);
            pixelPerfectCamera.refResolutionX = Mathf.FloorToInt(width / zoomLevel);
            pixelPerfectCamera.refResolutionY = Mathf.FloorToInt(height / zoomLevel);
        }
    }

    void LateUpdate()
    {
        float offsetY = 0;
        if (target != null)
        {
            if (!GameManager.instance.isPlayerInMarket)
            {
                if (!GameManager.instance.isPlayerInHostMap)
                    offsetY = mapOffsetY + mapHeight;
                camPos = target.position - offset;
                camPos.x = Mathf.Clamp(camPos.x, (borderX / zoomLevel), mapWidth - (borderX / zoomLevel));
                camPos.y = Mathf.Clamp(camPos.y, offsetY + (borderY / zoomLevel), mapHeight + offsetY - (borderY / zoomLevel));
                transform.position = camPos;
            }
            else
            {
                transform.position = target.position - offset;
            }
        }
    }

    public void ChangeZoomLv(int lv)
    {
        zoomLevel = Mathf.Clamp(lv, 1, 5);
        pixelPerfectCamera.refResolutionX = Mathf.FloorToInt(width / zoomLevel);
        pixelPerfectCamera.refResolutionY = Mathf.FloorToInt(height / zoomLevel);
    }

    public void WindowSizeSet(int lv, int widthSize, int heightSize)
    {
        width = widthSize;
        height = heightSize;
        ChangeZoomLv(lv) ;
        Debug.Log(width + " : " + height);
    }
}
