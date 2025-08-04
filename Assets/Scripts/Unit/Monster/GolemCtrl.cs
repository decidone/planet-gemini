using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
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

    protected override void AttackEnd()
    {
        animator.SetBool("isAttack", false);
        animator.SetBool("isMove", false);
        if (IsServer)
            AnimSetFloat(targetVec, false);
        attackState = AttackState.Waiting;
    }

    public void FXSpawn()
    {
        if (getTargetTr == null)
            return;

        //golemFX = Instantiate(golemAttackFX, new Vector2(getTargetTr.position.x, getTargetTr.position.y), getTargetTr.rotation);
        NetworkObject golemFX = networkObjectPool.GetNetworkObject(golemAttackFX, new Vector2(getTargetTr.position.x, getTargetTr.position.y), Quaternion.identity);
        if (!golemFX.IsSpawned) golemFX.Spawn(true);
        golemFX.GetComponentInChildren<GolemFXCtrl>().TargetPosAndDamage(damage);
    }
}
