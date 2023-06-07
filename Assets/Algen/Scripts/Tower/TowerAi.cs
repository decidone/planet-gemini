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
    public bool isRuin = false;

    // Repair 관련
    //[HideInInspector]
    public bool isRepair = false;
    public Image repairBar;
    public float repairGauge = 0.0f;

    protected CircleCollider2D circle2D = null;
    protected CapsuleCollider2D capsule2D = null;

    public bool isPreBuilding = false;
    public bool isSetBuildingOk = false;

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
        hpBar.fillAmount = hp / towerData.MaxHp;
        repairBar.fillAmount = repairGauge / towerData.MaxRepairGauge;
    }
    protected virtual void Update()
    {
        if (isPreBuilding && isSetBuildingOk && !isRuin)
        {
            RepairFunc(true);
        }
    }

    public void TakeDamage(float damage)
    {
        if (!isPreBuilding)
        {
            if (!unitCanvers.activeSelf)
            {
                unitCanvers.SetActive(true);
                hpBar.enabled = true;
            }
        }

        if (hp <= 0f)
            return;

        float reducedDamage = Mathf.Max(damage - towerData.Defense, 5);

        hp -= reducedDamage;
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
        if (hp == towerData.MaxHp)
        {
            return;
        }
        else if (hp + heal > towerData.MaxHp)
        {
            hp = towerData.MaxHp;
            if(!isRepair)
                unitCanvers.SetActive(false);
        }
        else        
            hp += heal;

        hpBar.fillAmount = hp / towerData.MaxHp;
    }

    public void RepairSet(bool repair)
    {
        hp = towerData.MaxHp;
        isRepair = repair;
        //repairBar.enabled = repair;
    }

    protected void RepairFunc(bool isBuilding)
    {
        repairGauge += 10.0f * Time.deltaTime;

        if (isBuilding)
        {
            repairBar.fillAmount = repairGauge / towerData.MaxBuildingGauge;
            if (repairGauge >= towerData.MaxBuildingGauge)
            {
                isPreBuilding = false;
                repairGauge = 0.0f;
                repairBar.enabled = false;
                if(hp < towerData.MaxHp)
                {
                    unitCanvers.SetActive(true);
                    hpBar.enabled = true;
                }
                else
                {
                    unitCanvers.SetActive(false);
                    //isRepair = true;
                }

                EnableColliders();
            }
        }
        else
        {
            repairBar.fillAmount = repairGauge / towerData.MaxRepairGauge;
            if (repairGauge >= towerData.MaxRepairGauge)
            {
                RepairEnd();
            }
        }
    }

    void RepairEnd()
    {
        hpBar.enabled = true;

        if (hp < towerData.MaxHp)
        {
            unitCanvers.SetActive(true);
            hpBar.enabled = true;
        }
        else
        {
            hp = towerData.MaxHp;
            unitCanvers.SetActive(false);
        }

        hpBar.fillAmount = hp / towerData.MaxHp;

        repairBar.enabled = false;
        repairGauge = 0.0f;

        isRuin = false;
        isPreBuilding = false;
        //isRepair = false;

        EnableColliders();

        animator.SetBool("isDie", false);
    }

    public void SetBuild()
    {
        //isRepair = true;
        unitCanvers.SetActive(true);
        hpBar.enabled = false;
        repairBar.enabled = true;
        repairGauge = 0;
        repairBar.fillAmount = repairGauge / towerData.MaxRepairGauge;
        isSetBuildingOk = true;
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
