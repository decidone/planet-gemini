using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
public class GolemCtrl : MonsterAi
{
    [SerializeField]
    private GameObject golemAttackFX;

    GameObject golemFX;
    Transform getTargetTr;

    protected override void AttackSet(Transform targetTr)
    {
        base.AttackSet(targetTr);
        getTargetTr = targetTr;
    }

    public void FXSpawn()
    {
        if (getTargetTr == null)
            return;

        golemFX = Instantiate(golemAttackFX, new Vector2(getTargetTr.position.x, getTargetTr.position.y), getTargetTr.rotation);
        golemFX.GetComponentInChildren<GolemFXCtrl>().TargetPosAndDamage(unitCommonData.Damage);
    }
}
