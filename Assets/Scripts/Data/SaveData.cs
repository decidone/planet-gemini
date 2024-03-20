using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    public List<PlayerData> playerDataList = new List<PlayerData>();

    public InventoryData HostMapInvenData = new InventoryData();
    public InventoryData ClientMapInvenData = new InventoryData();
}
