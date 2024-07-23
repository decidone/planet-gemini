using System;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Unity.Netcode;

public class DataManager : MonoBehaviour
{
    InputManager inputManager;
    public SaveData saveData;
    public string path;
    public int selectedSlot;    // 저장 슬롯. 나중에 ui 넣을 때 지정
    NetworkObjManager netObjMgr;
    [SerializeField]
    GameObject beltGroup;
    [SerializeField]
    GameObject beltMgr;
    List<Transporter> transporters = new List<Transporter>();
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
            Debug.LogWarning("More than one instance of DataManager found!");
            return;
        }

        instance = this;
    }
    #endregion

    private void Start()
    {
        path = Application.persistentDataPath + "/save";
        saveData = new SaveData();
        selectedSlot = 0;

        inputManager = InputManager.instance;
        netObjMgr = NetworkObjManager.instance;
        inputManager.controls.HotKey.Save.performed += ctx => Save(selectedSlot);
        inputManager.controls.HotKey.Load.performed += ctx => Load(selectedSlot);
    }

    public string Save(int saveSlotNum)
    {
        return Save(saveSlotNum, null);
    }

    public string Save(int saveSlotNum, string fileName)
    {
        saveData = new SaveData();

        // 저장 시간
        DateTime currentDateTime = DateTime.Now;
        string formattedDateTime = currentDateTime.ToString("yyyy-MM-dd HH:mm:ss");
        saveData.saveDate = formattedDateTime;

        // 파일 이름
        saveData.fileName = fileName;

        // 플레이어
        PlayerSaveData playerData = new PlayerSaveData();
        playerData.isHostPlayer = true; // 임시
        saveData.playerDataList.Add(playerData);

        // 행성 인벤토리
        InventorySaveData hostMapInventoryData = GameManager.instance.hostMapInven.SaveData();
        saveData.HostMapInvenData = hostMapInventoryData;
        InventorySaveData clientMapInventoryData = GameManager.instance.clientMapInven.SaveData();
        saveData.ClientMapInvenData = clientMapInventoryData;

        foreach (ScienceBtn scienceBtn in TempScienceDb.instance.scienceBtns)
        {
            ScienceData scienceData = scienceBtn.SaveData();
            saveData.ScienceData.Add(scienceData);
        }

        foreach (Structure structure in netObjMgr.netStructures)
        {
            StructureSaveData structureSaveData = structure.SaveData();
            saveData.structureData.Add(structureSaveData);
        }

        foreach (BeltGroupMgr beltGroup in netObjMgr.netBeltGroupMgrs)
        {
            BeltGroupSaveData beltGroupSaveData = beltGroup.SaveData();
            saveData.beltGroupData.Add(beltGroupSaveData);
        }

        foreach (UnitCommonAi unitAi in netObjMgr.netUnitCommonAis)
        {
            UnitSaveData unitSaveData = unitAi.SaveData();
            saveData.unitData.Add(unitSaveData);
        }

        MonsterSpawnerManager monsterSpawner = MonsterSpawnerManager.instance;
        SpawnerManagerSaveData spawnerManagerSaveData = monsterSpawner.SaveData();
        saveData.spawnerManagerSaveData = spawnerManagerSaveData;

        // Json 저장
        Debug.Log("saved: " + path);
        string json = JsonConvert.SerializeObject(saveData);
        File.WriteAllText(path + saveSlotNum.ToString() + ".json", json);
        selectedSlot = saveSlotNum;
        return json;
    }

    public string GetJsonFromFile(int saveSlotNum)
    {
        string json = File.ReadAllText(path + saveSlotNum.ToString() + ".json");
        return json;
    }

    public void Load(int saveSlotNum)
    {
        // 호스트가 파일로부터 json을 불러와서 동기화
        string json = GetJsonFromFile(saveSlotNum);
        selectedSlot = saveSlotNum;
        saveData = JsonConvert.DeserializeObject<SaveData>(json);
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

        foreach(UnitSaveData unitSave in saveData.unitData)
        {
            SpawnUnit(unitSave);
        }

        SetSpawnerManager(saveData.spawnerManagerSaveData);

        SetConnectedFunc();
    }

    public void Load(string json)
    {
        // 클라이언트가 접속 시 호스트로부터 json을 받아서 동기화
        // 네트워크 오브젝트라서 스폰을 시킬 필요가 없는 경우 등등 호스트가 파일을 불러와서 동기화 하는 과정과는 좀 달라질 예정
        saveData = JsonConvert.DeserializeObject<SaveData>(json);
        LoadData(saveData);
    }

    public void LoadData(SaveData saveData)
    {
        // 행성 인벤토리
        GameManager.instance.hostMapInven.LoadData(saveData.HostMapInvenData);
        GameManager.instance.clientMapInven.LoadData(saveData.ClientMapInvenData);
        TempScienceDb.instance.LoadData(saveData.ScienceData);
    }

    public void Clear()
    {
        //selectedSlot = -1;
        saveData = new SaveData();
    }

    void SpawnStructure(StructureSaveData saveData)
    {
        Building building = GeminiNetworkManager.instance.GetBuildingSOFromIndex(saveData.index);
        Vector3 spawnPos = Vector3Extensions.ToVector3(saveData.pos);
        //Vector3 spawnPos = new Vector3(saveData.pos[0], saveData.pos[1], saveData.pos[2]);
        GameObject spawnobj = Instantiate(building.gameObj, spawnPos, Quaternion.identity);
        spawnobj.TryGetComponent(out NetworkObject netObj);
        if (!netObj.IsSpawned) spawnobj.GetComponent<NetworkObject>().Spawn();

        if (netObj.TryGetComponent(out Structure structure))
        {
            structure.GameStartSpawnSet(saveData.level, saveData.direction, building.height, building.width, saveData.planet, saveData.index);
            structure.StructureStateSet(saveData.isPreBuilding, saveData.isSetBuildingOk, saveData.destroyStart, saveData.hp, saveData.repairGauge, saveData.destroyTimer);
            structure.GameStartRecipeSet(saveData.recipeId);
            structure.MapDataSaveClientRpc(Vector3Extensions.ToVector3(saveData.tileSetPos));

            if (saveData.connectedStrPos.Count > 0)
            {
                if (structure.TryGetComponent(out Transporter transporter))
                {
                    transporters.Add(transporter);
                    structure.ConnectedPosListPosSet(Vector3Extensions.ToVector3(saveData.connectedStrPos[0]));                    
                }
                else if (structure.TryGetComponent(out UnitFactory unitFactory))
                {
                    unitFactory.UnitSpawnPosSetServerRpc(Vector3Extensions.ToVector3(saveData.connectedStrPos[0]));
                }
                else if (structure.TryGetComponent(out LDConnector lDConnector))
                {
                    lDConnectors.Add(lDConnector);
                    for (int i = 0; i < saveData.connectedStrPos.Count; i++)
                    {
                        structure.ConnectedPosListPosSet(Vector3Extensions.ToVector3(saveData.connectedStrPos[i]));
                    }                    
                }
            }            

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
            }
            else
            {
                if (structure.TryGetComponent(out SplitterCtrl splitterCtrl))
                {
                    for (int a = 0; a < saveData.filters.Count; a++)
                    {
                        FilterSaveData filterSaveData = saveData.filters[a];
                        if(filterSaveData.filterItemIndex != -1)
                            splitterCtrl.GameStartFillterSet(a, filterSaveData.filterOn, filterSaveData.filterInvert, filterSaveData.filterItemIndex);
                    }
                }
                else if (structure.TryGetComponent(out Unloader unloader))
                {
                    FilterSaveData filterSaveData = saveData.filters[0];
                    if (filterSaveData.filterItemIndex != -1)
                        unloader.GameStartFillterSet(filterSaveData.filterItemIndex);
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
        GameObject spawnobj = Instantiate(beltGroup);
        spawnobj.TryGetComponent(out NetworkObject netObj);
        if (!netObj.IsSpawned) spawnobj.GetComponent<NetworkObject>().Spawn();
        spawnobj.transform.parent = beltMgr.transform;
        spawnobj.TryGetComponent(out BeltGroupMgr beltGroupMgr);
        
        foreach (var beltData in saveData.beltList)
        {
            Building building = GeminiNetworkManager.instance.GetBuildingSOFromIndex(beltData.Item2.index);
            Vector3 spawnPos = Vector3Extensions.ToVector3(beltData.Item2.pos);
            GameObject beltObj = Instantiate(building.gameObj, spawnPos, Quaternion.identity);
            beltObj.TryGetComponent(out NetworkObject netBeltObj);
            if (!netBeltObj.IsSpawned) beltObj.GetComponent<NetworkObject>().Spawn();

            if (netBeltObj.TryGetComponent(out Structure structure))
            {
                structure.GameStartSpawnSet(beltData.Item2.level, beltData.Item2.direction, building.height, building.width, beltData.Item2.planet, beltData.Item2.index);
                structure.MapDataSaveClientRpc(Vector3Extensions.ToVector3(beltData.Item2.tileSetPos));
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
        beltGroupMgr.isSetBuildingOk = true;
    }

    void SetConnectedFunc()
    {
        foreach (Transporter transporter in transporters)
        {
            GameObject findObj = CellObjFind(transporter.connectedPosList[0], transporter.isInHostMap);
            if (findObj != null && findObj.TryGetComponent(out Transporter takeTransporter))
            {
                transporter.TakeBuildSet(takeTransporter);
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
        if (!netObj.IsSpawned) spawnobj.GetComponent<NetworkObject>().Spawn();

        spawnobj.transform.position = Vector3Extensions.ToVector3(unitSaveData.pos);
        spawnobj.GetComponent<UnitAi>().GameStartSet(unitSaveData);
    }

    void SetSpawnerManager(SpawnerManagerSaveData spawnerManagerSaveData)
    {
        levelData = SpawnerSetManager.instance.arealevelData;
        if(spawnerManagerSaveData.splitCount != 0)
            MonsterSpawnerManager.instance.SplitCountSet(spawnerManagerSaveData.splitCount);

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
            MonsterSpawnerManager.instance.AreaGroupSet(monsterSpawner, spawnerSaveData.level, planet);
            monsterSpawner.groupManager = spawnerGroup;
            monsterSpawner.GameStartSet(spawnerSaveData, levelData[spawnerSaveData.level - 1], Vector3Extensions.ToVector3(spawnerSaveData.wavePos), planet);
            if(spawnerSaveData.waveState)
                monsterSpawner.GameStartWaveSet(spawnerSaveData.waveTimer);
            SetSpawner(monsterSpawner, spawnerSaveData, planet, spawnerSaveData.waveState);
        }

        return spawnerGroupObj;
    }

    void SetSpawner(MonsterSpawner monsterSpawner, SpawnerSaveData spawnerSaveData, bool planet, bool waveState)
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
                if (waveState)
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
            }

            monster.GetComponent<MonsterAi>().GameStartSet(unitSaveData);
        }
    }

    GameObject SpawnSpawner(SpawnerSaveData spawnerSaveData)
    {
        GameObject spawnerObj = Instantiate(spawner);
        NetworkObject networkObject = spawnerObj.GetComponent<NetworkObject>();
        if (!networkObject.IsSpawned) networkObject.Spawn();
        spawnerObj.transform.position = Vector3Extensions.ToVector3(spawnerSaveData.spawnerPos);

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
