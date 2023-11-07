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

    public GameObject preBuilding;

    UnitDrag unitDrag;
    RemoveBuild removeBuild;
    UpgradeBuild UpgradeBuild;

    GameObject selectedBuild;

    InputManager inputManager;
    bool isClick = false;
    bool isLeftClick;

    DragSlot dragSlot;

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
        dragSlot = DragSlot.instance;

        inputManager = InputManager.instance;
        inputManager.controls.MainCamera.LeftMouseButtonDown.performed += ctx => LeftMouseButtonDown();
        inputManager.controls.MainCamera.LeftMouseButtonUp.performed += ctx => LeftMouseButtonUp();
        inputManager.controls.MainCamera.RightMouseButtonDown.performed += ctx => RightMouseButtonDown();
        inputManager.controls.MainCamera.RightMouseButtonUp.performed += ctx => RightMouseButtonUp();
    }

    private void Update()
    {
        if (!preBuilding.activeSelf && isClick)
        {
            endPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            BoxSizeChange();

            if (isLeftClick)
            {
                if (inputManager.ctrl && !inputManager.shift)
                    ColorSet(Color.blue);
                else if (inputManager.shift && !inputManager.ctrl)
                    ColorSet(Color.red);
                else
                    ColorSet(Color.green);
            }
            else
            {
                sprite.enabled = false;
            }
        }
    }

    void LeftMouseButtonDown()
    {
        if (preBuilding.activeSelf) return;
        if (dragSlot.slot.item != null) return;

        if (!RaycastUtility.IsPointerOverUI(Input.mousePosition))
        {
            startPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            sprite.enabled = true;
            isClick = true;
            isLeftClick = true;
        }
    }

    void LeftMouseButtonUp()
    {
        if (preBuilding.activeSelf) return;
        if (dragSlot.slot.item != null) return;

        if (inputManager.ctrl && !inputManager.shift)
            UpgradeBuild.LeftMouseUp(startPosition, endPosition);
        else if (inputManager.shift && !inputManager.ctrl)
            removeBuild.LeftMouseUp(startPosition, endPosition);
        else
            unitDrag.LeftMouseUp(startPosition, endPosition);

        DisableFunc();
    }

    void RightMouseButtonDown()
    {
        if (preBuilding.activeSelf) return;
        if (dragSlot.slot.item != null) return;

        if (!RaycastUtility.IsPointerOverUI(Input.mousePosition))
        {
            startPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            isClick = true;
            isLeftClick = false;
        }
    }

    void RightMouseButtonUp()
    {
        if (preBuilding.activeSelf) return;
        if (dragSlot.slot.item != null) return;

        if (!inputManager.shift)
        {
            if (unitDrag.isSelectingUnits)
                unitDrag.RightMouseUp(startPosition, endPosition);
            else
            {
                if (!RaycastUtility.IsPointerOverUI(Input.mousePosition))
                {
                    if (selectedBuild)
                    {
                        RaycastHit2D[] hits = Physics2D.RaycastAll(endPosition, Vector2.zero);

                        selectedBuild.TryGetComponent(out UnitFactory unitFactory);
                        selectedBuild.TryGetComponent(out TransportBuild transport);

                        if (hits.Length > 0)
                        {
                            foreach (RaycastHit2D hit in hits)
                            {
                                if (selectedBuild == hit.collider.gameObject)
                                {
                                    if (unitFactory)
                                        unitFactory.DestroyLineRenderer();
                                    else if(transport)
                                        transport.DestroyLineRenderer();
                                    break;
                                }
                                else if (unitFactory)
                                {
                                    unitFactory.ResetLine(endPosition);
                                    unitFactory.UnitSpawnPosSet(endPosition);
                                }
                                else if (transport && hit.collider.TryGetComponent(out TransportBuild othTrans))
                                {
                                    transport.ResetLine(othTrans.transform.position);
                                    transport.TakeBuildSet(othTrans);
                                }
                            }
                        }
                        else
                        {
                            if (unitFactory)
                            {
                                unitFactory.ResetLine(endPosition);
                                unitFactory.UnitSpawnPosSet(endPosition);
                            }
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
        isClick = false;
    }

    public void SelectBuild(GameObject obj)
    {
        selectedBuild = obj;
    }

    public void CancelBuild()
    {
        selectedBuild = null;
    }
}
