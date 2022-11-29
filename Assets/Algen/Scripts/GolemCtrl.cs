using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolemCtrl : MonsterAi
{
    [SerializeField]
    private GameObject[] golemAttackFX;

    GameObject golemFX;
    protected override void RandomAttackNum(int attackNum, Transform targetTr)
    {
        attackState = AttackState.Attacking;

        attackMotion = Random.Range(0, attackNum);

        if (attackMotion == 0)
        {
            golemFX = Instantiate(golemAttackFX[0], new Vector2(targetTr.position.x, targetTr.position.y - 0.5f), targetTr.rotation);
        }
        else if (attackMotion == 1)
        {
            golemFX = Instantiate(golemAttackFX[1], new Vector2(this.transform.position.x, this.transform.position.y), this.transform.rotation);
            golemFX.transform.SetParent(this.transform, true);
        }
        else if (attackMotion == 2)
        {
             golemFX = Instantiate(golemAttackFX[2], new Vector2(this.transform.position.x, this.transform.position.y), this.transform.rotation);
        }

        animator.SetBool("isAttack", true);

        animator.SetFloat("attackMotion", attackMotion);
        animator.Play("Attack", -1, 0);
        golemFX.GetComponentInChildren<GolemFXCtrl>().GetTarget(targetTr.position, attackMotion);
    }

    protected override void AttackEnd(string str)
    {
        if (str == "false")
        {
            animator.SetBool("isAttack", false);
            attackState = AttackState.AttackEnd;

            StartCoroutine("AttackDelay");
        }
    }
}
