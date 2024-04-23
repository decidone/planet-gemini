using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    // 플레이어
    public List<PlayerSaveData> playerDataList = new List<PlayerSaveData>();

    // 행성 인벤토리
    public InventorySaveData HostMapInvenData = new InventorySaveData();
    public InventorySaveData ClientMapInvenData = new InventorySaveData();
}
