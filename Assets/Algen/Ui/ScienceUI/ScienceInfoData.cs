using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScienceInfoData
{
    //public string name;
    //public List<string> items;
    //public List<int> amounts;
    //public int level;
    //public int coreLv;
    //public ScienceInfoData() { }

    //public ScienceInfoData(string _name, List<string> _items, List<int> _amounts, int _level, int _coreLv)
    //{
    //    name = _name;
    //    items = _items;
    //    amounts = _amounts;
    //    level = _level;
    //    coreLv = _coreLv;
    //}

    //public int GetItemCount()
    //{
    //    return items.Count;
    //}

    public string name;
    public List<string> items;
    public List<int> amounts;
    public int coreLv;
    public ScienceInfoData() { }

    public ScienceInfoData(string _name, List<string> _items, List<int> _amounts, int _coreLv)
    {
        name = _name;
        items = _items;
        amounts = _amounts;
        coreLv = _coreLv;
    }

    public int GetItemCount()
    {
        return items.Count;
    }
}
