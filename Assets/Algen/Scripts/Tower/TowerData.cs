using UnityEngine;

[CreateAssetMenu(fileName = "Tower Data", menuName = "Scriptable Object/Tower Data", order = int.MaxValue)]
public class TowerData : ScriptableObject
{
    [SerializeField]
    private string towerName;//�̸�
    public string TowerName { get { return towerName; } }
    [SerializeField]
    private int maxHp;//ü��
    public int MaxHp { get { return maxHp; } }
    [SerializeField]
    private int damage;//������
    public int Damage { get { return damage; } }
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