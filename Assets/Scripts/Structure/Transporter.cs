using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Netcode;

public class Transporter : Production
{
    public Transporter takeBuild;
    public List<Transporter> sendBuildList = new List<Transporter>();

    [SerializeField]
    GameObject trUnit;
    NetworkVariable<bool> isUnitInStr = new NetworkVariable<bool>();
    bool isTransportable;
    List<GameObject> sendItemUnit = new List<GameObject>();

    int maxSendAmount;
    public bool isToggleOn = false;
    public int sendAmount;
    List<Dictionary<Item, int>> unitItemList = new List<Dictionary<Item, int>>();
    public int standbyUnitCount;
    List<TransportUnit> getItemUnit = new List<TransportUnit>();

    float transportTimer;
    float transportInterval;

    [HideInInspector]
    public MapClickEvent clickEvent;
    Transporter othTransporter;
    Dictionary<Item, int> invItemCheckDic = new Dictionary<Item, int>();

    protected override void Start()
    {
        base.Start();
        isRunning = true;
        maxFuel = 100;
        transportInterval = 1.0f;
        isStorageBuilding = true;
        clickEvent = GetComponent<MapClickEvent>();
        if (IsServer)
            isUnitInStr.Value = true;

        inventory.onItemChangedCallback += TransportableCheck;
        inventory.invenAllSlotUpdate += TransportableCheck;

    }

    protected override void Update()
    {
        base.Update();

        if (isDestroying)
        {
            isDestroying = false;
            isRunning = false;
            RemoveFunc();
        }

        if (!isPreBuilding && isRunning)
        {
            if (takeBuild != null && isTransportable)
            {
                prodTimer += Time.deltaTime;
                if (prodTimer > cooldown)
                {
                    if (isUnitInStr.Value && takeBuild.standbyUnitCount < 2)
                    {
                        if (!isToggleOn)
                        {
                            if (IsServer)
                            {
                                SendTransportItemDicCheck(takeBuild);
                            }
                            prodTimer = 0;
                        }
                        else
                        {
                            if (sendAmount != 0 && inventory.TotalItemsAmountLimitCheck(sendAmount))
                            {
                                if (IsServer)
                                {
                                    SendTransportItemDicCheck(takeBuild);
                                }
                                prodTimer = 0;
                            }
                        }
                    }
                }
            }
            else
                prodTimer = 0;

            if (IsServer)
            {
                if (unitItemList.Count > 0)
                {
                    transportTimer += Time.deltaTime;
                    if (transportTimer > transportInterval)
                    {
                        if (IsServer)
                            ExStorageCheck();
                        transportTimer = 0;
                    }
                }
                else
                    transportTimer = 0;
            }
        }
    }

    void TransportableCheck()
    {
        TransportableCheck(0);
    }

    public void TransportableCheck(int slotindex)
    {
        isTransportable = false;

        for (int i = 0; i < inventory.space; i++)
        {
            var invenItem = inventory.SlotCheck(i);

            if (invenItem.item != null)
            {
                isTransportable = true;
                break;
            }
        }
    }

    public override void OpenUI()
    {
        base.OpenUI();
        sInvenManager.SetInven(inventory, ui);
        sInvenManager.SetProd(this);
        //sInvenManager.progressBar.SetMaxProgress(effiCooldown - effiOverclock);
        sInvenManager.progressBar.SetMaxProgress(cooldown);
        sInvenManager.SetCooldownText(cooldown);
        sInvenManager.TransporterSetting(isToggleOn, sendAmount);
        sInvenManager.sortBtn.gameObject.SetActive(true);
        sInvenManager.sortBtn.onClick.AddListener(SortInven);

        //if (takeBuild != null)
        //    LineRendererSet(takeBuild.transform.position);
    }

    public override void CloseUI()
    {
        base.CloseUI();
        sInvenManager.sortBtn.gameObject.SetActive(false);
        sInvenManager.sortBtn.onClick.RemoveAllListeners();
        sInvenManager.ReleaseInven();

        // ???
        //base.DestroyLineRenderer();
    }

    protected override void OnClientConnectedCallback(ulong clientId)
    {
        base.OnClientConnectedCallback(clientId);
        ConnectedSetServerRpc();
        SendFuncSetServerRpc(isToggleOn, sendAmount);
    }


    [ServerRpc(RequireOwnership = false)]
    void ConnectedSetServerRpc()
    {
        if (takeBuild != null)
        {
            ConnectedSetClientRpc(takeBuild.transform.position);
        }
    }

    [ClientRpc]
    void ConnectedSetClientRpc(Vector3 pos)
    {
        if (IsServer)
            return;

        StartCoroutine(SetInvoke(pos));
    }

    IEnumerator SetInvoke(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        Map map;
        if (isInHostMap)
            map = GameManager.instance.hostMap;
        else
            map = GameManager.instance.clientMap;

        Cell cell = map.GetCellDataFromPos(x, y);

        while (cell.structure == null)
        {
            yield return null;
        }

        GameObject findObj = cell.structure;
        if (findObj != null && findObj.TryGetComponent(out Transporter takeTransporter))
        {
            if (TryGetComponent(out MapClickEvent mapClick) && takeTransporter.TryGetComponent(out MapClickEvent othMapClick))
            {
                mapClick.GameStartSetRenderer(othMapClick);
            }
        }
    }

    void SendTransportItemDicCheck(Transporter othBuild)
    {
        if (!isToggleOn)
            maxSendAmount = 99;
        else
            maxSendAmount = sendAmount;

        int Sendcalculate = 0;
        Dictionary<Item, int> invItemCheckDic = new Dictionary<Item, int>();

        for (int i = 0; i < inventory.space; i++)
        {
            var invenItem = inventory.SlotCheck(i);

            if (invenItem.item != null)
            {
                int availableAmount = Mathf.Min(invenItem.amount, maxSendAmount - Sendcalculate);

                if (!invItemCheckDic.ContainsKey(invenItem.item))
                {
                    invItemCheckDic.Add(invenItem.item, availableAmount);
                }
                else
                {
                    invItemCheckDic[invenItem.item] += availableAmount;
                }

                Sendcalculate += availableAmount;

                if (Sendcalculate >= maxSendAmount)
                    break;
            }
        }

        othTransporter = othBuild;
        this.invItemCheckDic = invItemCheckDic;

        OpenAnimServerRpc("Open");
    }

    [ServerRpc]
    void OpenAnimServerRpc(string optionName)
    {
        OpenAnimClientRpc(optionName);
    }

    [ClientRpc]
    void OpenAnimClientRpc(string optionName)
    {
        animator.Play(optionName, -1, 0);
    }

    public void RemoveUnit(GameObject returnUnit)
    {
        sendItemUnit.Remove(returnUnit);
        if (IsServer)
        {
            if (sendItemUnit.Count < 3)
                isUnitInStr.Value = true;
            else
                isUnitInStr.Value = false;
        }
        
        //Destroy(returnUnit);
        returnUnit.GetComponent<TransportUnit>().DestroyFunc();
        OpenAnimServerRpc("ItemGetOpen");
    }

    public void UnitSendOpen()
    {
        // 애니메이션 트리거로 사용
        if (IsServer &&invItemCheckDic != null && invItemCheckDic.Count > 0)
        {
            GameObject unit = Instantiate(trUnit, transform.position, Quaternion.identity);
            unit.TryGetComponent(out NetworkObject netObj);
            if (!netObj.IsSpawned) unit.GetComponent<NetworkObject>().Spawn(true);
            
            sendItemUnit.Add(unit);
            if (IsServer)
            {
                if (sendItemUnit.Count < 3)
                    isUnitInStr.Value = true;
                else
                    isUnitInStr.Value = false;
            }

            TransportUnit transportUnit = unit.GetComponent<TransportUnit>();
            transportUnit.SetUnitColorIndex(2);

            transportUnit.MovePosSet(this, othTransporter, invItemCheckDic);
            foreach (var dicData in invItemCheckDic)
            {
                inventory.Sub(dicData.Key, dicData.Value);
            }
        }
    }

    public override bool CanTakeItem(Item item)
    {
        bool canTake = false;
        int containableAmount = inventory.SpaceCheck(item);

        if (1 <= containableAmount)
        {
            canTake = true;
        }
        else if (containableAmount != 0)
        {
            canTake = true;
        }
        else
        {
            canTake = false;
        }

        return canTake;
    }

    public override void OnFactoryItem(ItemProps itemProps)
    {
        if (IsServer)
            inventory.StorageAdd(itemProps.item, itemProps.amount);
        itemProps.itemPool.Release(itemProps.gameObject);
    }

    public override void OnFactoryItem(Item item)
    {
        if (IsServer)
            inventory.StorageAdd(item, 1);
    }

    public override void GetUIFunc()
    {
        InventoryList inventoryList = canvas.GetComponent<InventoryList>();

        foreach (GameObject list in inventoryList.StructureStorageArr)
        {
            if (list.name == "Transporter")
            {
                ui = list;
            }
        }
    }

    //public override void AddInvenItem()
    //{
    //    base.AddInvenItem();
    //    for (int i = 0; i < inventory.space; i++)
    //    {
    //        var invenItem = inventory.SlotCheck(i);

    //        if (invenItem.item != null && invenItem.amount > 0)
    //        {
    //            playerInven.Add(invenItem.item, invenItem.amount);
    //        }
    //    }
    //}

    public override Dictionary<Item, int> PopUpItemCheck()
    {
        Dictionary<Item, int> returnDic = new Dictionary<Item, int>();

        int itemsCount = 0;
        //다른 슬롯의 같은 아이템도 개수 추가하도록
        for (int i = 0; i < inventory.space; i++)
        {
            var invenItem = inventory.SlotCheck(i);

            if (invenItem.item != null && invenItem.amount > 0)
            {
                if (!returnDic.ContainsKey(invenItem.item))
                {
                    returnDic.Add(invenItem.item, invenItem.amount);
                }
                else
                {
                    returnDic[invenItem.item] += invenItem.amount;
                }
                itemsCount++;
                if (itemsCount > 5)
                    break;
            }
        }

        if (returnDic.Count > 0)
        {
            return returnDic;
        }
        else
            return null;
    }

    [ServerRpc]
    void UnitItemListSyncServerRpc()
    {
        UnitItemListSyncClientRpc(unitItemList.Count);
    }

    [ClientRpc]
    void UnitItemListSyncClientRpc(int listCount)
    {
        standbyUnitCount = listCount;
    }

    public void TakeTransportItem(TransportUnit takeUnit, Dictionary<Item, int> _itemDic)
    {
        if (_itemDic != null && _itemDic.Count > 0)
        {
            unitItemList.Add(new Dictionary<Item, int>(_itemDic));
            UnitItemListSyncServerRpc();
            getItemUnit.Add(takeUnit);
            ExStorageCheck();
            OpenAnimServerRpc("ItemGetOpen");
        }
        else
        {
            takeUnit.TakeItemEnd(false);
        }
    }

    void ExStorageCheck()
    {
        foreach (var exStorage in unitItemList[0].ToList()) // ToList()를 사용하여 복제
        {
            int containableAmount = inventory.SpaceCheck(exStorage.Key);
            if (exStorage.Value <= containableAmount)
            {
                inventory.Add(exStorage.Key, exStorage.Value);
                unitItemList[0].Remove(exStorage.Key);
            }
            else if (containableAmount != 0)
            {
                inventory.Add(exStorage.Key, containableAmount);
                unitItemList[0][exStorage.Key] -= containableAmount; // 원래 변수 수정
            }
            else
            {
                break;
            }
        }

        if (unitItemList[0].Count == 0)
        {
            unitItemList.RemoveAt(0);
            UnitItemListSyncServerRpc();
            getItemUnit[0].TakeItemEnd(true);
            getItemUnit.RemoveAt(0);
        }
    }

    //public void SendFuncSet(bool toggleOn, int amount)
    //{
    //    isToggleOn = toggleOn;
    //    sendAmount = amount;
    //}

    [ServerRpc(RequireOwnership = false)]
    public void SendFuncSetServerRpc(bool toggleOn, int amount)
    {
        SendFuncSetClientRpc(toggleOn, amount);
    }

    [ClientRpc]
    void SendFuncSetClientRpc(bool toggleOn, int amount)
    {
        isToggleOn = toggleOn;
        sendAmount = amount;
        sInvenManager.TransporterResetUI();
    }

    //public override void DestroyLineRenderer()
    //{
    //    base.DestroyLineRenderer();
    //    takeBuild = null;
    //}

    public void TakeBuildReset()
    {
        takeBuild = null;
    }

    public void TakeBuildSet(Transporter trBuild)
    {
        takeBuild = trBuild;
        trBuild.sendBuildList.Add(this);
    }

    public void RemoveFunc()
    {
        clickEvent.RemoveAllLines();

        if (takeBuild != null && takeBuild.sendBuildList.Contains(this))
        {
            takeBuild.sendBuildList.Remove(this);
        }

        foreach (GameObject trUnit in sendItemUnit)
        {
            trUnit.GetComponent<TransportUnit>().MainTrBuildRemove();
        }

        foreach (Transporter transport in sendBuildList)
        {
            transport.RemoveTakeBuild();
            foreach (GameObject trUnit in transport.sendItemUnit)
            {
                trUnit.GetComponent<TransportUnit>().TakeItemEnd(false);
            }
        }
    }

    public void TrUnitToHomelessDrone()
    {
        //건물이 파괴될 때 소유한 드론이 있는 경우 HomelessDroneManager에 인계
        if (sendItemUnit.Count > 0)
        {
            foreach (GameObject trUnit in sendItemUnit)
            {
                if (trUnit != null)
                    HomelessDroneManager.instance.AddDrone(trUnit.GetComponent<TransportUnit>());
            }
        }
    }

    public void RemoveTakeBuild()
    {
        clickEvent.RemoveAllLines();
        takeBuild = null;
    }

    public void UnitLoad(Vector3 spawnPos, Transporter othTransporter, Dictionary<int, int> itemDic)
    {
        GameObject unit = Instantiate(trUnit, spawnPos, Quaternion.identity);
        unit.TryGetComponent(out NetworkObject netObj);
        if (!netObj.IsSpawned) unit.GetComponent<NetworkObject>().Spawn(true);

        Dictionary<Item, int> item = new Dictionary<Item, int>();
        foreach (var data in itemDic)
        {
            item.Add(GeminiNetworkManager.instance.GetItemSOFromIndex(data.Key), data.Value);
        }

        sendItemUnit.Add(unit);
        if (IsServer)
        {
            if (sendItemUnit.Count < 3)
                isUnitInStr.Value = true;
            else
                isUnitInStr.Value = false;
        }
        TransportUnit transportUnit = unit.GetComponent<TransportUnit>();
        transportUnit.SetUnitColorIndex(2);
        transportUnit.MovePosSet(this, othTransporter, item);
    }

    public void UnitLoad(Vector3 spawnPos, Dictionary<int, int> itemDic)
    {
        GameObject unit = Instantiate(trUnit, spawnPos, Quaternion.identity);
        unit.TryGetComponent(out NetworkObject netObj);
        if (!netObj.IsSpawned) unit.GetComponent<NetworkObject>().Spawn(true);

        Dictionary<Item, int> item = new Dictionary<Item, int>();
        foreach (var data in itemDic)
        {
            item.Add(GeminiNetworkManager.instance.GetItemSOFromIndex(data.Key), data.Value);
        }

        sendItemUnit.Add(unit);
        if (IsServer)
        {
            if (sendItemUnit.Count < 3)
                isUnitInStr.Value = true;
            else
                isUnitInStr.Value = false;
        }

        TransportUnit unitScript = unit.GetComponent<TransportUnit>();
        unitScript.MovePosSet(this, this, item);
        unitScript.TakeItemEnd(false);
    }

    public override StructureSaveData SaveData()
    {
        StructureSaveData data = base.SaveData();

        if(takeBuild != null)
        {
            data.connectedStrPos.Add(Vector3Extensions.FromVector3(takeBuild.transform.position));
        }

        if (sendItemUnit.Count > 0)
        {
            Dictionary<int, Dictionary<int, int>> itemDataSave = new Dictionary<int, Dictionary<int, int>>();

            for (int i = 0; i < sendItemUnit.Count; i++)
            {
                SerializedVector3 vector3 = Vector3Extensions.FromVector3(sendItemUnit[i].transform.position);
                TransportUnit unit = sendItemUnit[i].GetComponent<TransportUnit>();
                data.trUnitPosData.Add(vector3);
                Dictionary<int, int> itemSave = new Dictionary<int, int>();
                foreach (var itemData in unit.itemDic)
                {
                    itemSave.Add(GeminiNetworkManager.instance.GetItemSOIndex(itemData.Key), itemData.Value);
                }

                itemDataSave.Add(i, itemSave);
            }

            data.trUnitItemData = itemDataSave;
        }

        return data;
    }
}
