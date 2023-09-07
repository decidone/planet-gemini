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
    bool rightUp = false;

    public GameObject preBuilding;

    #region Singleton
    public static DragGraphic instance;

    UnitDrag unitDrag;
    RemoveBuild removeBuild;
    bool isAltKeyDown = false;

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
        boxVisual = GetComponent<Transform>();
        sprite = GetComponent<SpriteRenderer>();
        sprite.enabled = false;
        startPosition = Vector2.zero;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            ColorSet(Color.red);
            isAltKeyDown = true;
        }
        else if (Input.GetKeyUp(KeyCode.LeftAlt))
        {
            ColorSet(Color.green);
            isAltKeyDown = false;
        }

        if (!preBuilding.activeSelf)
        {
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                startPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                rightUp = false;
                clickCheck = true;
                sprite.enabled = true;
            }
            if (clickCheck)
            {
                if (Input.GetMouseButton(0))
                {
                    endPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    BoxSizeChange();
                }
                if (Input.GetMouseButtonUp(0) && !rightUp)
                {
                    if (!isAltKeyDown)
                        unitDrag.LeftMouseUp(startPosition, endPosition);
                    else
                        removeBuild.LeftMouseUp(startPosition, endPosition);
                    DisableFunc();
                    clickCheck = false;
                    rightUp = false;
                }
            }
            if (Input.GetMouseButtonDown(1))
            {
                if (!isAltKeyDown && startPosition == endPosition)
                    unitDrag.RightMouseUp();
                DisableFunc();
                clickCheck = false;
                rightUp = true;
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
    }
}
