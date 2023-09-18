using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragGraphic : MonoBehaviour
{
    Transform boxVisual;
    Vector2 startPosition;
    Vector2 endPosition;
    SpriteRenderer sprite;
    bool clickCheck = false;
    bool holdAnyKey = false;

    public GameObject preBuilding;

    UnitDrag unitDrag;
    RemoveBuild removeBuild;
    UpgradeBuild UpgradeBuild;

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

    // Start is called before the first frame update
    void Start()
    {
        unitDrag = GameManager.instance.GetComponent<UnitDrag>();
        removeBuild = GameManager.instance.GetComponent<RemoveBuild>();
        UpgradeBuild = GameManager.instance.GetComponent<UpgradeBuild>();
        boxVisual = GetComponent<Transform>();
        sprite = GetComponent<SpriteRenderer>();
        sprite.enabled = false;
        startPosition = Vector2.zero;
    }

    private void Update()
    {
        bool ctrlKeyHeld = Input.GetKey(KeyCode.LeftControl);
        bool isMouseOverUI = EventSystem.current.IsPointerOverGameObject();
        bool isLeftMouseButtonDown = Input.GetMouseButtonDown(0);
        bool isLeftMouseButtonUp = Input.GetMouseButtonUp(0);
        bool isRightMouseButtonDown = Input.GetMouseButtonDown(1);
        bool isRightMouseButtonUp = Input.GetMouseButtonUp(1);

        if (!preBuilding.activeSelf)
        {
            if (ctrlKeyHeld)
                holdAnyKey = true;
            else
                holdAnyKey = false;

            if ((isLeftMouseButtonDown || isRightMouseButtonDown) && !isMouseOverUI)
            {
                startPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                clickCheck = true;
                sprite.enabled = true;
            }

            if (clickCheck)
            {
                if (Input.GetMouseButton(0))
                {
                    if (!holdAnyKey)
                        ColorSet(Color.green);
                    else if (ctrlKeyHeld)
                        ColorSet(Color.blue);
                }
                else if (Input.GetMouseButton(1))
                    ColorSet(Color.red);

                endPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                BoxSizeChange();

                if (isLeftMouseButtonUp)
                {
                    if (!holdAnyKey)
                        unitDrag.LeftMouseUp(startPosition, endPosition);
                    else if (ctrlKeyHeld)
                        UpgradeBuild.LeftMouseUp(startPosition, endPosition);
                    DisableFunc();
                }
                else if (isRightMouseButtonUp)
                {
                    if (unitDrag.isSelectingUnits && startPosition == endPosition)
                    {
                        unitDrag.RightMouseUp();
                    }
                    else
                        removeBuild.LeftMouseUp(startPosition, endPosition);
                    DisableFunc();
                }
            }
        }
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
        clickCheck = false;
    }
}
