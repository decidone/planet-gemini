using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
public class BuildingData
{
    public List<string> items;
    public List<int> amounts;
    public float cooldown;

    public BuildingData() { }

    public BuildingData(List<string> _items, List<int> _amounts, float _cooldown)
    {
        items = _items;
        amounts = _amounts;
        cooldown = _cooldown;
    }

    public int GetItemCount()
    {
        return items.Count;
    }
}
