using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class HomelessDroneManager : MonoBehaviour
{
    [SerializeField] GameObject trUnit;
    public List<TransportUnit> drones;

    #region Singleton
    public static HomelessDroneManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }
    #endregion

    public void AddDrone(TransportUnit drone)
    {
        if (!drones.Contains(drone))
        {
            drones.Add(drone);
        }
    }

    public void RemoveDrone(TransportUnit drone)
    {
        if (drones.Contains(drone))
        {
            drones.Remove(drone);
        }
    }

    public List<HomelessDroneSaveData> SaveDroneData()
    {
        List<HomelessDroneSaveData> dataList = new List<HomelessDroneSaveData>();

        foreach (var drone in drones)
        {
            HomelessDroneSaveData data = new HomelessDroneSaveData();

            if (drone.trUnitState == TrUnitState.idle)
            {
                if (drone.isHomelessUnit)
                    data.state = "homeless";
                else
                    data.state = "idle";
            }
            else if (drone.trUnitState == TrUnitState.trMove)
                data.state = "move";
            else if (drone.trUnitState == TrUnitState.returnBuild)
                data.state = "return";

            data.startPos = Vector3Extensions.FromVector3(drone.startPos);
            data.endPos = Vector3Extensions.FromVector3(drone.endPos);
            data.pos = Vector3Extensions.FromVector3(drone.transform.position);

            Dictionary<int, int> items = new Dictionary<int, int>();
            foreach (KeyValuePair<Item, int> kv in drone.itemDic)
            {
                if (!(drone.isBuyerUnit && kv.Key == ItemList.instance.itemDic["CopperGoblet"]))
                    items.Add(GeminiNetworkManager.instance.GetItemSOIndex(kv.Key), kv.Value);
            }
            data.invenItemData = items;

            dataList.Add(data);
        }

        return dataList;
    }

    public void LoadDroneData(List<HomelessDroneSaveData> dataList)
    {
        foreach (var data in dataList)
        {
            SpawnDrone(data);
        }
    }

    void SpawnDrone(HomelessDroneSaveData saveData)
    {
        GameObject unit = Instantiate(trUnit, Vector3Extensions.ToVector3(saveData.pos), Quaternion.identity);
        unit.TryGetComponent(out NetworkObject netObj);
        if (!netObj.IsSpawned) netObj.Spawn(true);
        unit.GetComponent<TransportUnit>().SetHomelessDrone(saveData);

        // 스폰시킨 드론들이 사라지기 전에 다시 세이브/로드할 것을 대비해서 리스트에 저장
        AddDrone(unit.GetComponent<TransportUnit>());
    }
}
