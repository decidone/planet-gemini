using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
public class BuildingData
{
    public List<string> items;
    public List<int> amounts;

    public BuildingData() { }

    public BuildingData(List<string> _items, List<int> _amounts)
    {
        items = _items;
        amounts = _amounts;
    }

    public int GetItemCount()
    {
        return items.Count;
    }
}
