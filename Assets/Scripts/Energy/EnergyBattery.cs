using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class EnergyBattery : Structure
{
    /*
     * 에너지 저장량, 최대 저장량
     * 에너지 축적, 소모 메서드
     */

    public float capacity;
    public float stored;

    protected override void Awake()
    {
        base.Awake();
        stored = 0;
    }

    protected override void OnClientConnectedCallback(ulong clientId)
    {
        base.OnClientConnectedCallback(clientId);
        StoredSyncServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    void StoredSyncServerRpc()
    {
        StoredSyncClientRpc(stored);
    }

    [ClientRpc]
    void StoredSyncClientRpc(float syncStored)
    {
        if (IsServer)
            return;
        stored = syncStored;
    }

    public float StoreEnergy(float energy)
    {
        //return값: 용량이 다 차서 저장하지 못한 에너지 반환
        float canStore = capacity - stored;
        if (capacity > stored)
        {
            if (energy > canStore)
            {
                stored = capacity;
            }
            else
            {
                stored += energy;
            }
        }

        if (energy > canStore)
        {
            return (energy - canStore);
        }
        else
        {
            return 0;
        }
    }

    public float PullEnergy(float energy)
    {
        // return값: 기본 0, 에너지 요구량이 배터리 저장량보다 큰 경우 부족한 만큼 반환
        if (energy <= stored)
        {
            stored -= energy;
            return 0;
        }
        else
        {
            float temp = energy - stored;
            stored = 0;
            return temp;
        }
    }

    public override void AddConnector(EnergyGroupConnector connector)
    {
        if (!connectors.Contains(connector))
        {
            connectors.Add(connector);
            if (conn == null)
            {
                conn = connector;
                conn.AddBattery(this);
            }
        }
    }

    public override void RemoveConnector(EnergyGroupConnector connector)
    {
        if (connectors.Contains(connector))
        {
            connectors.Remove(connector);
            if (conn == connector)
            {
                conn.RemoveBattery(this);
                conn = null;
                if (connectors.Count > 0)
                {
                    conn = connectors[0];
                    conn.AddBattery(this);
                }
            }
        }
    }

    public override StructureSaveData SaveData()
    {
        StructureSaveData data = base.SaveData();


        return data;
    }
}
