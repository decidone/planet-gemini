using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ScienceBuilding : PortalObj
{
    Vector3 pos;

    protected override void Update() { }

    protected override void Start()
    {
        base.Start();
        SciBuildingRepairEnd();
        pos = new Vector3(transform.position.x - 0.5f, transform.position.y - 0.5f, 0);
        MapDataSaveClientRpc(pos);

        if(hp == maxHp)
            unitCanvas.SetActive(false);
    }

    public override void OnClientConnectedCallback()
    {
        ClientConnectSyncServerRpc();
        PortalObjConnectServerRpc();
    }

    //protected override void OnClientConnectedCallback(ulong clientId)
    //{
    //    ClientConnectSyncServerRpc();
    //    PortalObjConnectServerRpc();
    //}

    public override void NearStrBuilt()
    {
        // 건물을 지었을 때나 근처에 새로운 건물이 지어졌을 때 동작
        //CheckPos();
    }

    public override void StructureStateSet(bool preBuilding, bool destroy, float hpSet, float repairGaugeSet, float destroyTimerSet)
    {
        base.StructureStateSet(preBuilding, destroy, hpSet, repairGaugeSet, destroyTimerSet);

        SciBuildingRepairEnd();
    }

    public void SetPortal(bool hostMap)
    {
        isInHostMap = hostMap;
    }

    void SciBuildingRepairEnd()
    {
        buildingIndex = BuildingList.instance.FindBuildingListIndex("BuildingScienceBuilding");
        PortalSciManager.instance.UISet();
    }

    public override Dictionary<Item, int> PopUpItemCheck()
    {
        return null;
    }

    public override void IncreasedStructureCheck() { }

    protected override void DieFuncServer()
    {
        gameManager.SetGameOverUI();
    }

    public override void ColliderTriggerOnOff(bool isOn)
    {
        col.isTrigger = true;
    }
}
