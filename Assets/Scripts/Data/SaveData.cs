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

    public List<ScienceData> ScienceData = new List<ScienceData>();

    public List<StructureSaveData> structureData = new List<StructureSaveData>();
    public List<BeltGroupSaveData> beltGroupData = new List<BeltGroupSaveData>();
    public List<UnitSaveData> unitData = new List<UnitSaveData>();
    public SpawnerManagerSaveData spawnerManagerSaveData = new SpawnerManagerSaveData();
}
