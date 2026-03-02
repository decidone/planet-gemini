using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MapClickEvent : NetworkBehaviour
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
    public Transporter transporter;
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
        lines = new List<LineRenderer>();

        ldConnector = GetComponent<LDConnector>();
        transporter = GetComponent<Transporter>();
        if (ldConnector != null)
            strType = "ldConnector";
        else if (transporter != null)
            strType = "transporter";
    }

    private void Start()
    {
        controller = GameManager.instance.mapCameraController;
        cam = controller.cam;
    }

    private void Update()
    {
        if (Time.timeScale == 0)
        {
            return;
        }

        if (updateRenderer && lineRenderer != null)
        {
            pos = cam.ScreenToWorldPoint(Input.mousePosition);
            endLine = new Vector3(pos.x, pos.y, -1);
            lineRenderer.SetPosition(1, endLine);
        }
    }

    public void GameStartSetRenderer(MapClickEvent othMapClickEvent)
    {
        if (this == othMapClickEvent)
            return;

        if (strType == "ldConnector")
        {
            for (int i = 0; i < lines.Count; i++)
            {
                for (int j = 0; j < othMapClickEvent.lines.Count; j++)
                {
                    if (lines[i] == othMapClickEvent.lines[j])
                    {
                        // 이미 해당 건물에 연결된 라인이 있는 경우
                        return;
                    }
                }
            }
        }
        //else if (strType == "transporter")
        //{
        //    if (transporter.takeBuild == othMapClickEvent.transporter)
        //    {
        //        return;
        //    }
        //}

        startLine = new Vector3(transform.position.x, transform.position.y, -1);
        GameObject currentLine = Instantiate(lineObj, startLine, Quaternion.identity);
        lineRenderer = currentLine.GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, startLine);

        endLine = new Vector3(othMapClickEvent.transform.position.x, othMapClickEvent.transform.position.y, -1);
        lineRenderer.SetPosition(1, endLine);
        MapLine lineProps = lineRenderer.GetComponent<MapLine>();
        lineProps.lineSource = this;
        lineProps.lineTarget = othMapClickEvent;
        lines.Add(lineRenderer);
        if (strType == "ldConnector")
        {        
            othMapClickEvent.lines.Add(lineRenderer);
            Connect(othMapClickEvent.Connector);
        }
        else if (strType == "transporter")
        {
            Connect(othMapClickEvent);
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
        if (this == clickEvent)
            return false;

        if (strType == "ldConnector")
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
            {   // 라인렌더러를 삭제후 rpc로 시작점 끝점을 지정해 줘야함
                updateRenderer = false;
                Destroy(lineRenderer.gameObject);
                RendererSetServerRpc(clickEvent.transform.position);
                return true;
            }
        }
        else if (strType == "transporter")
        {
            if (transporter.takeBuild == clickEvent.transporter)
            {
                return false;
            }

            if (lineRenderer != null)
            {   // 라인렌더러를 삭제후 rpc로 시작점 끝점을 지정해 줘야함
                updateRenderer = false;
                Destroy(lineRenderer.gameObject);
                RendererSetServerRpc(clickEvent.transform.position);
                return true;
            }
        }

        return false;
    }

    [ServerRpc(RequireOwnership = false)]
    void RendererSetServerRpc(Vector3 targetPos)
    {
        RendererSetClientRpc(targetPos);
    }

    [ClientRpc]
    void RendererSetClientRpc(Vector3 targetPos)
    {
        MapClickEvent clickEvent = FindTargetClickEvent(targetPos);

        startLine = new Vector3(transform.position.x, transform.position.y, -1);
        GameObject currentLine = Instantiate(lineObj, startLine, Quaternion.identity);
        lineRenderer = currentLine.GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, startLine);

        endLine = new Vector3(clickEvent.transform.position.x, clickEvent.transform.position.y, -1);
        lineRenderer.SetPosition(1, endLine);
        MapLine lineProps = lineRenderer.GetComponent<MapLine>();
        lineProps.lineSource = this;
        lineProps.lineTarget = clickEvent;
        lines.Add(lineRenderer);

        if (strType == "ldConnector")
        {
            clickEvent.lines.Add(lineRenderer);
            Connect(clickEvent.Connector);
        }
        else if (strType == "transporter")
        {
            Connect(clickEvent);
        }
    }

    MapClickEvent FindTargetClickEvent(Vector3 targetPos)
    {
        Structure othObj = null;
        if (strType == "ldConnector")
        {
            othObj = DataManager.instance.CellObjFind(targetPos, ldConnector.isInHostMap);
        }
        else if (strType == "transporter")
        {
            othObj = DataManager.instance.CellObjFind(targetPos, transporter.isInHostMap);
        }

        return othObj.GetComponent<MapClickEvent>();
    }

    public bool RemoveRenderer(MapClickEvent clickEvent)
    {
        if (strType == "ldConnector")
        {
            for (int i = 0; i < lines.Count; i++)
            {
                for (int j = 0; j < clickEvent.lines.Count; j++)
                {
                    if (lines[i] == clickEvent.lines[j])
                    {
                        RendererRemoveServerRpc(clickEvent.transform.position, i);
                        return true;
                    }
                }
            }
        }
        else if (strType == "transporter")
        {
            if (transporter.takeBuild == clickEvent.transporter)
            {
                RendererRemoveServerRpc(clickEvent.transform.position, 0);
                return true;
            }
        }
        
        return false;
    }


    [ServerRpc(RequireOwnership = false)]
    void RendererRemoveServerRpc(Vector3 targetPos, int lineIndex)
    {
        RendererRemoveClientRpc(targetPos, lineIndex);
    }

    [ClientRpc]
    void RendererRemoveClientRpc(Vector3 targetPos, int lineIndex)
    {
        MapClickEvent clickEvent = FindTargetClickEvent(targetPos);

        LineRenderer line = lines[lineIndex];
        clickEvent.lines.Remove(line);
        lines.Remove(line);
        Destroy(line.gameObject);
        if (strType == "ldConnector")
        {
            Disconnect(clickEvent.Connector);
        }
        else if (strType == "transporter")
        {
            Disconnect();
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

    public void Connect(MapClickEvent othClickEvent)
    {
        if (transporter != null && othClickEvent.TryGetComponent(out Transporter othTransporter))
            transporter.TakeBuildSet(othTransporter);
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

    public void Disconnect()
    {
        if (transporter != null)
            transporter.TakeBuildReset();
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
