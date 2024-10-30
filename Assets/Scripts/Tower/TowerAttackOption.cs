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
    bool poisonTrueAttack;  // 추가 고정 데미지
    [SerializeField]
    float poisonDamage;     // 독 데미지
    [SerializeField]
    bool ignoreDdefense;    // 방어력 무시
    [SerializeField]
    float ignorePercent;     // 독 데미지

    public void TowerAttackFxSet(TowerAttackFx fx)
    {
        if (slowDebuff)
        {
            fx.SlowDebuffSet(slowTime);
        }
        else if (poisonTrueAttack)
        {
            fx.PoisonTrueAttackSet(poisonDamage);
        }
        else if (ignoreDdefense)
        {
            fx.IgnoreDdefenseSet(ignorePercent);
        }
    }
}
