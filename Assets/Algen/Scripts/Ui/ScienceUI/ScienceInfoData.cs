using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
public class ScienceInfoData
{
    public List<string> items;
    public List<int> amounts;
    public int coreLv;

    public ScienceInfoData() { }

    public ScienceInfoData(List<string> _items, List<int> _amounts, int _coreLv)
    {
        items = _items;
        amounts = _amounts;
        coreLv = _coreLv;
    }

    public int GetItemCount()
    {
        return items.Count;
    }
}
