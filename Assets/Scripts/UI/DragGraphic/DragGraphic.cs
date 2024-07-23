using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragGraphic : MonoBehaviour
{
    Transform boxVisual;
    Vector3 startPosition;
    Vector3 endPosition;
    SpriteRenderer sprite;
    Vector2 mousePos;

    PreBuilding preBuilding;

    UnitDrag unitDrag;
    RemoveBuild removeBuild;
    UpgradeBuild UpgradeBuild;

    GameObject selectedBuild;

    InputManager inputManager;
    bool isDrag;
    bool isCtrlDrag;
    bool isShiftDrag;

    bool upgradeBtnOn;
    bool removeBtnOn;

    #region Singleton
    public static DragGraphic instance;

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

    void Start()
    {
        unitDrag = GameManager.instance.GetComponent<UnitDrag>();
        removeBuild = GameManager.instance.GetComponent<RemoveBuild>();
        UpgradeBuild = GameManager.instance.GetComponent<UpgradeBuild>();
        boxVisual = GetComponent<Transform>();
        sprite = GetComponent<SpriteRenderer>();
        sprite.enabled = false;
        startPosition = Vector2.zero;
        isDrag = false;
        isCtrlDrag = false;
        isShiftDrag = false;
        preBuilding = PreBuilding.instance;
        inputManager = InputManager.instance;
        inputManager.controls.MainCamera.LeftMouseButtonDown.performed += ctx => LeftMouseButtonDown();
        inputManager.controls.MainCamera.LeftMouseButtonUp.performed += ctx => LeftMouseButtonUp();
        inputManager.controls.MainCamera.RightMouseButtonDown.performed += ctx => RightMouseButtonDown();
        inputManager.controls.MainCamera.RightMouseButtonUp.performed += ctx => RightMouseButtonUp();
    }

    private void Update()
    {
        if (!preBuilding.isBuildingOn && isDrag)
        {
            endPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            BoxSizeChange();

            if (((isCtrlDrag && !inputManager.ctrl) && !upgradeBtnOn) || ((isShiftDrag && !inputManager.shift) && !removeBtnOn))
                DisableFunc();
        }
    }

    void LeftMouseButtonDown()
    {
        if (preBuilding.isBuildingOn) return;
        if (ItemDragManager.instance.isDrag) return;

        if (!RaycastUtility.IsPointerOverUI(Input.mousePosition))
        {
            startPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            sprite.enabled = true;
            isDrag = true;

            if ((inputManager.ctrl && !inputManager.shift) || upgradeBtnOn)
            {
                isCtrlDrag = true;
                ColorSet(Color.blue);
            }
            else if ((inputManager.shift && !inputManager.ctrl) || removeBtnOn)
            {
                isShiftDrag = true;
                ColorSet(Color.red);
            }
            else
            {
                ColorSet(Color.green);
            }
        }
    }

    void LeftMouseButtonUp()
    {
        if (preBuilding.isBuildingOn) return;
        if (ItemDragManager.instance.isDrag) return;
        if (!isDrag) return;

        if (isCtrlDrag || upgradeBtnOn)
            UpgradeBuild.LeftMouseUp(startPosition, endPosition);
        else if (isShiftDrag || removeBtnOn)
            removeBuild.LeftMouseUp(startPosition, endPosition);
        else
            unitDrag.LeftMouseUp(startPosition, endPosition);

        DisableFunc();
    }

    void RightMouseButtonDown()
    {
        if (preBuilding.isBuildingOn) return;
        if (ItemDragManager.instance.isDrag) return;

        DisableFunc();
    }

    void RightMouseButtonUp()
    {
        if (preBuilding.isBuildingOn) return;
        if (ItemDragManager.instance.isDrag) return;

        endPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (unitDrag.isSelectingUnits)
            unitDrag.RightMouseUp(endPosition, endPosition);
        else
        {
            if (!RaycastUtility.IsPointerOverUI(Input.mousePosition))
            {
                if (selectedBuild)
                {
                    RaycastHit2D[] hits = Physics2D.RaycastAll(endPosition, Vector2.zero);

                    selectedBuild.TryGetComponent(out UnitFactory unitFactory);
                    //selectedBuild.TryGetComponent(out Transporter transport);

                    if (hits.Length > 0)
                    {
                        foreach (RaycastHit2D hit in hits)
                        {
                            if (selectedBuild == hit.collider.gameObject)
                            {
                                if (unitFactory)
                                    unitFactory.DestroyLineRenderer();
                                //else if(transport)
                                //    transport.DestroyLineRenderer();
                                break;
                            }
                            else if (unitFactory)
                            {
                                unitFactory.ResetLine(endPosition);
                                unitFactory.UnitSpawnPosSetServerRpc(endPosition);
                            }
                            //else if (transport && hit.collider.TryGetComponent(out Transporter othTrans))
                            //{
                            //    transport.ResetLine(othTrans.transform.position);
                            //    transport.TakeBuildSet(othTrans);
                            //}
                        }
                    }
                    else
                    {
                        if (unitFactory)
                        {
                            unitFactory.ResetLine(endPosition);
                            unitFactory.UnitSpawnPosSetServerRpc(endPosition);
                        }
                    }
                }
            }
        }
        DisableFunc();
    }

    public void ColorSet(Color color)
    {
        Color slotColor = sprite.color;
        slotColor = color;
        slotColor.a = 0.2f;
        sprite.color = slotColor;
    }

    public void BoxSizeChange()
    {
        Vector2 boxCenter = (startPosition + endPosition) / 2;
        boxVisual.position = new Vector3(boxCenter.x, boxCenter.y, boxVisual.position.z);

        Vector2 boxSize = new Vector2(Mathf.Abs(startPosition.x - endPosition.x), Mathf.Abs(startPosition.y - endPosition.y));
        boxVisual.localScale = new Vector3(boxSize.x, boxSize.y, boxVisual.localScale.z);
    }

    public void DisableFunc()
    {
        startPosition = Vector2.zero;
        endPosition = Vector2.zero;
        sprite.enabled = false;
        isDrag = false;
        isCtrlDrag = false;
        isShiftDrag = false;
    }

    public void SelectBuild(GameObject obj)
    {
        selectedBuild = obj;
    }

    public void CancelBuild()
    {
        selectedBuild = null;
    }

    public void BtnFuncReset()
    {
        upgradeBtnOn = false;
        removeBtnOn = false;
    }

    public void BtnFunc(bool isUpgrade)
    {
        if (isUpgrade)
        {
            upgradeBtnOn = true;
            removeBtnOn = false;
        }
        else
        {
            removeBtnOn = true;
            upgradeBtnOn = false;
        }
    }
}
