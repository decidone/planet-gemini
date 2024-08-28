using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

// UTF-8 설정
public class CameraController : MonoBehaviour
{
    public Transform target;
    [SerializeField]
    Vector3 offset;

    PixelPerfectCamera pixelPerfectCamera;
    public int zoomLevel;
    float scrollWheelInput;

    InputManager inputManager;

    int width = 1920;
    int height = 1080;

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
        SettingsMenu settingsMenu = SettingsMenu.instance;
        WindowSizeSet(zoomLevel, settingsMenu.fixedWidth, settingsMenu.fixedHeight);
    }

    void Update()
    {
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
        if (target != null)
            transform.position = target.position - offset;
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
