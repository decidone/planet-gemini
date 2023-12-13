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
    [SerializeField]
    Canvas canvas;
    [SerializeField]
    int dragSpeed;
    [SerializeField]
    float borderX;
    [SerializeField]
    float borderY;

    PixelPerfectCamera pixelPerfectCamera;
    [HideInInspector]
    public Camera cam;
    GameManager gameManager;
    InputManager inputManager;
    CameraController mainCamController;
    Vector3 camStartPos;
    Vector3 camPos;
    Vector3 dragStartPos;
    Vector3 dragPos;
    bool mouseHold;
    int mainCamZoom;
    int zoomLevel;
    float scrollWheelInput;
    float mapWidth;
    float mapHeight;

    MapClickEvent focusedEvent;
    MapClickEvent tempEvent;
    bool isLineRendered;

    void Awake()
    {
        mouseHold = false;
        zoomLevel = 1;
        mainCamZoom = 1;
        pixelPerfectCamera = CameraObj.GetComponent<PixelPerfectCamera>();
        cam = CameraObj.GetComponent<Camera>();
        focusedEvent = new MapClickEvent();
        tempEvent = new MapClickEvent();
    }

    void Start()
    {
        gameManager = GameManager.instance;
        inputManager = InputManager.instance;
        inputManager.controls.State.ToggleMap.performed += ctx => ToggleMap();
        inputManager.controls.MapCamera.MouseHold.performed += ctx => ToggleMouse();
        inputManager.controls.MapCamera.LeftClick.performed += ctx => LeftClick();
        inputManager.controls.MapCamera.RightClick.performed += ctx => RightClick();

        mainCamController = Camera.main.GetComponent<CameraController>();

        mapWidth = gameManager.map.width;
        mapHeight = gameManager.map.height;
    }

    void Update()
    {
        if (!inputManager.isMapOpened)
            return;

        if (mouseHold)
        {
            dragPos.x = (dragStartPos.x - Input.mousePosition.x) / (dragSpeed * zoomLevel);
            dragPos.y = (dragStartPos.y - Input.mousePosition.y) / (dragSpeed * zoomLevel);
            camPos.x = Mathf.Clamp(camStartPos.x + dragPos.x, borderX/zoomLevel, mapWidth - (borderX/zoomLevel));
            camPos.y = Mathf.Clamp(camStartPos.y + dragPos.y, borderY/zoomLevel, mapHeight - (borderY/zoomLevel));
            transform.position = camPos;
        }
        else
        {
            scrollWheelInput = inputManager.controls.MapCamera.Zoom.ReadValue<float>();
            if (scrollWheelInput != 0)
            {
                camPos = transform.position;
                if (scrollWheelInput < 0)
                {
                    zoomLevel -= 1;
                    zoomLevel = Mathf.Clamp(zoomLevel, 1, 4);
                    pixelPerfectCamera.refResolutionX = Mathf.FloorToInt(Screen.width / zoomLevel);
                    pixelPerfectCamera.refResolutionY = Mathf.FloorToInt(Screen.height / zoomLevel);

                    camPos.x = Mathf.Clamp(camPos.x, borderX / zoomLevel, mapWidth - (borderX / zoomLevel));
                    camPos.y = Mathf.Clamp(camPos.y, borderY / zoomLevel, mapHeight - (borderY / zoomLevel));
                    transform.position = camPos;
                }
                else if (scrollWheelInput > 0)
                {
                    zoomLevel += 1;
                    zoomLevel = Mathf.Clamp(zoomLevel, 1, 4);
                    pixelPerfectCamera.refResolutionX = Mathf.FloorToInt(Screen.width / zoomLevel);
                    pixelPerfectCamera.refResolutionY = Mathf.FloorToInt(Screen.height / zoomLevel);
                }
            }
        }
    }

    void ToggleMap()
    {
        if (inputManager.mouseLeft || inputManager.mouseRight)
            return;

        if (!inputManager.isMapOpened)
        {
            mainCamZoom = mainCamController.zoomLevel;  //메인 카메라의 줌 레벨에 따라 픽셀퍼펙트가 깨지는 버그가 있어서 줌 레벨을 고정시켜 줌
            mainCamController.ChangeZoomLv(1);
            canvas.enabled = false;

            OpenUI();
            inputManager.OpenMap();
        }
        else
        {
            mainCamController.ChangeZoomLv(mainCamZoom);
            canvas.enabled = true;

            CloseUI();
            inputManager.CloseMap();
        }
    }

    void ToggleMouse()
    {
        if (!mouseHold)
        {
            //button down
            mouseHold = true;
            dragStartPos = Input.mousePosition;
            camStartPos = transform.position;
        }
        else
        {
            //button up
            mouseHold = false;
        }
    }

    void LeftClick()
    {
        if (RaycastUtility.IsPointerOverUI(Input.mousePosition))
            return;

        Vector2 pos = cam.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D[] hits = Physics2D.RaycastAll(pos, Vector2.zero);

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider.TryGetComponent(out MapClickEvent _mapClickEvent))
            {
                tempEvent = _mapClickEvent;
            }
        }

        if (focusedEvent == null)   //첫 클릭
        {
            if (tempEvent != null)
            {
                focusedEvent = tempEvent;
            }
        }
        else if (focusedEvent != null && !isLineRendered)   //두 번째 클릭
        {
            if (tempEvent == null)
            {
                CancelFocus();
            }
            else
            {
                if (tempEvent == focusedEvent)
                {
                    // 라인 생성
                    isLineRendered = true;
                    focusedEvent.StartRenderer();   //기존 라인 끊는거는 클릭이벤트쪽에서 처리
                }
                else
                {
                    focusedEvent = tempEvent;
                }
            }
        }
        else //세 번째 클릭
        {
            if (focusedEvent == tempEvent)
            {
                //자기 자신 클릭
                CancelRender();
            }
            else
            {
                if (tempEvent == null)
                {
                    //건물이 없는 곳 클릭
                    CancelRender();
                    CancelFocus();
                }
                else
                {
                    if (focusedEvent.strType == tempEvent.strType)
                    {
                        //연결 가능한 건물
                        focusedEvent.EndRenderer(tempEvent);
                        isLineRendered = false;
                    }
                    else
                    {
                        //연결 불가능한 건물
                        CancelRender();
                        CancelFocus();
                        focusedEvent = tempEvent;
                    }
                }
            }
        }

        tempEvent = null;
    }

    void RightClick()
    {
        CancelRender();
        CancelFocus();
    }

    void CancelRender()
    {
        isLineRendered = false;
        if (focusedEvent != null)
        {
            focusedEvent.DestroyLineRenderer();
        }
    }

    void CancelFocus()
    {
        focusedEvent = null;
    }

    void OpenUI()
    {
        camPos = target.position - offset;
        camPos.x = Mathf.Clamp(camPos.x, borderX / zoomLevel, mapWidth - (borderX / zoomLevel));
        camPos.y = Mathf.Clamp(camPos.y, borderY / zoomLevel, mapHeight - (borderY / zoomLevel));
        transform.position = camPos;
        
        if (PreBuilding.instance != null)
            PreBuilding.instance.CancelBuild();
        CameraObj.SetActive(true);
    }

    void CloseUI()
    {
        CameraObj.SetActive(false);
    }
}
