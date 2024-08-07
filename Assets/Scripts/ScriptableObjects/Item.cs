using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    new public string name = "New Item";
    public Sprite icon = null;
    public int tier = -1;
}
