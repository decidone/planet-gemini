using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterListSO", menuName = "SOList/MonsterListSO")]
public class UnitListSO : ScriptableObject
{
    public List<GameObject> weakMonsterList;
    public List<GameObject> normalMonsterList;
    public List<GameObject> strongMonsterList;
    public List<GameObject> guardian;

    public List<GameObject> userUnitList;
}
