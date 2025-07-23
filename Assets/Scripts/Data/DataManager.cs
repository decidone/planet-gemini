using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Unity.Netcode;
using System.Text;

public class DataManager : MonoBehaviour
{
    public SaveData saveData;
    public string path;
    public int selectedSlot;    // 저장 슬롯. 나중에 ui 넣을 때 지정
    NetworkObjManager netObjMgr;
    [SerializeField]
    GameObject beltGroup;
    [SerializeField]
    GameObject beltMgr;
    Dictionary<Transporter, StructureSaveData> transporters = new Dictionary<Transporter, StructureSaveData>();
    //List<Transporter> transporters = new List<Transporter>();
    List<LDConnector> lDConnectors = new List<LDConnector>();
    [SerializeField]
    GameObject spawner;
    AreaLevelData[] levelData;

    #region Singleton
    public static DataManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        path = Application.persistentDataPath + "/save";
    }
    #endregion

    private void Start()
    {
        saveData = new SaveData();
        selectedSlot = 0;
        netObjMgr = NetworkObjManager.instance;
    }

    public (string, byte[]) Save(int saveSlotNum)
    {
        return Save(saveSlotNum, null);
    }

    public (string, byte[]) Save(int saveSlotNum, string fileName)
    {
        PlayerSaveData lastClientSaveData = saveData.clientPlayerData;
        saveData = new SaveData();

        InGameData inGameData = GameManager.instance.SaveData();
        saveData.InGameData = inGameData;
        saveData.InGameData.fileName = fileName;
        saveData.InGameData.hostPortalName = GameManager.instance.portal[0].portalName;
        saveData.InGameData.clientPortalName = GameManager.instance.portal[1].portalName;

        // 플레이어
        saveData.hostPlayerData = GameManager.instance.PlayerSaveData(true);
        PlayerSaveData tempData = GameManager.instance.PlayerSaveData(false);

        if (lastClientSaveData.clientFirstConnection && tempData.hp == -1)
        {
            saveData.clientPlayerData = lastClientSaveData;
        }
        else
        {
            saveData.clientPlayerData = tempData;
        }

        // 행성 인벤토리
        InventorySaveData hostMapInventoryData = GameManager.instance.hostMapInven.SaveData();
        saveData.hostMapInvenData = hostMapInventoryData;
        InventorySaveData clientMapInventoryData = GameManager.instance.clientMapInven.SaveData();
        saveData.clientMapInvenData = clientMapInventoryData;

        foreach (ScienceBtn scienceBtn in ScienceDb.instance.scienceBtns)
        {
            ScienceData scienceData = scienceBtn.SaveData();
            saveData.scienceData.Add(scienceData);
        }

        foreach (Structure structure in netObjMgr.netStructures)
        {
            StructureSaveData structureSaveData = structure.SaveData();
            saveData.structureData.Add(structureSaveData);
        }

        foreach (BeltGroupMgr beltGroup in netObjMgr.netBeltGroupMgrs)
        {
            if (beltGroup.beltList.Count > 0)
            {
                BeltGroupSaveData beltGroupSaveData = beltGroup.SaveData();
                saveData.beltGroupData.Add(beltGroupSaveData);
            }
        }

        foreach (UnitCommonAi unitAi in netObjMgr.netUnitCommonAis)
        {
            //if (unitAi.GetComponent<TankCtrl>().playerOnTank)
            //    continue;
            UnitSaveData unitSaveData = unitAi.SaveData();
            saveData.unitData.Add(unitSaveData);
        }

        MonsterSpawnerManager monsterSpawner = MonsterSpawnerManager.instance;
        SpawnerManagerSaveData spawnerManagerSaveData = monsterSpawner.SaveData();
        saveData.spawnerManagerSaveData = spawnerManagerSaveData;

        OverallSaveData overallSaveData = Overall.instance.SaveData();
        saveData.overallData = overallSaveData;

        MapsSaveData mapsSaveData = MapGenerator.instance.SaveData();
        //saveData.mapData = mapsSaveData;
        //MapSaveData mapSaveData = GameManager.instance.SaveMapData();
        //saveData.mapData = mapSaveData;

        List<NetItemPropsData> netItemPropsDatas = NetworkItemPoolSync.instance.NetItemSaveData();
        saveData.netItemData = netItemPropsDatas;

        List<HomelessDroneSaveData> homelessDroneSaveData = HomelessDroneManager.instance.SaveDroneData();
        saveData.homelessDroneData = homelessDroneSaveData;

        // Json 저장
        Debug.Log("saved: " + path);
        string json = JsonConvert.SerializeObject(saveData);
        File.WriteAllText(path + saveSlotNum.ToString() + ".json", json);

        string mapJson = JsonConvert.SerializeObject(mapsSaveData);
        var compData = Compression.Compress(mapJson);
        File.WriteAllBytes(path + saveSlotNum.ToString() + ".maps", compData);

        selectedSlot = saveSlotNum;

        return (json, compData);
    }

    //public string GetJsonFromFile(int saveSlotNum)
    //{
    //    string json = File.ReadAllText(path + saveSlotNum.ToString() + ".json");
    //    return json;
    //}

    //public MapsSaveData GetMapDataFromFile(int saveSlotNum)
    //{
    //    byte[] json = File.ReadAllBytes(path + saveSlotNum.ToString() + ".maps");
    //    string decompData = Compression.Decompress(json);
    //    MapsSaveData mapData = new MapsSaveData();
    //    mapData = JsonConvert.DeserializeObject<MapsSaveData>(decompData);

    //    return mapData;
    //}

    public void Load()
    {
        // 호스트가 파일로부터 json을 불러와서 동기화
        saveData = LoadManager.instance.GetSaveData();

        //string json = GetJsonFromFile(saveSlotNum);
        //selectedSlot = saveSlotNum;
        //saveData = JsonConvert.DeserializeObject<SaveData>(json);
        LoadData(saveData);

        transporters.Clear();
        lDConnectors.Clear();

        //TempScienceDb.instance.LoadData(saveData.ScienceData);

        foreach (StructureSaveData structureSave in saveData.structureData)
        {
            SpawnStructure(structureSave);
        }

        foreach (BeltGroupSaveData beltGroupSave in saveData.beltGroupData)
        {
            SpawnBeltGroup(beltGroupSave);
        }

        foreach (UnitSaveData unitSave in saveData.unitData)
        {
            SpawnUnit(unitSave);
        }

        NetworkItemPoolSync.instance.NetItemLoadData(saveData.netItemData);

        HomelessDroneManager.instance.LoadDroneData(saveData.homelessDroneData);

        SetSpawnerManager(saveData.spawnerManagerSaveData);

        SetConnectedFunc();
    }

    public void LoadClient()
    {
        // 클라이언트가 접속 시 호스트로부터 json을 받아서 동기화
        // 네트워크 오브젝트라서 스폰을 시킬 필요가 없는 경우 등등 호스트가 파일을 불러와서 동기화 하는 과정과는 좀 달라질 예정
        SaveData saveData = LoadManager.instance.GetSaveData();
        LoadData(saveData);
    }

    public void LoadData(SaveData saveData)
    {
        GameManager.instance.LoadData(saveData.InGameData);
        GameManager.instance.LoadPlayerData(saveData.hostPlayerData, saveData.clientPlayerData);

        // 행성 인벤토리
        GameManager.instance.hostMapInven.LoadData(saveData.hostMapInvenData);
        GameManager.instance.clientMapInven.LoadData(saveData.clientMapInvenData);

        ScienceDb.instance.LoadSet(saveData.scienceData);
        Overall.instance.LoadData(saveData.overallData);
        //MapGenerator.instance.LoadData(saveData.mapData);
        //GameManager.instance.LoadMapData(saveData.mapData);
    }

    void SpawnStructure(StructureSaveData saveData)
    {
        Building building = GeminiNetworkManager.instance.GetBuildingSOFromIndex(saveData.index);
        Vector3 spawnPos = Vector3Extensions.ToVector3(saveData.pos);
        //Vector3 spawnPos = new Vector3(saveData.pos[0], saveData.pos[1], saveData.pos[2]);
        GameObject spawnobj;

        if (!saveData.sideObj)
        {
            spawnobj = Instantiate(building.gameObj, spawnPos, Quaternion.identity);
        }
        else
        {
            spawnobj = Instantiate(building.sideObj, spawnPos, Quaternion.identity);
        }

        spawnobj.TryGetComponent(out NetworkObject netObj);
        if (!netObj.IsSpawned) spawnobj.GetComponent<NetworkObject>().Spawn(true);

        if (netObj.TryGetComponent(out Structure structure))
        {
            structure.GameStartSpawnSet(saveData.level, saveData.direction, building.height, building.width, saveData.planet, saveData.index);
            structure.StructureStateSet(saveData.isPreBuilding, saveData.destroyStart, saveData.hp, saveData.repairGauge, saveData.destroyTimer);
            structure.GameStartRecipeSet(saveData.recipeId);
            structure.MapDataSaveClientRpc(Vector3Extensions.ToVector3(saveData.pos));

            if (saveData.portalName != "")
                structure.portalName = saveData.portalName;

            //if (saveData.connectedStrPos.Count > 0) //아래로 뺐음
            //{
            //    if (structure.TryGetComponent(out Transporter transporter))
            //    {
            //        transporters.Add(transporter, saveData);
            //        structure.ConnectedPosListPosSet(Vector3Extensions.ToVector3(saveData.connectedStrPos[0]));
            //    }
            //    else if (structure.TryGetComponent(out UnitFactory unitFactory))
            //    {
            //        unitFactory.UnitSpawnPosSetServerRpc(Vector3Extensions.ToVector3(saveData.connectedStrPos[0]));
            //    }
            //    else if (structure.TryGetComponent(out LDConnector lDConnector))
            //    {
            //        lDConnectors.Add(lDConnector);
            //        for (int i = 0; i < saveData.connectedStrPos.Count; i++)
            //        {
            //            structure.ConnectedPosListPosSet(Vector3Extensions.ToVector3(saveData.connectedStrPos[i]));
            //        }
            //    }
            //}

            if (structure.TryGetComponent(out Production prod))
            {
                prod.GameStartItemSet(saveData.inven);
                if (prod.GetComponent<PortalObj>())
                {
                    if (saveData.planet)
                    {
                        Portal portal = GameManager.instance.portal[0];
                        spawnobj.transform.parent = portal.transform;
                        portal.SetPortalObjEnd(structure.structureData.FactoryName, spawnobj);
                    }
                    else
                    {
                        Portal portal = GameManager.instance.portal[1];
                        spawnobj.transform.parent = portal.transform;
                        portal.SetPortalObjEnd(structure.structureData.FactoryName, spawnobj);
                    }
                }

                if (structure.TryGetComponent(out AttackTower tower))
                {
                    tower.energyBulletAmount = saveData.energyBulletAmount;
                }

                if (structure.TryGetComponent(out Disintegrator disintegrator))
                {
                    disintegrator.SetAuto(saveData.isAuto);
                }

                if (structure.TryGetComponent(out AutoSeller autoSeller))
                {
                    if (saveData.trUnitPosData.Count > 0)
                    {
                        for (int i = 0; i < saveData.trUnitPosData.Count; i++)
                        {
                            Vector3 unitSpawnPos = Vector3Extensions.ToVector3(saveData.trUnitPosData[i]);

                            Dictionary<int, int> itemDic = new Dictionary<int, int>();

                            if (saveData.trUnitItemData.ContainsKey(i))
                            {
                                itemDic = saveData.trUnitItemData[i];
                            }

                            autoSeller.UnitLoad(unitSpawnPos, itemDic);
                        }
                    }
                }

                if (structure.TryGetComponent(out AutoBuyer autoBuyer))
                {
                    autoBuyer.maxBuyAmount = saveData.maxBuyAmount;
                    autoBuyer.minBuyAmount = saveData.minBuyAmount;

                    if (saveData.trUnitPosData.Count > 0)
                    {
                        for (int i = 0; i < saveData.trUnitPosData.Count; i++)
                        {
                            Vector3 unitSpawnPos = Vector3Extensions.ToVector3(saveData.trUnitPosData[i]);

                            Dictionary<int, int> itemDic = new Dictionary<int, int>();

                            if (saveData.trUnitItemData.ContainsKey(i))
                            {
                                itemDic = saveData.trUnitItemData[i];
                            }

                            autoBuyer.UnitLoad(unitSpawnPos, itemDic);
                        }
                    }
                }

                if (structure.TryGetComponent(out Transporter transporter))
                {
                    transporters.Add(transporter, saveData);
                    if (saveData.connectedStrPos.Count > 0)
                        structure.ConnectedPosListPosSet(Vector3Extensions.ToVector3(saveData.connectedStrPos[0]));
                }
                else if (structure.TryGetComponent(out UnitFactory unitFactory))
                {
                    if (saveData.connectedStrPos.Count > 0)
                        unitFactory.UnitSpawnPosSetServerRpc(Vector3Extensions.ToVector3(saveData.connectedStrPos[0]));
                }
            }
            else
            {
                if (structure.TryGetComponent(out SplitterCtrl splitterCtrl))
                {
                    for (int a = 0; a < saveData.filters.Count; a++)
                    {
                        FilterSaveData filterSaveData = saveData.filters[a];
                        splitterCtrl.GameStartFillterSet(a, filterSaveData.filterOn, filterSaveData.filterInvert, filterSaveData.filterItemIndex);
                    }
                }
                else if (structure.TryGetComponent(out Unloader unloader))
                {
                    FilterSaveData filterSaveData = saveData.filters[0];
                    if (filterSaveData.filterItemIndex != -1)
                        unloader.GameStartFillterSet(filterSaveData.filterItemIndex);
                }
                else if (structure.TryGetComponent(out LDConnector lDConnector))
                {
                    lDConnectors.Add(lDConnector);
                    if (saveData.connectedStrPos.Count > 0)
                    {
                        for (int i = 0; i < saveData.connectedStrPos.Count; i++)
                        {
                            structure.ConnectedPosListPosSet(Vector3Extensions.ToVector3(saveData.connectedStrPos[i]));
                        }
                    }
                }

                foreach (int itemIndex in saveData.itemIndex)
                {
                    structure.GameStartItemSet(itemIndex);
                }
            }
        }
    }

    void SpawnBeltGroup(BeltGroupSaveData saveData)
    {
        GameObject beltGroupObj = Instantiate(beltGroup);
        beltGroupObj.TryGetComponent(out NetworkObject netObj);
        if (!netObj.IsSpawned) beltGroupObj.GetComponent<NetworkObject>().Spawn(true);
        beltGroupObj.transform.parent = beltMgr.transform;
        beltGroupObj.TryGetComponent(out BeltGroupMgr beltGroupMgr);

        foreach (var beltData in saveData.beltList)
        {
            Building building = GeminiNetworkManager.instance.GetBuildingSOFromIndex(beltData.Item2.index);
            Vector3 spawnPos = Vector3Extensions.ToVector3(beltData.Item2.pos);
            GameObject beltObj = Instantiate(building.gameObj, spawnPos, Quaternion.identity);
            beltObj.TryGetComponent(out NetworkObject netBeltObj);
            if (!netBeltObj.IsSpawned) beltObj.GetComponent<NetworkObject>().Spawn(true);

            if (netBeltObj.TryGetComponent(out Structure structure))
            {
                structure.GameStartSpawnSet(beltData.Item2.level, beltData.Item2.direction, building.height, building.width, beltData.Item2.planet, beltData.Item2.index);
                structure.StructureStateSet(beltData.Item2.isPreBuilding, beltData.Item2.destroyStart, beltData.Item2.hp, beltData.Item2.repairGauge, beltData.Item2.destroyTimer);
                structure.MapDataSaveClientRpc(Vector3Extensions.ToVector3(beltData.Item2.pos));
            }

            if (netBeltObj.TryGetComponent(out BeltCtrl belt))
            {
                belt.GameStartBeltSet(beltData.Item1.modelMotion, beltData.Item1.isTrun, beltData.Item1.isRightTurn, beltData.Item1.beltState);

                for (int i = 0; i < beltData.Item1.itemIndex.Count; i++)
                {
                    Vector3 itemPos = Vector3Extensions.ToVector3(beltData.Item1.itemPos[i]);
                    belt.GameStartItemSet(itemPos, beltData.Item1.itemIndex[i]);
                }
            }

            beltObj.transform.parent = beltGroupMgr.transform;
            beltGroupMgr.beltList.Add(belt);
        }

        beltGroupMgr.SetBeltData();
        beltGroupMgr.ItemIndexSet();
    }

    void SetConnectedFunc()
    {
        foreach (var transporterData in transporters)
        {
            Transporter transporter = transporterData.Key;
            StructureSaveData strData = transporterData.Value;
            Transporter takeTransporter = null;
            GameObject findObj = null;
            if (transporter.connectedPosList.Count > 0)
                findObj = CellObjFind(transporter.connectedPosList[0], transporter.isInHostMap);

            if (findObj != null && findObj.TryGetComponent(out takeTransporter))
                transporter.TakeBuildSet(takeTransporter);

            if (strData.trUnitPosData.Count > 0)
            {
                for (int i = 0; i < strData.trUnitPosData.Count; i++)
                {
                    Vector3 spawnPos = Vector3Extensions.ToVector3(strData.trUnitPosData[i]);

                    Dictionary<int, int> itemDic = new Dictionary<int, int>();

                    if (strData.trUnitItemData.ContainsKey(i))
                    {
                        itemDic = strData.trUnitItemData[i]; 
                    }

                    if (takeTransporter != null)
                    {
                        transporter.UnitLoad(spawnPos, takeTransporter, itemDic);
                    }
                    else
                    {
                        transporter.UnitLoad(spawnPos, itemDic);
                    }
                }
            }
        }

        foreach (LDConnector lDConnector in lDConnectors)
        {
            lDConnector.connector.Init();
            lDConnector.isBuildDone = true;

            for (int i = 0; i < lDConnector.connectedPosList.Count; i++)
            {
                GameObject findObj = CellObjFind(lDConnector.connectedPosList[i], lDConnector.isInHostMap);
                if (findObj != null && findObj.TryGetComponent(out LDConnector othLDConnector))
                {
                    if (lDConnector.TryGetComponent(out MapClickEvent mapClick) && othLDConnector.TryGetComponent(out MapClickEvent othMapClick))                    
                    {
                        mapClick.GameStartSetRenderer(othMapClick);
                    }
                }
            }
        }
    }

    void SpawnUnit(UnitSaveData unitSaveData)
    {
        GameObject spawnobj = Instantiate(GeminiNetworkManager.instance.unitListSO.userUnitList[unitSaveData.unitIndex]);
        spawnobj.TryGetComponent(out NetworkObject netObj);
        if (!netObj.IsSpawned) spawnobj.GetComponent<NetworkObject>().Spawn(true);

        spawnobj.transform.position = Vector3Extensions.ToVector3(unitSaveData.pos);
        spawnobj.GetComponent<UnitAi>().GameStartSet(unitSaveData);

        if (unitSaveData.portalUnitIn)
        {
            if (unitSaveData.hostClientUnitIn)
            {
                GameManager.instance.portal[0].GetComponentInChildren<PortalUnitIn>().LoadUnitData(spawnobj);
            }
            else
            {
                GameManager.instance.portal[1].GetComponentInChildren<PortalUnitIn>().LoadUnitData(spawnobj);
            }
        }
    }

    void SetSpawnerManager(SpawnerManagerSaveData spawnerManagerSaveData)
    {
        levelData = SpawnerSetManager.instance.arealevelData;
        if(spawnerManagerSaveData.splitCount != 0)
            MonsterSpawnerManager.instance.SplitCountSet(spawnerManagerSaveData.splitCount);

        MonsterSpawnerManager.instance.WaveStateLoad(spawnerManagerSaveData);

        for (int i = 0; i < spawnerManagerSaveData.splitCount; i++)
        {
            for (int j = 0; j < spawnerManagerSaveData.splitCount; j++)
            {
                SpawnerGroupData spawner1GroupData = spawnerManagerSaveData.spawnerMap1Matrix[i, j];
                if(spawner1GroupData != null)
                {
                    GameObject group = SetSpawnerGroupMgr(spawner1GroupData, true);
                    MonsterSpawnerManager.instance.MatrixSet(group, (i, j), true);
                }

                SpawnerGroupData spawner2GroupData = spawnerManagerSaveData.spawnerMap2Matrix[i, j];
                if (spawner2GroupData != null)
                {
                    GameObject group = SetSpawnerGroupMgr(spawner2GroupData, false);
                    MonsterSpawnerManager.instance.MatrixSet(group, (i, j), false);
                }
            }
        }
    }

    GameObject SetSpawnerGroupMgr(SpawnerGroupData spawnerGroupData, bool planet)
    {
        SpawnerSetManager spawnerSetManager = SpawnerSetManager.instance;
        GameObject spawnerGroupObj = spawnerSetManager.SpawnerGroupSet(Vector3Extensions.ToVector3(spawnerGroupData.pos));
        spawnerGroupObj.TryGetComponent(out SpawnerGroupManager spawnerGroup);
        spawnerGroup.SpawnerGroupStatsSet(spawnerGroupData.spawnerMatrixIndex);
        foreach (SpawnerSaveData spawnerSaveData in spawnerGroupData.spawnerSaveDataList)
        {
            if (spawnerSaveData.dieCheck && spawnerSaveData.monsterList.Count == 0)
                continue;

            GameObject spawner = SpawnSpawner(spawnerSaveData);
            spawnerGroup.SpawnerSet(spawner);
            spawner.TryGetComponent(out MonsterSpawner monsterSpawner);
            monsterSpawner.dieCheck = spawnerSaveData.dieCheck;
            if (monsterSpawner.dieCheck)
            {
                monsterSpawner.DieFuncLoad();
            }
            MonsterSpawnerManager.instance.AreaGroupSet(monsterSpawner, spawnerSaveData.spawnerGroupIndex, planet);
            monsterSpawner.groupManager = spawnerGroup;
            monsterSpawner.GameStartSet(spawnerSaveData, levelData[spawnerSaveData.level - 1], Vector3Extensions.ToVector3(spawnerSaveData.wavePos), planet, spawnerSaveData.spawnerGroupIndex);
            SetSpawner(monsterSpawner, spawnerSaveData, planet);
            monsterSpawner.violentCollSize = spawnerSaveData.violentCollSize;
        }

        return spawnerGroupObj;
    }

    void SetSpawner(MonsterSpawner monsterSpawner, SpawnerSaveData spawnerSaveData, bool planet)
    {
        foreach (UnitSaveData unitSaveData in spawnerSaveData.monsterList)
        {
            GameObject monster = null;
            if (!unitSaveData.waveState)
            {
                monster = monsterSpawner.SpawnMonster(unitSaveData.monsterType, unitSaveData.unitIndex, planet);
                monster.transform.position = Vector3Extensions.ToVector3(unitSaveData.pos);
            }
            else
            {
                monster = monsterSpawner.WaveMonsterSpawn(unitSaveData.monsterType, unitSaveData.unitIndex, planet, unitSaveData.isWaveColonyCallCheck);
                monster.transform.position = Vector3Extensions.ToVector3(unitSaveData.pos);
                monster.GetComponent<MonsterAi>().WaveStart(Vector3Extensions.ToVector3(unitSaveData.wavePos));
            }
            monster.GetComponent<MonsterAi>().GameStartSet(unitSaveData);
        }
    }

    GameObject SpawnSpawner(SpawnerSaveData spawnerSaveData)
    {
        GameObject spawnerObj = Instantiate(spawner);
        NetworkObject networkObject = spawnerObj.GetComponent<NetworkObject>();
        if (!networkObject.IsSpawned) networkObject.Spawn(true);
        spawnerObj.transform.position = Vector3Extensions.ToVector3(spawnerSaveData.spawnerPos);
        MapGenerator.instance.SetCorruption(spawnerObj.transform.position, spawnerSaveData.level);

        return spawnerObj;
    }

    public GameObject CellObjFind(Vector3 findPos, bool isInHostMap)
    {
        int x = Mathf.FloorToInt(findPos.x);
        int y = Mathf.FloorToInt(findPos.y);
        Map map;
        if (isInHostMap)
            map = GameManager.instance.hostMap;
        else
            map = GameManager.instance.clientMap;

        Cell cell = map.GetCellDataFromPos(x, y);

        return cell.structure;
    }
}
