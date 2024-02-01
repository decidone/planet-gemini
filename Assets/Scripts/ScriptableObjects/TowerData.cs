using UnityEngine;

// UTF-8 설정
[CreateAssetMenu(fileName = "Tower Data", menuName = "Data/Tower Data", order = int.MaxValue)]
public class TowerData : ScriptableObject
{
    [SerializeField]
    private int damage;//데미지
    public int Damage { get { return damage; } }
    [SerializeField]
    private float attackDist;//공격 범위
    public float AttackDist { get { return attackDist; } }
    [SerializeField]
    private float attDelayTime;//공격 딜레이
    public float AttDelayTime { get { return attDelayTime; } }
}
