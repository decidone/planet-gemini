using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
public class ScienceInfoData
{
    public List<string> items;
    public List<int> amounts;
    public int coreLv;
    public float time;
    public string info;
    public bool basicScience;
    public ScienceInfoData() { }

    public ScienceInfoData(List<string> _items, List<int> _amounts, int _coreLv, float _time, string _info, bool _basicScience)
    {
        items = _items;
        amounts = _amounts;
        coreLv = _coreLv;
        time = _time;
        info = _info;
        basicScience = _basicScience;
    }
}
