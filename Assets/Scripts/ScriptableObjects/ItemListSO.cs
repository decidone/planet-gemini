using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemListSO", menuName = "SOList/ItemListSO")]
public class ItemListSO : ScriptableObject
{
    public List<Item> itemSOList;
}
