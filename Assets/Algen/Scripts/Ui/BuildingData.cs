using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingData
{
    public string name;
    public List<string> items;
    public List<int> amounts;
    public float cooldown;

    public BuildingData() { }

    public BuildingData(string _name, List<string> _items, List<int> _amounts, float _cooldown)
    {
        name = _name;
        items = _items;
        amounts = _amounts;
        cooldown = _cooldown;
    }

    public int GetItemCount()
    {
        return items.Count;
    }
}
