using System;
using System.Collections;
using System.Collections.Generic;
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

    protected override void Update()
    {
        base.Update();
    }

    protected override void CheckNearObj(Vector3 startVec, Vector3 endVec, int index, Action<GameObject> callback)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(this.transform.position + startVec, endVec, 1f);

        if (nearPos[index] != null)
            nearPos[index] = this.transform.position + startVec + endVec;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hitCollider = hits[i].collider;
            if (hitCollider.CompareTag("Factory") && hitCollider.GetComponent<Structure>().isSetBuildingOk &&
                hits[i].collider.gameObject != this.gameObject)
            {
                nearObj[index] = hits[i].collider.gameObject;
                callback(hitCollider.gameObject);
                break;
            }
        }
    }

    public void SpawnUnitCheck(GameObject unit)
    {
        if (IsServer)
        {
            unit.TryGetComponent(out NetworkObject netObj);
            if (!netObj.IsSpawned) unit.GetComponent<NetworkObject>().Spawn(true);
        }

        UnitAi unitAi = unit.GetComponent<UnitAi>();
        unitAi.PortalUnitOutFuncServerRpc(isInHostMap, this.transform.position);
        UnitSpawnPosFind();
        unitAi.MovePosSetServerRpc(spawnPos, 0, true);
    }

    public void UnitSpawnPosFind()
    {
        if (!isSetPos)
        {
            for (int i = 0; i < nearPos.Length; i++)
            {
                if (nearObj[i] != null)
                    continue;
                else
                {
                    spawnPos = nearPos[i];
                    break;
                }
            }
        }
    }

    public void UnitSpawnPosSet(Vector2 _spawnPos)
    {
        isSetPos = true;
        spawnPos = _spawnPos;
    }

    public override void DestroyLineRenderer()
    {
        base.DestroyLineRenderer();
        isSetPos = false;
    }
}
