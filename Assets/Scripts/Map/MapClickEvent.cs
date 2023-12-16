using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapClickEvent : MonoBehaviour
{
    [SerializeField]
    protected GameObject lineObj;
    [HideInInspector]
    public LineRenderer lineRenderer;
    public List<LineRenderer> lines;
    public int lineCountLimit;
    protected Vector3 startLine;
    protected Vector3 endLine;
    Vector2 pos;
    LDConnector ldConnector;
    [HideInInspector]
    public string strType;
    public EnergyGroupConnector Connector;
    bool updateRenderer;
    MapCameraController controller;
    Camera cam;

    private void Awake()
    {
        strType = "";
        updateRenderer = false;
        pos = new Vector2();
        ldConnector = GetComponent<LDConnector>();
        lines = new List<LineRenderer>();
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

    public bool StartRenderer()
    {
        if (lines.Count < lineCountLimit)
        {
            startLine = new Vector3(transform.position.x, transform.position.y, -1);
            GameObject currentLine = Instantiate(lineObj, startLine, Quaternion.identity);
            lineRenderer = currentLine.GetComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, startLine);
            updateRenderer = true;

            return true;
        }

        return false;
    }

    public bool EndRenderer(MapClickEvent clickEvent)
    {
        for (int i = 0; i < lines.Count; i++)
        {
            for (int j = 0; j < clickEvent.lines.Count; j++)
            {
                if (lines[i] == clickEvent.lines[j])
                {
                    // 이미 해당 건물에 연결된 라인이 있는 경우
                    return false;
                }
            }
        }

        if (lineRenderer != null && clickEvent.lines.Count < clickEvent.lineCountLimit)
        {
            updateRenderer = false;
            endLine = new Vector3(clickEvent.transform.position.x, clickEvent.transform.position.y, -1);
            lineRenderer.SetPosition(1, endLine);
            MapLine lineProps = lineRenderer.GetComponent<MapLine>();
            lineProps.lineSource = this;
            lineProps.lineTarget = clickEvent;
            lines.Add(lineRenderer);
            clickEvent.lines.Add(lineRenderer);
            
            if (strType == "ldConnector")
            {
                Connect(clickEvent.Connector);
            }

            return true;
        }

        return false;
    }

    public bool RemoveRenderer(MapClickEvent clickEvent)
    {
        for (int i = 0; i < lines.Count; i++)
        {
            for (int j = 0; j < clickEvent.lines.Count; j++)
            {
                if (lines[i] == clickEvent.lines[j])
                {
                    LineRenderer line = lines[i];
                    clickEvent.lines.Remove(line);
                    lines.Remove(line);
                    Destroy(line.gameObject);
                    if (strType == "ldConnector")
                    {
                        Disconnect(clickEvent.Connector);
                    }
                    return true;
                }
            }
        }

        return false;
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

    public void Connect(EnergyGroupConnector conn)
    {
        Connector.CheckAndAdd(conn);
        conn.CheckAndAdd(Connector);

        if (Connector.group != null && conn.group != null)
        {
            if (Connector.group != conn.group)
                Connector.group.MergeGroup(conn.group);
        }
    }

    public void Disconnect(EnergyGroupConnector conn)
    {
        Connector.SubtractConnector(conn);
        conn.SubtractConnector(Connector);

        if (Connector.group != null)
        {
            Connector.group.ConnectionCheck(0);
        }
    }

    public void RemoveAllLines()
    {
        Queue<LineRenderer> queue = new Queue<LineRenderer>(lines);
        int count = queue.Count;
        for (int i = 0; i < count; i++)
        {
            LineRenderer line = queue.Dequeue();
            MapLine lineProps = line.GetComponent<MapLine>();
            if (lineProps != null)
            {
                lineProps.lineSource.lines.Remove(line);
                lineProps.lineTarget.lines.Remove(line);
                Destroy(line.gameObject);
            }
        }
        queue.Clear();
        lines.Clear();
    }
}
