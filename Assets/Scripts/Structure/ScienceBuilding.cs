using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ScienceBuilding : PortalObj
{
    Vector3 pos; 
    protected override void Start()
    {
        base.Start();
        SciBuildingRepairEnd();
        pos = new Vector3(transform.position.x - 0.5f, transform.position.y - 0.5f, 0);
        MapDataSaveClientRpc(pos);
    }

    protected override void OnClientConnectedCallback(ulong clientId)
    {
        base.OnClientConnectedCallback(clientId);
    }

    [ClientRpc]
    protected override void RepairGaugeClientRpc(bool preBuilding, bool setBuildingOk, bool destroy, float hpSet, float repairGaugeSet, float destroyTimerSet)
    {
        StructureStateSet(preBuilding, setBuildingOk, destroy, hpSet, repairGaugeSet, destroyTimerSet);
        SciBuildingRepairEnd();
    }

    public void SetPortal(bool hostMap)
    {
        isInHostMap = hostMap;
    }

    void SciBuildingRepairEnd()
    {
        GameManager gameManager = GameManager.instance;
        gameManager.SciBuildingSet(isInHostMap);
        buildingIndex = BuildingList.instance.FindBuildingListIndex("BuildingScienceBuilding");
        PortalSciManager.instance.UISet();
    }

    public override Dictionary<Item, int> PopUpItemCheck()
    {
        return null;
    }

    public override void IncreasedStructureCheck() { }
}
