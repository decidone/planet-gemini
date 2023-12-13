using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyRepeater : Structure
{
    void Update()
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
