using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class ViewOnOff : MonoBehaviour
{
    [SerializeField]
    Structure structure;
    string structureName;

    void Start()
    {
        structureName = structure.buildName;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out PreBuildingImg pre))
        {
            if (structureName == "Overclock" && pre.structure.structureData.factoryName == "Overclock")
            {
                if(pre.isEnergyUse)
                    structure.Focused();
            }
            else if (structureName == "RepairTower" && pre.structure.structureData.factoryName == "RepairTower")
            {
                structure.Focused();
            }
            else if (structureName == "SunTower" && pre.structure.structureData.factoryName == "SunTower")
            {
                structure.Focused();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        structure.DisableFocused();
    }
}
