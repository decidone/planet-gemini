using System;
using Unity.Netcode;
using UnityEngine;

public class PortalUnitOut : PortalObj
{
    public Vector2[] nearPos = new Vector2[8];
    public Vector2 spawnPos;
    bool isSetPos;

    protected override void Start()
    {
        base.Start();
        isPortalBuild = true;
        isSetPos = false;
        isGetLine = true;
    }

    public void SpawnUnitCheck(GameObject unit)
    {
        if (IsServer)
        {
            unit.TryGetComponent(out NetworkObject netObj);
            if (!netObj.IsSpawned) netObj.Spawn(true);
        }

        UnitAi unitAi = unit.GetComponent<UnitAi>();
        unitAi.PortalUnitOutFuncServerRpc(isInHostMap, this.transform.position);
    }
}
