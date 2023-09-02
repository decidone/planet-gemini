using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScienceInfoData
{
    public List<string> items;
    public List<int> amounts;
    public int coreLv;
    public string info;

    public ScienceInfoData() { }

    public ScienceInfoData(List<string> _items, List<int> _amounts, int _coreLv, string _info)
    {
        items = _items;
        amounts = _amounts;
        coreLv = _coreLv;
        info = _info;
    }
}
