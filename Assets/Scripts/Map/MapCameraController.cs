using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.U2D;

// UTF-8 설정
public class MapCameraController : MonoBehaviour
{
    public Transform target;
    [SerializeField]
    Vector3 offset;
    [SerializeField]
    GameObject CameraObj;
    [SerializeField]
    Canvas canvas;
    [SerializeField]
    float dragSpeed;
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
    Vector3 camPos;
    int mainCamZoom;
    int zoomLevel;
    float scrollWheelInput;
    float mapWidth;
    float mapHeight;
    float mapOffsetY;
    Vector2 movement;

    MapClickEvent focusedEvent;
    MapClickEvent tempEvent;
    bool isLineRendered;

    public static MapCameraController instance;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        zoomLevel = 1;
        mainCamZoom = 1;
        pixelPerfectCamera = CameraObj.GetComponent<PixelPerfectCamera>();
        cam = CameraObj.GetComponent<Camera>();
    }

    void Start()
    {
        gameManager = GameManager.instance;
        mainCamController = Camera.main.GetComponent<CameraController>();
    }

    void OnEnable()
    {
        inputManager = InputManager.instance;
        inputManager.controls.State.ToggleMap.performed += ToggleMap;
        inputManager.controls.MapCamera.LeftClick.performed += LeftClick;
    }

    void OnDisable()
    {
        inputManager.controls.State.ToggleMap.performed -= ToggleMap;
        inputManager.controls.MapCamera.LeftClick.performed -= LeftClick;
    }

    void FixedUpdate()
    {
        camPos.x = Mathf.Clamp(camPos.x + ((movement.x * dragSpeed) / zoomLevel), borderX / zoomLevel, mapWidth - (borderX / zoomLevel));
        camPos.y = Mathf.Clamp(camPos.y + ((movement.y * dragSpeed) / zoomLevel), mapOffsetY + (borderY / zoomLevel), mapHeight + mapOffsetY - (borderY / zoomLevel));
        transform.position = camPos;
    }

    void Update()
    {
        if (Time.timeScale == 0)
        {
            return;
        }

        if (!inputManager.isMapOpened)
            return;

        camPos = transform.position;
        movement = inputManager.controls.MapCamera.Movement.ReadValue<Vector2>();
        scrollWheelInput = inputManager.controls.MapCamera.Zoom.ReadValue<float>();

        if (scrollWheelInput != 0)
        {
            if (scrollWheelInput < 0)
            {
                zoomLevel -= 1;
                zoomLevel = Mathf.Clamp(zoomLevel, 1, 4);
                pixelPerfectCamera.refResolutionX = Mathf.FloorToInt(Screen.width / zoomLevel);
                pixelPerfectCamera.refResolutionY = Mathf.FloorToInt(Screen.height / zoomLevel);

                camPos.x = Mathf.Clamp(camPos.x, borderX / zoomLevel, mapWidth - (borderX / zoomLevel));
                camPos.y = Mathf.Clamp(camPos.y, mapOffsetY + (borderY / zoomLevel), mapHeight + mapOffsetY - (borderY / zoomLevel));
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

    public void SetCamRange(Map map)
    {
        mapWidth = map.width;
        mapHeight = map.height;
        mapOffsetY = map.offsetY;
    }

    void ToggleMap(InputAction.CallbackContext ctx)
    {
        ToggleMap();
    }

    public void ToggleMap()
    {
        if (inputManager.mouseLeft || inputManager.mouseRight)
            return;

        if (!inputManager.isMapOpened)
        {
            mainCamZoom = mainCamController.zoomLevel;  //메인 카메라의 줌 레벨에 따라 픽셀퍼펙트가 깨지는 버그가 있어서 줌 레벨을 고정시켜 줌
            mainCamController.ChangeZoomLv(1);
            gameManager.CloseAllOpenedUI();
            gameManager.CloseBasicUIs();

            OpenUI();
            inputManager.OpenMap();
        }
        else
        {
            mainCamController.ChangeZoomLv(mainCamZoom);
            
            if(!gameManager.isBasicUIClose) gameManager.OpenBasicUIs();

            CloseUI();
            inputManager.CloseMap();
        }

        GameManager.instance.onUIChangedCallback?.Invoke(CameraObj);
    }

    void LeftClick(InputAction.CallbackContext ctx)
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
            if (hits[i].collider.gameObject.layer == LayerMask.NameToLayer("Portal")
                || hits[i].collider.gameObject.layer == LayerMask.NameToLayer("LocalPortal"))
            {
                PlayerController player = GameManager.instance.player.GetComponent<PlayerController>();
                bool isteleported = player.TeleportLocal(hits[i].collider.transform.position);
                if (isteleported)
                    ToggleMap();
            }
        }

        if (focusedEvent == null)   //첫 클릭
        {
            if (tempEvent != null)
            {
                SetFocus();
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
                    //라인 생성
                    //기존 라인 끊는거는 클릭이벤트쪽에서 처리
                    if (focusedEvent.StartRenderer())
                        isLineRendered = true;
                }
                else
                {
                    if (focusedEvent.strType == tempEvent.strType)
                    {
                        if (!focusedEvent.RemoveRenderer(tempEvent))
                        {
                            CancelFocus();
                            SetFocus();
                        }
                    }
                    else
                    {
                        CancelFocus();
                        SetFocus();
                    }
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
                        if (focusedEvent.EndRenderer(tempEvent))
                            isLineRendered = false;

                        Debug.Log("EndRenderer");
                    }
                    else
                    {
                        //연결 불가능한 건물
                        CancelRender();
                        CancelFocus();
                        SetFocus();
                    }
                }
            }
        }

        tempEvent = null;
    }

    void SetFocus()
    {
        focusedEvent = tempEvent;
        GameObject point = gameManager.SelectPointSpawn(focusedEvent.gameObject);
        point.layer = LayerMask.NameToLayer("MapUI");
    }

    void CancelFocus()
    {
        focusedEvent = null;
        gameManager.SelectPointRemove();
    }

    void CancelRender()
    {
        isLineRendered = false;
        if (focusedEvent != null)
        {
            focusedEvent.DestroyLineRenderer();
        }
    }

    void OpenUI()
    {
        camPos = target.position - offset;
        camPos.x = Mathf.Clamp(camPos.x, borderX / zoomLevel, mapWidth - (borderX / zoomLevel));
        camPos.y = Mathf.Clamp(camPos.y, mapOffsetY + (borderY / zoomLevel), mapHeight + mapOffsetY - (borderY / zoomLevel));
        transform.position = camPos;

        if (PreBuilding.instance.isBuildingOn)
            PreBuilding.instance.CancelBuild();
        BuildingInvenManager.instance.onToggleMapChangeCallback?.Invoke();
        ScienceManager.instance.onToggleMapChangeCallback?.Invoke();

        CameraObj.SetActive(true);
    }

    void CloseUI()
    {
        CameraObj.SetActive(false);
    }
}
