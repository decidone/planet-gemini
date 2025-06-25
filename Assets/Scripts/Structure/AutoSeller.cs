using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Netcode;

public class AutoSeller : Production
{
    [SerializeField]
    GameObject trUnit;
    TransportUnit transportUnit;
    NetworkVariable<bool> isUnitInStr = new NetworkVariable<bool>();
    //bool isUnitInStr;
    bool isTransportable;
    [SerializeField]
    MerchandiseListSO toolShopMerchListSO;
    List<Merchandise> merchList = new List<Merchandise>();
    List<Item> merchItems = new List<Item>();

    int maxSendAmount;
    //public int sendAmount;

    //float transportInterval;
    //float transportTimer;

    Dictionary<Item, int> invItemCheckDic = new Dictionary<Item, int>();

    protected override void Start()
    {
        base.Start();
        isRunning = true;
        maxFuel = 100;
        //transportTimer = 1.0f;
        isStorageBuilding = true;
        if (IsServer)
            isUnitInStr.Value = (transportUnit == null);

        merchList = toolShopMerchListSO.MerchandiseSOList;
        foreach (var merch in merchList)
        {
            if (!merchItems.Contains(merch.item))
            {
                merchItems.Add(merch.item);
            }
        }

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
            if (isTransportable)
            {
                prodTimer += Time.deltaTime;
                if (prodTimer > cooldown)
                {
                    if (isUnitInStr.Value)
                    {
                        if (IsServer)
                        {
                            SendTransportItemDicCheck();
                        }
                        prodTimer = 0;
                    }
                }
            }
            else
            {
                prodTimer = 0;
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

            if (invenItem.item != null && merchItems.Contains(invenItem.item))
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
        //sInvenManager.TransporterSetting(isToggleOn, sendAmount);

        //if (takeBuild != null)
        //    LineRendererSet(takeBuild.transform.position);
    }

    public override void CloseUI()
    {
        base.CloseUI();
        sInvenManager.ReleaseInven();
    }

    protected override void OnClientConnectedCallback(ulong clientId)
    {
        base.OnClientConnectedCallback(clientId);
        //SendFuncSetServerRpc(sendAmount);
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

    void SendTransportItemDicCheck()
    {
        maxSendAmount = 99;
        int Sendcalculate = 0;
        Dictionary<Item, int> tempInvItemCheckDic = new Dictionary<Item, int>();

        for (int i = 0; i < inventory.space; i++)
        {
            var invenItem = inventory.SlotCheck(i);

            if (invenItem.item != null && merchItems.Contains(invenItem.item))
            {
                int availableAmount = Mathf.Min(invenItem.amount, maxSendAmount - Sendcalculate);

                if (!tempInvItemCheckDic.ContainsKey(invenItem.item))
                {
                    tempInvItemCheckDic.Add(invenItem.item, availableAmount);
                }
                else
                {
                    tempInvItemCheckDic[invenItem.item] += availableAmount;
                }

                Sendcalculate += availableAmount;

                if (Sendcalculate >= maxSendAmount)
                    break;
            }
        }

        this.invItemCheckDic = tempInvItemCheckDic;

        //UnitSendOpen();
        OpenAnimServerRpc("Open");
    }

    public void RemoveUnit(GameObject returnUnit)
    {
        if (IsServer)
            isUnitInStr.Value = true;
        transportUnit = null;
        //Destroy(returnUnit);
        returnUnit.GetComponent<TransportUnit>().DestroyFunc();
        OpenAnimServerRpc("ItemGetOpen");
    }

    public void UnitSendOpen()
    {
        if (IsServer && invItemCheckDic != null && invItemCheckDic.Count > 0)
        {
            GameObject unit = Instantiate(trUnit, transform.position, Quaternion.identity);
            unit.TryGetComponent(out NetworkObject netObj);
            if (!netObj.IsSpawned) unit.GetComponent<NetworkObject>().Spawn(true);
            if (IsServer)
                isUnitInStr.Value = false;
            transportUnit = unit.GetComponent<TransportUnit>();
            transportUnit.SetUnitColorIndex(1);

            Vector3 portalPos;
            if (this.isInHostMap)
                portalPos = GameManager.instance.hostPlayerSpawnPos;
            else
                portalPos = GameManager.instance.clientPlayerSpawnPos;

            int totalPrice = 0;
            foreach (var dicData in invItemCheckDic)
            {
                foreach (var merch in merchList)
                {
                    if (merch.item == dicData.Key)
                    {
                        totalPrice += (merch.sellPrice * dicData.Value);
                        break;
                    }
                }
            }

            unit.GetComponent<TransportUnit>().MovePosSet(this, portalPos, invItemCheckDic, totalPrice);
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
            if (list.name == "AutoSeller")
            {
                ui = list;
            }
        }
    }

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

    //public void SendFuncSet(bool toggleOn, int amount)
    //{
    //    isToggleOn = toggleOn;
    //    sendAmount = amount;
    //}

    //[ServerRpc(RequireOwnership = false)]
    //public void SendFuncSetServerRpc(int amount)
    //{
    //    SendFuncSetClientRpc(amount);
    //}

    //[ClientRpc]
    //void SendFuncSetClientRpc(int amount)
    //{
    //    sendAmount = amount;
    //    //sInvenManager.TransporterResetUI();
    //}

    public void RemoveFunc()
    {
        if (transportUnit != null)
            transportUnit.MainTrBuildRemove();
    }

    public void TrUnitToHomelessDrone()
    {
        //건물이 파괴될 때 소유한 드론이 있는 경우 HomelessDroneManager에 인계
        if (transportUnit != null)
        {
            HomelessDroneManager.instance.AddDrone(transportUnit);
        }
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

        Vector3 portalPos;
        if (this.isInHostMap)
            portalPos = GameManager.instance.hostPlayerSpawnPos;
        else
            portalPos = GameManager.instance.clientPlayerSpawnPos;

        int totalPrice = 0;
        foreach (var dicData in item)
        {
            foreach (var merch in toolShopMerchListSO.MerchandiseSOList)
            {
                if (merch.item == dicData.Key)
                {
                    totalPrice += (merch.sellPrice * dicData.Value);
                    break;
                }
            }
        }

        TransportUnit unitScript = unit.GetComponent<TransportUnit>();
        transportUnit = unitScript;
        if (IsServer)
            isUnitInStr.Value = false;
        transportUnit.SetUnitColorIndex(1);
        unitScript.MovePosSet(this, portalPos, item, totalPrice);
        if (item.Count == 0)
            unitScript.TakeItemEnd(false);
    }

    public override StructureSaveData SaveData()
    {
        StructureSaveData data = base.SaveData();

        if (transportUnit != null)
        {
            SerializedVector3 vector3 = Vector3Extensions.FromVector3(transportUnit.transform.position);
            data.trUnitPosData.Add(vector3);
            Dictionary<int, int> itemSave = new Dictionary<int, int>();
            foreach (var itemData in transportUnit.itemDic)
            {
                itemSave.Add(GeminiNetworkManager.instance.GetItemSOIndex(itemData.Key), itemData.Value);
            }

            Dictionary<int, Dictionary<int, int>> itemDataSave = new Dictionary<int, Dictionary<int, int>>();
            itemDataSave.Add(0, itemSave);
            data.trUnitItemData = itemDataSave;
        }

        return data;
    }
}
