using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New MerchandiseListSO", menuName = "SOList/MerchandiseListSO")]
public class MerchandiseListSO : ScriptableObject
{
    public List<Merchandise> MerchandiseSOList;
}
