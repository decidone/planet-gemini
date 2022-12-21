using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolemCtrl : MonsterAi
{
    [SerializeField]
    private GameObject[] golemAttackFX;

    GameObject golemFX;
    Transform getTargetTr;
    Vector3 targetVec = Vector3.zero;
    Vector3 nextStep = Vector3.zero;
    float speed;
    bool checkTarget = false;
    protected override void RandomAttackNum(int attackNum, Transform targetTr)
    {
        attackState = AttackState.Attacking;

        attackMotion = Random.Range(0, attackNum);
        getTargetTr = targetTr;
        animator.SetBool("isAttack", true);
        animator.SetFloat("attackMotion", attackMotion);
        animator.Play("Attack", -1, 0);
    }

    void AttackEnd(string str)
    {
        if (str == "false")
        {
            animator.SetBool("isAttack", false);
            attackState = AttackState.AttackEnd;
            StartCoroutine("AttackDelay");
            checkTarget = false;
        }
    }

    protected override void AttackMove()
    {
        if (checkTarget == false)
        {
            if (getTargetTr != null)
            {
                targetVec = (new Vector3(getTargetTr.position.x, getTargetTr.position.y, 0) - this.transform.position).normalized;
            }
            checkTarget = true;
        }
        if (attackMotion == 1)
        {
            transform.position += targetVec * 15 * Time.fixedDeltaTime;
        }
    }
    public void FXSpawn()
    {
        if (attackMotion == 0)
        {
            golemFX = Instantiate(golemAttackFX[0], new Vector2(getTargetTr.position.x, getTargetTr.position.y - 0.5f), getTargetTr.rotation);
        }
        else if (attackMotion == 1)
        {
            golemFX = Instantiate(golemAttackFX[1], new Vector2(this.transform.position.x, this.transform.position.y), this.transform.rotation);
            golemFX.transform.SetParent(this.transform, true);
        }
        else if (attackMotion == 2)
        {
            Vector3 dir = getTargetTr.position - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            golemFX = Instantiate(golemAttackFX[2], new Vector2(this.transform.position.x, this.transform.position.y), this.transform.rotation);
            if (Quaternion.AngleAxis(angle + 180, Vector3.forward).z < 0)
                golemFX.transform.rotation = Quaternion.AngleAxis(angle + 180, Vector3.forward);
            else
                golemFX.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        golemFX.GetComponentInChildren<GolemFXCtrl>().GetTarget(getTargetTr.position, attackMotion);
    }
}

