using UnityEngine;

[CreateAssetMenu(fileName = "Unit Data", menuName = "Scriptable Object/Unit Data", order = int.MaxValue)]
public class UnitData : ScriptableObject
{
    [SerializeField]
    private string unitName;//�̸�
    public string UnitName { get { return unitName; } }
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
    private float attackDist;//���� ����
    public float AttackDist { get { return attackDist; } }
    [SerializeField]
    private float attDelayTime;//���� ������
    public float AttDelayTime { get { return attDelayTime; } }
    [SerializeField]
    private float colliderRadius;//Ÿ�� Ž�� ����
    public float ColliderRadius { get { return colliderRadius; } }
}
