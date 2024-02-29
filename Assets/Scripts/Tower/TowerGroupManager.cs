using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerGroupManager : MonoBehaviour
{
    public GameObject FireTowerGp;
    public GameObject PoisonTowerGp;
    public GameObject IceTowerGp;
    public GameObject SunTowerGp;
    public GameObject StormTowerGp;
    public GameObject RepairTowerGp;

    public GameObject TowerGroupSet(string twName)
    {
        switch (twName)
        {
            case "FireTower":
                return FireTowerGp;
            case "PoisonTower":
                return PoisonTowerGp;
            case "IceTower":
                return IceTowerGp;
            case "SunTower":
                return SunTowerGp;
            case "StormTower":
                return StormTowerGp;
            case "RepairTower":
                return RepairTowerGp;
            default:
                return null;
        }
    }
}
