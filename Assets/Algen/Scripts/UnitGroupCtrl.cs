using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitGroupCtrl : MonoBehaviour
{
    public List<GameObject> unitList = new List<GameObject>();

    private void OnEnable()
    {
        // 이벤트 핸들러 등록
        UnitDrag.removeUnit += ClearUnitList;
        UnitDrag.addUnit += AddUnitList;
        UnitDrag.targetSet += TargetSetPos;
        UnitDrag.patrolSet += PatrolSetPos;
    }

    private void OnDisable()
    {
        // 이벤트 핸들러 해제
        UnitDrag.removeUnit -= ClearUnitList;
        UnitDrag.addUnit -= AddUnitList;
        UnitDrag.targetSet -= TargetSetPos;
        UnitDrag.patrolSet -= PatrolSetPos;
    }

    private void ClearUnitList()
    {
        unitList.Clear();
    }

    private void AddUnitList(GameObject obj)
    {
        unitList.Add(obj);
    }

    private void TargetSetPos(Vector2 targetPos)
    {
        foreach (GameObject obj in unitList)
        {
            obj.GetComponent<UnitAi>().MovePosSet(targetPos);
        }
    }

    private void PatrolSetPos(Vector2 patrolPos)
    {
        foreach (GameObject obj in unitList)
        {
            obj.GetComponent<UnitAi>().PatrolPosSet(patrolPos);
        }
    }
}
