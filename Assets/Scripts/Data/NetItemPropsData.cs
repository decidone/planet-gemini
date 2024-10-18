using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]

public class NetItemPropsData
{
    // 아이템 인덱스, 개수, 위치
    public int itemIndex;
    public int amount;
    public SerializedVector3 pos = new SerializedVector3();
}
