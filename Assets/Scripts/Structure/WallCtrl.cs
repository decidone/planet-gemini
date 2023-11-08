using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallCtrl : Structure
{
    protected override void Awake()
    {
        base.Awake();
    }

    protected virtual void Update()
    {
        if (!removeState)
        {
            if (isRuin && isRepair)
            {
                RepairFunc(false);
            }
            else if (isPreBuilding && isSetBuildingOk && !isRuin)
            {
                RepairFunc(true);
            }
        }
    }
}
