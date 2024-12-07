using UnityEngine;

// UTF-8 설정
[CreateAssetMenu(fileName = "UnitCommonData Data", menuName = "Data/UnitCommon Data", order = int.MaxValue)]
public class UnitCommonData : ScriptableObject
{
    [SerializeField]
    private string unitName;//이름
    public string UnitName { get { return unitName; } }

    [SerializeField]
    private int maxHp;//체력
    public int MaxHp { get { return maxHp; } }
    [SerializeField]
    private int upgradeMaxHp;//체력
    public int UpgradeMaxHp { get { return upgradeMaxHp; } }

    [SerializeField]
    private int damage;//데미지
    public int Damage { get { return damage; } }
    [SerializeField]
    private int upgradeDamage;//데미지
    public int UpgradeDamage { get { return upgradeDamage; } }
    [SerializeField]
    private float moveSpeed;//이동속도
    public float MoveSpeed { get { return moveSpeed; } }
    [SerializeField]
    private float attackDist;//공격 범위
    public float AttackDist { get { return attackDist; } }
    [SerializeField]
    private float attDelayTime;//공격 딜레이
    public float AttDelayTime { get { return attDelayTime; } }
    [SerializeField]
    private float upgradeAttDelayTime;//공격 딜레이
    public float UpgradeAttDelayTime { get { return upgradeAttDelayTime; } }
    [SerializeField]
    private float colliderRadius;//타겟 탐색 범위
    public float ColliderRadius { get { return colliderRadius; } }

    [SerializeField]
    private float minCollRad;//타겟 탐색 최소 범위
    public float MinCollRad { get { return minCollRad; } }

    [SerializeField]
    private float defense;
    public float Defense { get { return defense; } }

    [SerializeField]
    private float upgradeDefense;
    public float UpgradeDefense { get { return upgradeDefense; } }

    //몬스터 전용
    [SerializeField]
    private float patrolRad;//패트롤 범위
    public float PatrolRad { get { return patrolRad; } }

}
