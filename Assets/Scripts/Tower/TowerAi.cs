using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
public class TowerAi : Structure
{
    public enum TowerState
    {
        Waiting,
        Attack,
        AttackDelay
    }

    [SerializeField]
    protected TowerData towerData;
    protected TowerData TowerData { set { towerData = value; } }

    // 유닛 상태 관련
    [HideInInspector]
    public TowerState towerState = TowerState.Waiting;

    [SerializeField]
    protected Animator animator;

    //public CapsuleCollider2D capsule2D = null;

    protected float searchTimer = 0f;
    protected float searchInterval = 1f; // 딜레이 간격 설정

    public GameObject RuinExplo;

    private void Awake()
    {
        buildName = structureData.FactoryName; 
        col = GetComponent<CapsuleCollider2D>();
        hp = structureData.MaxHp[level];
        hpBar.fillAmount = hp / structureData.MaxHp[level];
        repairBar.fillAmount = 0;
    }

    protected virtual void Update()
    {
        if (isPreBuilding && isSetBuildingOk && !isRuin)
        {
            RepairFunc(true);
        }
    }

    protected override void RepairEnd()
    {
        base.RepairEnd();
        animator.SetBool("isDie", false);
    }
}
