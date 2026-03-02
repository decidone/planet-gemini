using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Unity.Netcode;

// UTF-8 설정
public class UnitGroupCtrl : MonoBehaviour
{
    public List<GameObject> unitList = new List<GameObject>();
    public List<Vector3> unitVecList = new List<Vector3>();
    Vector3 groupCenter = Vector3.zero;
    float radius = 0;

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
        foreach (GameObject obj in unitList)
        {
            obj.GetComponent<UnitAi>().UnitSelImg(false);
        }
        unitList.Clear();
    }

    private void AddUnitList(GameObject obj)
    {
        if (obj.GetComponentInParent<UnitAi>())
        {
            unitList.Add(obj);
            obj.gameObject.GetComponent<UnitAi>().UnitSelImg(true);
        }
        CalculateGroupCenter();
    }

    private void TargetSetPos(Vector3 targetPos, bool isAttack, bool playerUnitPortalIn)
    {
        float totalDiameter = 0.7f * unitList.Count;

        float largeCircleRadius = totalDiameter / (2 * Mathf.PI);
        float delta = Mathf.Max(0f, largeCircleRadius);

        float minDiameter = (delta) / 2;

        foreach (GameObject obj in unitList)
        {
            UnitAi unti = obj.GetComponent<UnitAi>();
            unti.PortalUnitInServerRpc(playerUnitPortalIn);
            unti.MovePosSetServerRpc(targetPos, minDiameter, isAttack);
        }
    }

    private void PatrolSetPos(Vector3 patrolPos)
    {
        if(unitList.Count > 1)
        {
            for (int i = 0; i < unitList.Count; i++)
            {
                Vector3 movePosition = patrolPos + unitVecList[i];
                unitList[i].GetComponent<UnitAi>().PatrolPosSetServerRpc(movePosition);
            }
        }
        else if (unitList.Count == 1)
        {
            unitList[0].GetComponent<UnitAi>().PatrolPosSetServerRpc(patrolPos);
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
            unitList[i].GetComponent<UnitAi>().HoldFuncServerRpc();
        }
    }

    void MonsterTargerSet(GameObject obj)
    {
        NetworkObject networkObject = obj.GetComponent<NetworkObject>();
        for (int i = 0; i < unitList.Count; i++)
        {
            unitList[i].GetComponent<UnitAi>().TargetSetServerRpc(networkObject);
        }
    }
}
