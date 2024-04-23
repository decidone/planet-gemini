using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InventorySaveData
{
    public Dictionary<int, int> totalItemIndexes = new Dictionary<int, int>();
    public Dictionary<int, int> itemIndexes = new Dictionary<int, int>();
    public Dictionary<int, int> amounts = new Dictionary<int, int>();
}
