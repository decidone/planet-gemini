using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class LDConnector : Structure
{
    public EnergyGroupConnector connector;
    [SerializeField]
    SpriteRenderer view;
    [HideInInspector]
    public bool isBuildDone;
    bool isPlaced;
    GameManager gameManager;
    PreBuilding preBuilding;
    Structure preBuildingStr;
    bool preBuildingCheck;
    [HideInInspector]
    public MapClickEvent clickEvent;

    protected override void Awake()
    {
        base.Awake();
        isBuildDone = false;
    }

    protected void Start()
    {
        isPlaced = false;
        clickEvent = GetComponent<MapClickEvent>();
        gameManager = GameManager.instance;
        preBuilding = PreBuilding.instance;
    }

    protected override void Update()
    {
        base.Update();

        if (!isPlaced)
        {
            if (isSetBuildingOk)
            {
                view.enabled = false;
                isPlaced = true;
            }
        }
        if (gameManager.focusedStructure == null)
        {
            if (preBuilding.isBuildingOn && !removeState)
            {
                if (!preBuildingCheck)
                {
                    preBuildingCheck = true;
                    if (preBuilding.isEnergyUse || preBuilding.isEnergyStr)
                    {
                        view.enabled = true;
                    }
                }
            }
            else
            {
                if (preBuildingCheck)
                {
                    preBuildingCheck = false;
                    view.enabled = false;
                }
            }
        }
        if (!isPreBuilding)
        {
            if (!isBuildDone)
            {
                connector.Init();
                isBuildDone = true;
            }
        }
    }

    protected override void OnClientConnectedCallback(ulong clientId)
    {
        base.OnClientConnectedCallback(clientId);
        LDConnectedSetServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    void LDConnectedSetServerRpc()
    {
        for (int i = 0; i < clickEvent.lines.Count; i++)
        {
            LineRenderer lineRenderer = clickEvent.lines[i];
            MapLine lineProps = lineRenderer.GetComponent<MapLine>();
            LDConnectedSetClientRpc(lineProps.lineTarget.transform.position);
        }
    }

    [ClientRpc]
    void LDConnectedSetClientRpc(Vector3 pos)
    {
        if (IsServer)
            return;

        StartCoroutine(SetInvoke(pos));
    }

    IEnumerator SetInvoke(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        Map map;
        if (isInHostMap)
            map = gameManager.hostMap;
        else
            map = gameManager.clientMap;

        Cell cell = map.GetCellDataFromPos(x, y);
        while (cell.structure == null) 
        {
            yield return null;
        }

        GameObject findObj = cell.structure;
        if (findObj != null && findObj.TryGetComponent(out LDConnector othLDConnector))
        {
            if (TryGetComponent(out MapClickEvent mapClick) && othLDConnector.TryGetComponent(out MapClickEvent othMapClick))
            {
                mapClick.GameStartSetRenderer(othMapClick);
            }
        }
    }

    public override void Focused()
    {
        if (connector.group != null)
        {
            connector.group.TerritoryViewOn();
        }
    }

    public override void DisableFocused()
    {
        if (connector.group != null)
        {
            connector.group.TerritoryViewOff();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public override void RemoveObjServerRpc()
    {
        //여기서 건물 철거 전 처리(삭제가 아니여도 비활성화가 필요하니 그거 생각해서 만들 것)
               DisableFocused();
 connector.RemoveFromGroup();
        clickEvent.RemoveAllLines();
        base.RemoveObjServerRpc();
    }

    public override StructureSaveData SaveData()
    {
        StructureSaveData data = base.SaveData();

        for (int i = 0; i < clickEvent.lines.Count; i++)
        {
            LineRenderer lineRenderer = clickEvent.lines[i];
            MapLine lineProps = lineRenderer.GetComponent<MapLine>();
            data.connectedStrPos.Add(Vector3Extensions.FromVector3(lineProps.lineTarget.gameObject.transform.position));
        }

        return data;
    }
    public override (bool, bool, bool, EnergyGroup, float) PopUpEnergyCheck()
    {
        if (connector != null && connector.group != null)
        {
            return (energyUse, isEnergyStr, false, connector.group, energyConsumption);
        }

        return (false, false, false, null, 0);
    }
}
