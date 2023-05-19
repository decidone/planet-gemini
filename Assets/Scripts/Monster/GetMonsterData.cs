using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetMonsterData : MonoBehaviour
{
    [SerializeField]
    public MonsterData monsteData;
    public MonsterData MonsterData { set { monsteData = value; } }
}
