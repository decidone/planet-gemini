using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerAttackOption : MonoBehaviour
{
    [SerializeField]
    bool slowDebuff;        // 공속 느리게
    [SerializeField]
    float slowTime;         // 디버프 시간
    [SerializeField]
    bool poisonTrueAttack;  // 독 공격
    [SerializeField]
    float poisonTime;         // 디버프 시간
    [SerializeField]
    bool ignoreDdefense;    // 방어력 무시
    [SerializeField]
    float ignorePercent;    // 방어력 무시 퍼센트

    public void TowerAttackFxSet(TowerAttackFx fx)
    {
        if (slowDebuff)
        {
            fx.SlowDebuffSet(slowTime);
        }
        else if (poisonTrueAttack)
        {
            fx.PoisonTrueAttackSet(poisonTime);
        }
        else if (ignoreDdefense)
        {
            fx.IgnoreDdefenseSet(ignorePercent);
        }
    }
}
