using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class UnitGroupCtrl : MonoBehaviour
{
    public List<GameObject> unitList = new List<GameObject>();
    public List<Vector3> unitVecList = new List<Vector3>();
    Vector3 groupCenter = Vector3.zero;
    float radius = 0;

    //Seeker seeker;

    //protected Coroutine checkPathCoroutine; // 실행 중인 코루틴을 저장하는 변수

    //private void Awake()
    //{
    //    seeker = GetComponent<Seeker>();
    //}

    private void OnEnable()
    {
        // 이벤트 핸들러 등록
        UnitDrag.removeUnit += ClearUnitList;
        UnitDrag.addUnit += AddUnitList;
        UnitDrag.targetSet += TargetSetPos;
        UnitDrag.patrolSet += PatrolSetPos;
        UnitDrag.groupCenterSet += CalculateGroupCenter;
        UnitDrag.unitHoldSet += HoldSet;
        UnitDrag.monsterTargetSet += MonsterTargerSet;
    }

    private void OnDisable()
    {
        // 이벤트 핸들러 해제
        UnitDrag.removeUnit -= ClearUnitList;
        UnitDrag.addUnit -= AddUnitList;
        UnitDrag.targetSet -= TargetSetPos;
        UnitDrag.patrolSet -= PatrolSetPos;
        UnitDrag.groupCenterSet -= CalculateGroupCenter;
        UnitDrag.unitHoldSet -= HoldSet;
        UnitDrag.monsterTargetSet -= MonsterTargerSet;
    }

    private void ClearUnitList()
    {
        foreach(GameObject obj in unitList)
        {
            obj.GetComponent<UnitAi>().UnitSelImg(false);
        }
        unitList.Clear();
    }

    private void AddUnitList(GameObject obj)
    {
        if(obj.GetComponentInParent<UnitAi>())
        {
            unitList.Add(obj);
            obj.gameObject.GetComponent<UnitAi>().UnitSelImg(true);
        }
        CalculateGroupCenter();
    }

    private void TargetSetPos(Vector3 targetPos, bool isAttack)
    {
        float totalDiameter = 0.7f * unitList.Count;

        float largeCircleRadius = totalDiameter / (2 * Mathf.PI);
        float delta = Mathf.Max(0f, largeCircleRadius);

        float minDiameter = (delta) / 2;

        foreach (GameObject obj in unitList)
        {
            obj.GetComponent<UnitAi>().MovePosSet(targetPos, minDiameter, isAttack);
        }
    }

    private void PatrolSetPos(Vector3 patrolPos)
    {
        if(unitList.Count > 1)
        {
            for (int i = 0; i < unitList.Count; i++)
            {
                Vector3 movePosition = patrolPos + unitVecList[i];
                unitList[i].GetComponent<UnitAi>().PatrolPosSet(movePosition);
            }
        }
        else if (unitList.Count == 1)
        {
            unitList[0].GetComponent<UnitAi>().PatrolPosSet(patrolPos);
        }
    }

    private void CalculateGroupCenter()
    {
        int count = unitList.Count;

        if (count > 1)
        {
            groupCenter = Vector3.zero;

            foreach (GameObject unit in unitList)
            {
                groupCenter += unit.transform.position;
            }

            if (count > 0)
            {
                groupCenter /= count;
            }

            radius = 0;

            foreach (GameObject unit in unitList)
            {
                float radChack = Vector3.Distance(unit.transform.position, groupCenter);
                if (radius < radChack)
                    radius = radChack;
            }

            unitVecList.Clear();

            foreach (GameObject unit in unitList)
            {
                Vector3 direction = unit.transform.position - groupCenter;
                unitVecList.Add(direction);
            }
        }
        else if (count == 1)
        {
            groupCenter = unitList[0].transform.position;
        }
    }

    public void DieUnitCheck(GameObject obj)
    {
        if (unitList.Contains(obj))
        {
            unitList.Remove(obj);
        }
    }

    private void HoldSet()
    {
        for (int i = 0; i < unitList.Count; i++)
        {
            unitList[i].GetComponent<UnitAi>().HoldFunc();
        }
    }

    void MonsterTargerSet(GameObject obj)
    {
        for (int i = 0; i < unitList.Count; i++)
        {
            unitList[i].GetComponent<UnitAi>().TargetSet(obj);
        }
    }
}
