using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CorrosionDrone : UnitAi
{
    [SerializeField] float debuffTimer;
    [SerializeField] float debuffInterval;
    [SerializeField] float debuffPer;

    [SerializeField] int debuffAmount;
    [SerializeField] List<MonsterAi> debuffTargetList = new List<MonsterAi>();
     
    bool isDebuffState = false;

    protected override void Awake()
    {
        base.Awake();
        int mask = (1 << LayerMask.NameToLayer("Monster"));

        contactFilter.SetLayerMask(mask);
        contactFilter.useLayerMask = true;
    }

    protected override void UnitAiCtrl()
    {
        switch (aIState)
        {
            case AIState.AI_Move:
                MoveFunc();
                break;
            case AIState.AI_Patrol:
                PatrolFunc();
                break;
            case AIState.AI_Attack:
                break;
            case AIState.AI_NormalTrace:
                {
                    if (aggroTarget)
                        AttackCheck();
                    NormalTrace();
                }
                break;
        }
    }

    public override void SearchObjectsInRange()
    {
        int hitCount = Physics2D.OverlapCircle(
            tr.position,
            unitCommonData.ColliderRadius,
            contactFilter,
            targetColls
        );

        if (hitCount == 0)
        {
            aggroTarget = null;
            if(targetList.Count > 0)
            {
                targetList.Clear();
            }
            return;
        }

        targetList.Clear();

        for (int i = 0; i < hitCount; i++)
        {
            GameObject target = targetColls[i].gameObject;
            targetList.Add(target);
        }

        if(targetList.Count > 0)
            DebuffFunc();
        else if (isDebuffState)
        {
            animator.SetBool("isAttack", false);
            isDebuffState = false;
        }

        AttackTargetCheck();
    }

    void DebuffFunc()
    {
        for (int i = debuffTargetList.Count - 1; i >= 0; i--)
        {
            MonsterAi m = debuffTargetList[i];

            if (!m || !m.isDebuffed || !targetList.Contains(m.gameObject))
            {
                if (m) m.isDebuffed = false;
                debuffTargetList.RemoveAt(i);
            }
        }

        int needCount = debuffAmount - debuffTargetList.Count;
        if (needCount <= 0)
            return;

        List<MonsterAi> monstersList = new List<MonsterAi>(); // 정렬용 리스트

        foreach (GameObject obj in targetList)
        {
            if (!obj) continue;

            if (obj.TryGetComponent(out MonsterAi ai) && !ai.isDebuffed)
                monstersList.Add(ai);
        }

        monstersList.Sort((a, b) =>
            ((Vector2)(a.transform.position - tr.position)).sqrMagnitude
            .CompareTo(
            ((Vector2)(b.transform.position - tr.position)).sqrMagnitude
        ));

        for (int i = 0; i < Mathf.Min(needCount, monstersList.Count); i++)
        {
            debuffTargetList.Add(monstersList[i]);
        }

        foreach (MonsterAi monster in debuffTargetList)
        {
            if (monster)
            {
                monster.RefreshDebuffServerRpc(1, debuffPer);    // 서버, 클라이언트 상관없이 디버프 띄워주는데 데미지 계산은 서버 디버프 유무로만 계산
            }
        }

        if (debuffTargetList.Count > 0 && !isDebuffState)
        { 
            animator.Play("Attack", -1, 0);
            animator.SetBool("isAttack", true);
            isDebuffState = true;
        }
    }
}
