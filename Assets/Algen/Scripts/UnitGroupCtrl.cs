using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitGroupCtrl : MonoBehaviour
{
    public List<GameObject> unitList = new List<GameObject>();
    public List<Vector3> unitVecList = new List<Vector3>();

    Vector3 Groupcenter = Vector3.zero;
    [SerializeField]
    float radius = 0;

    private void OnEnable()
    {
        // 이벤트 핸들러 등록
        UnitDrag.removeUnit += ClearUnitList;
        UnitDrag.addUnit += AddUnitList;
        UnitDrag.targetSet += TargetSetPos;
        UnitDrag.patrolSet += PatrolSetPos;
        UnitDrag.groupCenterSet += CalculateGroupCenter;
    }

    private void OnDisable()
    {
        // 이벤트 핸들러 해제
        UnitDrag.removeUnit -= ClearUnitList;
        UnitDrag.addUnit -= AddUnitList;
        UnitDrag.targetSet -= TargetSetPos;
        UnitDrag.patrolSet -= PatrolSetPos;
        UnitDrag.groupCenterSet -= CalculateGroupCenter;
    }

    private void ClearUnitList()
    {
        unitList.Clear();
    }

    private void AddUnitList(GameObject obj)
    {
        if(obj.GetComponent<UnitAi>())
        {
            unitList.Add(obj);
        }
    }

    private void TargetSetPos(Vector3 targetPos)
    {
        float totalDiameter = 1 * unitList.Count;
        float largeCircleRadius = totalDiameter / (2 * Mathf.PI);

        //float sumRadii = unitList.Count * 0.5f;
        //float delta = Mathf.Max(0f, sumRadii - largeCircleRadius);
        float delta = Mathf.Max(0f, largeCircleRadius);

        //float minDiameter = (sumRadii + delta) / 2;
        float minDiameter = (delta + 0.6f) / 2;

        foreach (GameObject obj in unitList)
        {
            obj.GetComponent<UnitAi>().MovePosSet(targetPos, minDiameter);
        }
    }

    private void PatrolSetPos(Vector3 patrolPos)
    {
        for (int i = 0; i < unitList.Count; i++)
        {
            Vector3 movePosition = patrolPos + unitVecList[i];
            unitList[i].GetComponent<UnitAi>().PatrolPosSet(movePosition);
        }
    }
    
    void CalculateGroupCenter()
    {
        int count = unitList.Count;

        Groupcenter = Vector3.zero;

        foreach (GameObject unit in unitList)
        {
            Groupcenter += unit.transform.position;
        }

        if (count > 0)
        {
            Groupcenter /= count;
        }

        radius = 0;

        foreach (GameObject unit in unitList)
        {
            float radChack = Vector3.Distance(unit.transform.position, Groupcenter);
            if (radius < radChack)
                radius = radChack;
        }

        unitVecList.Clear();

        foreach (GameObject unit in unitList)
        {
            Vector3 direction = unit.transform.position - Groupcenter;
            unitVecList.Add(direction);
        }
    }
}
