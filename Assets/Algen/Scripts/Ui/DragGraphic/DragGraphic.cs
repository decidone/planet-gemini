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
    bool clickCheck = false;
    Vector2 mousePos;

    public GameObject preBuilding;

    UnitDrag unitDrag;
    RemoveBuild removeBuild;
    UpgradeBuild UpgradeBuild;

    bool isLineDrawing;

    [SerializeField]
    GameObject lineObj;
    LineRenderer lineRenderer;
    GameObject transportBuild;

    InputManager inputManager;

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
        isLineDrawing = false;

        inputManager = InputManager.instance;
        inputManager.controls.MainCamera.LeftMouseButtonDown.performed += ctx => LeftMouseButtonDown();
        inputManager.controls.MainCamera.LeftMouseButtonUp.performed += ctx => LeftMouseButtonUp();
        inputManager.controls.MainCamera.RightMouseButtonDown.performed += ctx => RightMouseButtonDown();
        inputManager.controls.MainCamera.RightMouseButtonUp.performed += ctx => RightMouseButtonUp();
    }

    private void Update()
    {
        if (!preBuilding.activeSelf)
        {
            if (clickCheck && !isLineDrawing)
            {
                endPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                BoxSizeChange();
            }
            else if (!clickCheck && isLineDrawing)
            {
                mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                endPosition = new Vector3(mousePos.x, mousePos.y, -1f);
                lineRenderer.SetPosition(1, endPosition);
            }
        }
    }

    void LeftMouseButtonDown()
    {
        if (preBuilding.activeSelf) return;

        if (!RaycastUtility.IsPointerOverUI(Input.mousePosition))
        {
            startPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            clickCheck = true;
            isLineDrawing = false;
            sprite.enabled = true;
            if (lineRenderer != null)
                Destroy(lineRenderer.gameObject);
        }

        if (clickCheck && !isLineDrawing)
        {
            if (!inputManager.ctrl)
                ColorSet(Color.green);
            else if (inputManager.ctrl)
                ColorSet(Color.blue);
        }
    }

    void LeftMouseButtonUp()
    {
        if (preBuilding.activeSelf) return;

        if (!inputManager.ctrl)
            unitDrag.LeftMouseUp(startPosition, endPosition);
        else if (inputManager.ctrl)
            UpgradeBuild.LeftMouseUp(startPosition, endPosition);
        DisableFunc();
    }

    void RightMouseButtonDown()
    {
        if (preBuilding.activeSelf) return;

        if (inputManager.shift && !RaycastUtility.IsPointerOverUI(Input.mousePosition))
        {
            startPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            clickCheck = true;
            isLineDrawing = false;
            sprite.enabled = true;
            if (lineRenderer != null)
                Destroy(lineRenderer.gameObject);
        }
        else if (!RaycastUtility.IsPointerOverUI(Input.mousePosition) && !isLineDrawing)
        {
            startPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D[] hits = Physics2D.RaycastAll(startPosition, Vector2.zero);

            if (hits.Length > 0)
            {
                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.collider.TryGetComponent(out TransportBuild trBuild))
                    {
                        transportBuild = trBuild.gameObject;
                        Vector3 pos = new Vector3(transportBuild.transform.position.x, transportBuild.transform.position.y, -1);
                        if (trBuild.takeBuild != null)
                            trBuild.ResetTakeBuild();
                        LineDrawStart(pos);
                        isLineDrawing = true;
                        clickCheck = false;
                        break;
                    }
                }
            }
        }

        if (clickCheck && !isLineDrawing)
        {
            if (inputManager.shift)
                ColorSet(Color.red);
        }
    }

    void RightMouseButtonUp()
    {
        if (preBuilding.activeSelf) return;

        if (clickCheck)
        {
            if (unitDrag.isSelectingUnits && !inputManager.shift)
                unitDrag.RightMouseUp(startPosition, endPosition);
            else if (inputManager.shift)
                removeBuild.RightMouseUp(startPosition, endPosition);
            DisableFunc();
        }
        else if (isLineDrawing)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(endPosition, Vector2.zero);

            if (hits.Length > 0)
            {
                bool isSameObj = false;
                bool isOnStructure = false;
                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.collider.GetComponent<Structure>())
                    {
                        isOnStructure = true;
                        if (hit.collider.TryGetComponent(out TransportBuild othTrans) && transportBuild != hit.collider.gameObject)
                        {
                            transportBuild.GetComponent<TransportBuild>().TakeBuildSet(othTrans);
                            break;
                        }
                        else if (transportBuild == hit.collider.gameObject)
                        {
                            isSameObj = true;
                        }
                    }
                }
                if (!isSameObj || !isOnStructure)
                    EndDrawLine();
            }
            else
                EndDrawLine();
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

    public void LineDrawStart(Vector2 startPos)
    {
        GameObject currentLine = Instantiate(lineObj, startPos, Quaternion.identity);
        lineRenderer = currentLine.GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, startPos);
    }

    void EndDrawLine()
    {
        isLineDrawing = false;
        Destroy(lineRenderer.gameObject);
    }
}
