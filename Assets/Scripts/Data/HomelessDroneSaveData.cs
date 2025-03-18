using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class HomelessDroneSaveData
{
    public string state;    // idle, move, return, homeless
    public SerializedVector3 startPos;
    public SerializedVector3 endPos;
    public SerializedVector3 pos;
    public Dictionary<int, int> invenItemData = new Dictionary<int, int>();
}
