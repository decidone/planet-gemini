using UnityEngine;

[CreateAssetMenu(fileName = "Tower Data", menuName = "Scriptable Object/Tower Data", order = int.MaxValue)]
public class TowerData : ScriptableObject
{
    [SerializeField]
    private string towerName;//이름
    public string TowerName { get { return towerName; } }
    [SerializeField]
    private int maxHp;//체력
    public int MaxHp { get { return maxHp; } }
    [SerializeField]
    private float defense;//체력
    public float Defense { get { return defense; } }
    [SerializeField]
    private int damage;//데미지
    public int Damage { get { return damage; } }
    [SerializeField]
    private float attackDist;//공격 범위
    public float AttackDist { get { return attackDist; } }
    [SerializeField]
    private float attDelayTime;//공격 딜레이
    public float AttDelayTime { get { return attDelayTime; } }
    [SerializeField]
    private float colliderRadius;//타겟 탐색 범위
    public float ColliderRadius { get { return colliderRadius; } }

    [SerializeField]
    private float maxBuildingGauge;
    public float MaxBuildingGauge { get { return maxBuildingGauge; } }

    [SerializeField]
    private float maxRepairGauge;
    public float MaxRepairGauge { get { return maxRepairGauge; } }
}
