using UnityEngine;

[CreateAssetMenu(fileName = "Monster Data", menuName = "Scriptable Object/Monster Data", order = int.MaxValue)]

public class MonsterData : ScriptableObject
{
    [SerializeField]
    private string monsterName;//이름
    public string MonsterName { get { return monsterName; } }
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
    private float moveSpeed;//이동속도
    public float MoveSpeed { get { return moveSpeed; } }
    [SerializeField]
    private float patrolRad;//패트롤 범위
    public float PatrolRad { get { return patrolRad; } }
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
    private int attackNum;//공격 모션
    public int AttackNum { get { return attackNum; } }
}
