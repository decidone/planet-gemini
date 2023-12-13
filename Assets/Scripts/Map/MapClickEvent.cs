using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapClickEvent : MonoBehaviour
{
    [SerializeField]
    protected GameObject lineObj;
    [HideInInspector]
    public LineRenderer lineRenderer;
    protected Vector3 startLine;
    protected Vector3 endLine;
    Vector2 pos;
    LDConnector ldConnector;
    [HideInInspector]
    public string strType;
    bool updateRenderer;
    MapCameraController controller;
    Camera cam;

    private void Awake()
    {
        strType = "";
        updateRenderer = false;
        pos = new Vector2();
        ldConnector = GetComponent<LDConnector>();
        if (ldConnector != null)
            strType = "ldConnector";
    }

    private void Start()
    {
        controller = GameManager.instance.mapCameraController;
        cam = controller.cam;
    }

    private void Update()
    {
        if (updateRenderer && lineRenderer != null)
        {
            pos = cam.ScreenToWorldPoint(Input.mousePosition);
            endLine = new Vector3(pos.x, pos.y, -1);
            lineRenderer.SetPosition(1, endLine);
        }
    }

    public void StartRenderer()
    {
        if (lineRenderer != null)
        {
            //일단 라인 하나만 했으니 기존 라인 제거 or 비교 후 추가 등
            DestroyLineRenderer();
        }

        startLine = new Vector3(transform.position.x, transform.position.y, -1);
        GameObject currentLine = Instantiate(lineObj, startLine, Quaternion.identity);
        currentLine.layer = LayerMask.NameToLayer("MapUI");
        lineRenderer = currentLine.GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, startLine);
        updateRenderer = true;
    }

    public void EndRenderer(MapClickEvent clickEvent)
    {
        updateRenderer = false;
        if (lineRenderer != null)
        {
            endLine = new Vector3(clickEvent.transform.position.x, clickEvent.transform.position.y, -1);
            lineRenderer.SetPosition(1, endLine);
        }
    }

    public void DestroyLineRenderer()
    {
        updateRenderer = false;
        if (lineRenderer != null)
        {
            Destroy(lineRenderer.gameObject);
            lineRenderer = null;
        }
    }
}
