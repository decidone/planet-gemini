using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GeminiNetworkManager : NetworkBehaviour
{
    [SerializeField]
    Transform hostChar;
    [SerializeField]
    Transform clientChar;
    [SerializeField]
    public ItemListSO itemListSO;
    [SerializeField]
    public BuildingListSO buildingListSO;
    [SerializeField]
    public UnitListSO unitListSO;
    public GameObject itemPref;

    public delegate void OnItemDestroyed();
    public OnItemDestroyed onItemDestroyedCallback;

    #region Singleton
    public static GeminiNetworkManager instance;

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

    [ServerRpc]
    public void HostSpawnServerRPC(ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        Transform playerTransform = Instantiate(hostChar);
        GameManager.instance.hostPlayerTransform = playerTransform;
        playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        PlayerObjSpawnDoneClientRpc(clientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ClientSpawnServerRPC(ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        Transform playerTransform = Instantiate(clientChar);
        GameManager.instance.clientPlayerTransform = playerTransform;
        playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        PlayerObjSpawnDoneClientRpc(clientId);
    }

    [ClientRpc]
    private void PlayerObjSpawnDoneClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            GameManager.instance.LoadingEnd();
        }
    }

    public int GetItemSOIndex(Item item)
    {
        return itemListSO.itemSOList.IndexOf(item);
    }

    public Item GetItemSOFromIndex(int itemSOIndex)
    {
        return itemListSO.itemSOList[itemSOIndex];
    }

    public int GetBuildingSOIndex(Building building)
    {
        return buildingListSO.buildingSOList.IndexOf(building);
    }

    public Building GetBuildingSOFromIndex(int itemSOIndex)
    {
        return buildingListSO.buildingSOList[itemSOIndex];
    }

    public int GetMonsterSOIndex(GameObject obj, int monsterType, bool isUserUnit)
    {
        int index = -1;

        if (isUserUnit)
        {
            index = GameObjFindIndex(unitListSO.userUnitList, obj);
        }
        else
        {
            if(monsterType == 0)
            {
                index = GameObjFindIndex(unitListSO.weakMonsterList, obj);
            }
            else if (monsterType == 1)
            {
                index = GameObjFindIndex(unitListSO.normalMonsterList, obj);
            }
            else if (monsterType == 2)
            {
                index = GameObjFindIndex(unitListSO.strongMonsterList, obj);
            }
            else if (monsterType == 3)
            {
                index = GameObjFindIndex(unitListSO.guardian, obj);
            }
        }

        return index;
    }

    int GameObjFindIndex(List<GameObject> objList, GameObject obj)
    {
        int index = -1;
        UnitCommonData objData = obj.GetComponent<UnitCommonAi>().unitCommonData;

        for (int i = 0; i < objList.Count; i++)
        {
            UnitCommonData findData = objList[i].GetComponent<UnitCommonAi>().unitCommonData;
            if(objData == findData)
            {
                index = i;
            }
        }

        return index;
    }

    public GameObject GetMonsterSOFromIndex(int itemSOIndex, int monsterType, bool isUserUnit)
    {
        GameObject obj = null;

        if (isUserUnit)
        {
            obj = unitListSO.userUnitList[itemSOIndex];
        }
        else
        {
            if (monsterType == 0)
            {
                obj = unitListSO.weakMonsterList[itemSOIndex];
            }
            else if (monsterType == 1)
            {
                obj = unitListSO.normalMonsterList[itemSOIndex];
            }
            else if (monsterType == 2)
            {
                obj = unitListSO.strongMonsterList[itemSOIndex];
            }
            else if (monsterType == 3)
            {
                obj = unitListSO.guardian[itemSOIndex];
            }
        }

        return obj;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ItemSpawnServerRpc(int itemIndex, int amount, Vector3 spawnPos)
    {
        Item item = GeminiNetworkManager.instance.GetItemSOFromIndex(itemIndex);
        Debug.Log("Item : " + item.name + ", Amount : " + amount);

        //GameObject dropItem = Instantiate(itemPref, spawnPos, Quaternion.identity);
        //NetworkObject itemNetworkObject = dropItem.GetComponent<NetworkObject>();
        NetworkObject itemNetworkObject = NetworkObjectPool.Singleton.GetNetworkObject(itemPref, spawnPos, Quaternion.identity);
        ItemProps itemProps = itemNetworkObject.GetComponent<ItemProps>();
        NetworkItemPoolSync.instance.NetPoolItemSet(itemProps);
        itemProps.waitingForDestroy = false;
        if (!itemNetworkObject.IsSpawned) itemNetworkObject.Spawn(true);

        SetItemPropsClientRpc(itemNetworkObject, itemIndex, amount);
    }

    [ClientRpc]
    public void SetItemPropsClientRpc(NetworkObjectReference networkObjectReference, int itemIndex, int amount)
    {
        networkObjectReference.TryGet(out NetworkObject itemNetworkObject);
        Item item = GeminiNetworkManager.instance.GetItemSOFromIndex(itemIndex);
        SpriteRenderer sprite = itemNetworkObject.GetComponent<SpriteRenderer>();
        sprite.sprite = item.icon;
        sprite.material = ResourcesManager.instance.outlintMat;
        ItemProps itemProps = itemNetworkObject.GetComponent<ItemProps>();
        itemProps.item = item;
        itemProps.amount = amount;
    }

    public void DestroyItem(NetworkObject itemObj)
    {
        DestroyItemServerRpc(itemObj.GetComponent<NetworkObject>());
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyItemServerRpc(NetworkObjectReference networkObjectReference)
    {
        networkObjectReference.TryGet(out NetworkObject networkObject);
        if (networkObject != null)
        {
            NetworkItemPoolSync.instance.NetPoolItemSub(networkObject.GetComponent<ItemProps>());
            networkObject.Despawn();
            //Destroy(networkObject.gameObject);
            onItemDestroyedCallback?.Invoke();
        }
    }

    public (string, byte[]) RequestJson()
    {
        var data = DataManager.instance.Save(0);
        string json = data.Item1;
        SaveData saveData = JsonConvert.DeserializeObject<SaveData>(json);
        SaveData clientData = new SaveData();

        clientData.InGameData = saveData.InGameData;
        clientData.hostPlayerData = saveData.hostPlayerData;
        clientData.clientPlayerData = saveData.clientPlayerData;
        clientData.hostMapInvenData = saveData.hostMapInvenData;
        clientData.clientMapInvenData = saveData.clientMapInvenData;
        clientData.scienceData = saveData.scienceData;
        clientData.overallData = saveData.overallData;
        //clientData.mapData = saveData.mapData;
        Debug.Log("request 1");

        return (JsonConvert.SerializeObject(clientData), data.Item2);
    }

    //[ClientRpc]
    //public void RequestJsonClientRpc(string json)
    //{
    //    if (IsServer)
    //        return;

    //    DataManager.instance.Load(json);
    //    MonsterSpawnerManager.instance.SetCorruption();
    //    GameManager.instance.SyncTimeServerRpc();
    //}
}
