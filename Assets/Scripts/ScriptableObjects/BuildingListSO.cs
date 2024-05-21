using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingListSO", menuName = "SOList/BuildingListSO")]
public class BuildingListSO : ScriptableObject
{
    public List<Building> buildingSOList;
}
