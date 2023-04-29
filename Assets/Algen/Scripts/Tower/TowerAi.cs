using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerAi : MonoBehaviour
{
    public enum TowerState
    {
        Waiting,
        Attack,
        AttackDelay,
        //Die
    }

    [SerializeField]
    protected TowerData towerData;
    protected TowerData TowerData { set { towerData = value; } }

    // 유닛 상태 관련
    [HideInInspector]
    public TowerState towerState = TowerState.Waiting;
    public GameObject unitCanvers = null;

    [SerializeField]
    protected Animator animator;

    // HpBar 관련
    public Image hpBar;
    [HideInInspector]
    public float hp = 200.0f;
    [HideInInspector]
    public bool isDie = false;

    // Repair 관련
    [HideInInspector]
    public bool isRepair = false;
    public Image repairBar;
    float repairGauge = 0.0f;
    float maxRepairGauge = 100.0f;

    protected CircleCollider2D circle2D = null;
    protected CapsuleCollider2D capsule2D = null;

    public bool isPreBuilding = false;

    private void Awake()
    {
        circle2D = GetComponent<CircleCollider2D>();
        capsule2D = GetComponent<CapsuleCollider2D>();
    }
    // Start is called before the first frame update
    void Start()
    {
        circle2D.radius = towerData.ColliderRadius;
        hp = towerData.MaxHp;
        repairBar.enabled = false;
        hpBar.fillAmount = hp / towerData.MaxHp;
        repairBar.fillAmount = repairGauge / maxRepairGauge;
    }

    //// Update is called once per frame
    //void Update()
    //{

    //}

    public void TakeDamage(float damage)
    {
        if (hp <= 0f)
            return;

        hp -= damage;
        hpBar.fillAmount = hp / towerData.MaxHp;

        if (hp <= 0f)
        {
            hp = 0f;
            DieFunc();
        }
    }

    protected virtual void DieFunc()
    {
 
    }

    public void HealFunc(float heal)
    {
        if (hp + heal > towerData.MaxHp)
            hp = towerData.MaxHp;
        else        
            hp += heal;

        hpBar.fillAmount = hp / towerData.MaxHp;
    }

    public void RepairSet(bool repair)
    {
        isRepair = repair;
        repairBar.enabled = repair;
    }

    protected void RepairFunc()
    {
        repairGauge += 10.0f * Time.deltaTime;

        repairBar.fillAmount = repairGauge / maxRepairGauge;

        if (repairGauge  >= maxRepairGauge)
        {
            RepairEnd();
        }
    }

    void RepairEnd()
    {
        hpBar.enabled = true;

        hp = 100.0f;
        hpBar.fillAmount = hp / towerData.MaxHp;

        repairBar.enabled = false;
        repairGauge = 0.0f;

        isDie = false;

        capsule2D.enabled = true;
        circle2D.enabled = true;

        animator.SetBool("isDie", false);
    }

    public void DisableColliders()
    {
        capsule2D.enabled = false;
        circle2D.enabled = false;
    }

    // 콜라이더 켜기
    public void EnableColliders()
    {
        capsule2D.enabled = true;
        circle2D.enabled = true;
    }
}
