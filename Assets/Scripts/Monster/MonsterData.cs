using UnityEngine;

[CreateAssetMenu(fileName = "Monster Data", menuName = "Scriptable Object/Monster Data", order = int.MaxValue)]

public class MonsterData : ScriptableObject
{
    [SerializeField]
    private string monsterName;//�̸�
    public string MonsterName { get { return monsterName; } }
    [SerializeField]
    private int maxHp;//ü��
    public int MaxHp { get { return maxHp; } }
    [SerializeField]
    private float defense;//ü��
    public float Defense { get { return defense; } }
    [SerializeField]
    private int damage;//������
    public int Damage { get { return damage; } }
    [SerializeField]
    private float moveSpeed;//�̵��ӵ�
    public float MoveSpeed { get { return moveSpeed; } }
    [SerializeField]
    private float patrolRad;//��Ʈ�� ����
    public float PatrolRad { get { return patrolRad; } }
    [SerializeField]
    private float attackDist;//���� ����
    public float AttackDist { get { return attackDist; } }
    [SerializeField]
    private float attDelayTime;//���� ������
    public float AttDelayTime { get { return attDelayTime; } }
    [SerializeField]
    private float colliderRadius;//Ÿ�� Ž�� ����
    public float ColliderRadius { get { return colliderRadius; } }

    [SerializeField]
    private int attackNum;//���� ���
    public int AttackNum { get { return attackNum; } }
}
