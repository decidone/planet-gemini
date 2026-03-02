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

    protected override void CheckNearObj(int index, Action<Structure> callback)
    {
        int nearX = (int)transform.position.x + twoDirections[index, 0];
        int nearY = (int)transform.position.y + twoDirections[index, 1];
        Cell cell = GameManager.instance.GetCellDataFromPosWithoutMap(nearX, nearY);
        if (cell == null)
            return;

        if (nearPos[index] != null)
            nearPos[index] = new Vector2(nearX, nearY);

        Structure obj = cell.structure;
        if (obj != null)
        {
            nearObj[index] = obj;
            callback(obj);
        }
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
