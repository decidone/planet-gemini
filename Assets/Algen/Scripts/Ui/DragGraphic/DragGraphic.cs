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

    UnitDrag unitDrag;
    RemoveBuild removeBuild;

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
        boxVisual = GetComponent<Transform>();
        sprite = GetComponent<SpriteRenderer>();
        sprite.enabled = false;
        startPosition = Vector2.zero;
    }

    private void Update()
    {
        bool altKeyHeld = Input.GetKey(KeyCode.LeftAlt);
        bool isMouseOverUI = EventSystem.current.IsPointerOverGameObject();
        bool isMouseButtonDown = Input.GetMouseButtonDown(0);
        bool isMouseButtonUp = Input.GetMouseButtonUp(0);

        if (altKeyHeld)        
            ColorSet(Color.red);        
        else
            ColorSet(Color.green);        

        if (!preBuilding.activeSelf)
        {
            if (isMouseButtonDown && !isMouseOverUI)
            {
                startPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                rightUp = false;
                clickCheck = true;
                sprite.enabled = true;
            }

            if (clickCheck)
            {
                endPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                BoxSizeChange();

                if (isMouseButtonUp && !rightUp)
                {
                    if (altKeyHeld)
                    {
                        removeBuild.LeftMouseUp(startPosition, endPosition);
                    }
                    else
                    {
                        unitDrag.LeftMouseUp(startPosition, endPosition);
                    }

                    DisableFunc();
                    rightUp = false;
                }
            }

            if (Input.GetMouseButtonDown(1))
            {
                if (!altKeyHeld && startPosition == endPosition)
                {
                    unitDrag.RightMouseUp();
                }
                DisableFunc();
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
        clickCheck = false;
    }
}
