using UnityEngine;

[CreateAssetMenu(fileName = "UnitCommonData Data", menuName = "Scriptable Object/UnitCommon Data", order = int.MaxValue)]
public class UnitCommonData : ScriptableObject
{
    [SerializeField]
    private string unitName;//�̸�
    public string UnitName { get { return unitName; } }
    [SerializeField]
    private int maxHp;//ü��
    public int MaxHp { get { return maxHp; } }

    [SerializeField]
    private int damage;//������
    public int Damage { get { return damage; } }
    [SerializeField]
    private float moveSpeed;//�̵��ӵ�
    public float MoveSpeed { get { return moveSpeed; } }
    [SerializeField]
    private float attackDist;//���� ����
    public float AttackDist { get { return attackDist; } }
    [SerializeField]
    private float attDelayTime;//���� ������
    public float AttDelayTime { get { return attDelayTime; } }
    [SerializeField]
    private float colliderRadius;//Ÿ�� Ž�� ����
    public float ColliderRadius { get { return colliderRadius; } }
    
    //���� ����
    [SerializeField]
    private float patrolRad;//��Ʈ�� ����
    public float PatrolRad { get { return patrolRad; } }
    [SerializeField]
    private int attackNum;//���� ���
    public int AttackNum { get { return attackNum; } }    
    [SerializeField]
    private float defense;
    public float Defense { get { return defense; } }
}
