using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Netcode;
using System.Xml.Linq;

public class AutoBuyer : Production
{
    [SerializeField]
    GameObject trUnit;
    TransportUnit transportUnit;
    NetworkVariable<bool> isUnitInStr = new NetworkVariable<bool>();
    bool isTransportable;
    bool isBuyable;
    [SerializeField]
    MerchandiseListSO oreShopMerchListSO;
    [SerializeField]
    MerchandiseListSO manaStoneShopMerchListSO;
    List<Merchandise> merchList = new List<Merchandise>();

    public int maxBuyAmount;    // 구매할 수 있는 최대 수량
    public int minBuyAmount;    // 아이템 보유 수량이 해당 변수 아래로 내려갈 때 (최대 수량 - 현재 수량)만큼 구매
    List<Dictionary<Item, int>> unitItemList = new List<Dictionary<Item, int>>();
    List<TransportUnit> getItemUnit = new List<TransportUnit>();

    float transportTimer;
    float transportInterval;

    Dictionary<Item, int> invItemCheckDic = new Dictionary<Item, int>();

    protected override void Start()
    {
        base.Start();
        isRunning = true;
        maxFuel = 100;
        transportInterval = 1.0f;
        isStorageBuilding = false;
        if (IsServer)
            isUnitInStr.Value = (transportUnit == null);

        merchList = oreShopMerchListSO.MerchandiseSOList.Concat(manaStoneShopMerchListSO.MerchandiseSOList).ToList();
        inventory.onItemChangedCallback += TransportableCheck;
        inventory.invenAllSlotUpdate += TransportableCheck;
        GameManager.instance.onFinanceChangedCallback += BuyableCheck;
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
                    if (isUnitInStr.Value && isBuyable)
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

            if (IsServer && slot.Item2 > 0 && outObj.Count > 0 && !itemSetDelay && checkObj)
            {
                int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(output);
                SendItem(itemIndex);
            }
            if (DelaySendList.Count > 0 && outObj.Count > 0 && !outObj[DelaySendList[0].Item2].GetComponent<Structure>().isFull)
            {
                SendDelayFunc(DelaySendList[0].Item1, DelaySendList[0].Item2, 0);
            }
        }
    }

    public override void CheckSlotState(int slotindex)
    {
        // update에서 검사해야 하는 특정 슬롯들 상태를 인벤토리 콜백이 있을 때 미리 저장
        slot = inventory.SlotCheck(0);
    }

    public void MaxSliderUIValueChanged(int amount)
    {
        MaxSliderValueSyncServerRpc(amount);
    }

    [ServerRpc(RequireOwnership = false)]
    public void MaxSliderValueSyncServerRpc(int amount)
    {
        MaxSliderValueSyncClientRpc(amount);
    }

    [ClientRpc]
    public void MaxSliderValueSyncClientRpc(int amount)
    {
        AutoBuyerManager buyerManager = AutoBuyerManager.instance;
        if (isUIOpened && buyerManager.buyer == this)
        {
            buyerManager.SetMaxSliderValue(amount);
        }
        maxBuyAmount = amount;
        TransportableCheck(0);
    }

    public void MinSliderUIValueChanged(int amount)
    {
        MinSliderValueSyncServerRpc(amount);
    }

    [ServerRpc(RequireOwnership = false)]
    public void MinSliderValueSyncServerRpc(int amount)
    {
        MinSliderValueSyncClientRpc(amount);
    }

    [ClientRpc]
    public void MinSliderValueSyncClientRpc(int amount)
    {
        AutoBuyerManager buyerManager = AutoBuyerManager.instance;
        if (isUIOpened && buyerManager.buyer == this)
        {
            buyerManager.SetMinSliderValue(amount);
        }
        minBuyAmount = amount;
        TransportableCheck();
    }

    void TransportableCheck()
    {
        TransportableCheck(0);
    }

    public void TransportableCheck(int slotIndex)
    {
        if (output == null)
        {
            isTransportable = false;
        }
        else
        {
            if (slot.Item1 == null)
            {
                if (maxBuyAmount > 0)
                {
                    isTransportable = true;
                }
                else
                {
                    isTransportable = false;
                }
            }
            else
            {
                isTransportable = (slot.Item2 < minBuyAmount);
            }
        }

        if (isTransportable)
        {
            BuyableCheck();
        }
    }

    public void BuyableCheck()
    {
        int availableAmount = 0;
        if (slot.Item1 != null)
        {
            availableAmount = maxBuyAmount - slot.Item2;
        }
        else
        {
            availableAmount = maxBuyAmount;
        }

        int totalPrice = 0;
        foreach (var merch in merchList)
        {
            if (merch.item == output)
            {
                totalPrice = (merch.buyPrice * availableAmount);
                break;
            }
        }

        isBuyable = (GameManager.instance.finance.finance >= totalPrice);
    }

    public override void OpenUI()
    {
        base.OpenUI();
        sInvenManager.SetInven(inventory, ui);
        sInvenManager.SetProd(this);
        sInvenManager.progressBar.SetMaxProgress(cooldown);
        sInvenManager.SetCooldownText(cooldown);

        sInvenManager.InvenInit();
        if (recipe.name != null)
            SetRecipe(recipe, recipeIndex);

        AutoBuyerManager.instance.SetBuyer(this);
    }

    public override void CloseUI()
    {
        base.CloseUI();
        sInvenManager.ReleaseInven();
        AutoBuyerManager.instance.ResetValue();
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

    public override void OpenRecipe()
    {
        if (!isUnitInStr.Value) return;

        rManager.OpenUI();
        rManager.SetRecipeUI("AutoBuyer", this);
    }

    public override void SetRecipe(Recipe _recipe, int index)
    {
        base.SetRecipe(_recipe, index);
        //output = itemDic[recipe.items[0]];
        sInvenManager.slots[0].SetInputItem(itemDic[recipe.items[0]]);
        sInvenManager.slots[0].outputSlot = true;
    }
    public override void SetOutput(Recipe recipe)
    {
        output = itemDic[recipe.items[0]];
    }

    protected override void OnClientConnectedCallback(ulong clientId)
    {
        base.OnClientConnectedCallback(clientId);
        ClientBuyerSyncServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ClientBuyerSyncServerRpc()
    {
        ClientBuyerSyncClientRpc(maxBuyAmount, minBuyAmount);
    }

    [ClientRpc]
    public void ClientBuyerSyncClientRpc(int max, int min)
    {
        if (!IsServer)
        {
            maxBuyAmount = max;
            minBuyAmount = min;
        }
    }

    void SendTransportItemDicCheck()
    {
        Dictionary<Item, int> tempInvItemCheckDic = new Dictionary<Item, int>();

        var invenItem = inventory.SlotCheck(0);

        if (invenItem.item != null)
        {
            if (invenItem.amount < minBuyAmount)
            {
                int availableAmount = maxBuyAmount - invenItem.amount;
                tempInvItemCheckDic.Add(invenItem.item, availableAmount);
            }
        }
        else
        {
            // 레시피 통해서 아웃풋 지정해줘야 함
            if (output != null)
                tempInvItemCheckDic.Add(output, maxBuyAmount);
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
            int totalPrice = 0;
            foreach (var merch in merchList)
            {
                if (merch.item == output)
                {
                    totalPrice = (merch.buyPrice * invItemCheckDic[output]);
                    break;
                }
            }

            if (GameManager.instance.finance.finance >= totalPrice && totalPrice != 0)
            {
                GameObject unit = Instantiate(trUnit, transform.position, Quaternion.identity);
                unit.TryGetComponent(out NetworkObject netObj);
                if (!netObj.IsSpawned) unit.GetComponent<NetworkObject>().Spawn(true);

                if (IsServer)
                    isUnitInStr.Value = false;
                transportUnit = unit.GetComponent<TransportUnit>();
                transportUnit.SetUnitColorIndex(0);
                Vector3 portalPos;
                if (this.isInHostMap)
                    portalPos = GameManager.instance.hostPlayerSpawnPos;
                else
                    portalPos = GameManager.instance.clientPlayerSpawnPos;

                GameManager.instance.SubFinanceServerRpc(totalPrice);
                invItemCheckDic.Add(ItemList.instance.itemDic["CopperGoblet"], 0);
                unit.GetComponent<TransportUnit>().MovePosSet(this, portalPos, invItemCheckDic);
            }
            else
            {
                Debug.Log("Not enough money or lack of input");
            }
        }
    }

    //public override bool CanTakeItem(Item item)
    //{
    //    return false;
    //}

    //public override void OnFactoryItem(ItemProps itemProps)
    //{
    //    if (IsServer)
    //        inventory.StorageAdd(itemProps.item, itemProps.amount);
    //    itemProps.itemPool.Release(itemProps.gameObject);
    //}

    //public override void OnFactoryItem(Item item)
    //{
    //    if (IsServer)
    //        inventory.StorageAdd(item, 1);
    //}

    public override void GetUIFunc()
    {
        InventoryList inventoryList = canvas.GetComponent<InventoryList>();

        foreach (GameObject list in inventoryList.StructureStorageArr)
        {
            if (list.name == "AutoBuyer")
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

    public void TakeTransportItem(TransportUnit takeUnit, Dictionary<Item, int> _itemDic)
    {
        if (_itemDic != null && _itemDic.Count > 0)
        {
            unitItemList.Add(new Dictionary<Item, int>(_itemDic));
            getItemUnit.Add(takeUnit);
            ExStorageCheck();
            OpenAnimServerRpc("ItemGetOpen");
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
            getItemUnit[0].TakeItemEnd(true);
            getItemUnit.RemoveAt(0);
        }
    }

    //public void SendFuncSet(bool toggleOn, int amount)
    //{
    //    isToggleOn = toggleOn;
    //    sendAmount = amount;
    //}

    //[ServerRpc(RequireOwnership = false)]
    //public void SendFuncSetServerRpc(bool toggleOn, int amount)
    //{
    //    SendFuncSetClientRpc(toggleOn, amount);
    //}

    //[ClientRpc]
    //void SendFuncSetClientRpc(bool toggleOn, int amount)
    //{
    //    isToggleOn = toggleOn;
    //    sendAmount = amount;
    //    sInvenManager.TransporterResetUI();
    //}

    public void RemoveFunc()
    {
        if (transportUnit !=  null)
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

        TransportUnit unitScript = unit.GetComponent<TransportUnit>();
        transportUnit = unitScript;
        transportUnit.SetUnitColorIndex(0);
        unitScript.MovePosSet(this, portalPos, item);

        //보낼 때 체크용 아이템을 하나 넣어두고 리턴할 때 삭제함. 따라서 아이템이 1개 있는 경우 돌아오는 드론
        if (item.Count <= 1)
            unitScript.TakeItemEnd(false);
    }

    public override StructureSaveData SaveData()
    {
        StructureSaveData data = base.SaveData();
        data.maxBuyAmount = this.maxBuyAmount;
        data.minBuyAmount = this.minBuyAmount;

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
