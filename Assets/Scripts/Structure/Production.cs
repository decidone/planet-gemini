using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// UTF-8 설정
public abstract class Production : Structure
{
    // 연료(석탄, 전기), 작업 시간, 작업량, 재료, 생산품, 아이템 슬롯
    [SerializeField]
    protected GameObject ui;
    [SerializeField]
    protected StructureInvenManager sInvenManager;
    [SerializeField]
    protected RecipeManager rManager;

    protected GameObject canvas;
    protected Inventory inventory;
    protected Dictionary<string, Item> itemDic;
    protected float prodTimer;
    protected int fuel;
    protected int maxFuel;
    protected Item output;
    protected Recipe recipe;
    protected int recipeIndex = -1;
    protected List<Recipe> recipes;
    protected int invenCount;
    protected Dictionary<Item, int> invenSlotDic;

    [SerializeField]
    protected GameObject lineObj;
    [HideInInspector]
    public LineRenderer lineRenderer;
    protected Vector3 startLine;
    protected Vector3 endLine;
    public bool isGetLine;

    protected (Item, int) slot = (null, 0);
    protected (Item, int) slot1 = (null, 0);
    protected (Item, int) slot2 = (null, 0);
    protected (Item, int) slot3 = (null, 0);

    protected override void Awake()
    {
        base.Awake();
        inventory = this.GetComponent<Inventory>();
        inventory.onItemChangedCallback += CheckSlotState;
        isGetLine = false;
        isStorageBuilding = false;
        itemDic = ItemList.instance.itemDic;
    }

    protected virtual void Start()
    {
        itemDic = ItemList.instance.itemDic;
        if (recipe == null)
            recipe = new Recipe();

        GameManager gameManager = GameManager.instance;
        canvas = gameManager.GetComponent<GameManager>().inventoryUiCanvas;
        sInvenManager = canvas.GetComponent<StructureInvenManager>();
        rManager = canvas.GetComponent<RecipeManager>();
        GetUIFunc();
        //CheckPos();

        StrBuilt();
    }

    protected override void Update()
    {
        base.Update();

        //if (isSetBuildingOk)
        //{
        //    for (int i = 0; i < nearObj.Length; i++)
        //    {
        //        if (nearObj[i] == null && sizeOneByOne)
        //        {
        //            CheckNearObj(checkPos[i], i, obj => StartCoroutine(SetOutObjCoroutine(obj)));
        //        }
        //        else if (nearObj[i] == null && !sizeOneByOne)
        //        {
        //            int dirIndex = i / 2;
        //            CheckNearObj(startTransform[indices[i]], directions[dirIndex], i, obj => StartCoroutine(SetOutObjCoroutine(obj)));
        //        }
        //    }
        //}

        if (IsServer && !isPreBuilding && checkObj)
        {
            if (!isMainSource && inObj.Count > 0 && !itemGetDelay)
                GetItem();
        }
        if (DelayGetList.Count > 0 && inObj.Count > 0)
        {
            GetDelayFunc(DelayGetList[0], 0);
        }
    }

    public override void NearStrBuilt()
    {
        // 건물을 지었을 때나 근처에 새로운 건물이 지어졌을 때 동작
        // 필요한 경우 SetDirNum()이나 CheckPos()도 호출해서 방향, 스프라이트를 잡아줌
        CheckPos();
        for (int i = 0; i < nearObj.Length; i++)
        {
            if (nearObj[i] == null && sizeOneByOne)
            {
                CheckNearObj(checkPos[i], i, obj => StartCoroutine(SetOutObjCoroutine(obj)));
            }
            else if (nearObj[i] == null && !sizeOneByOne)
            {
                CheckNearObj(i, obj => StartCoroutine(SetOutObjCoroutine(obj)));
            }
        }
    }

    protected override void OnClientConnectedCallback(ulong clientId)
    {
        ClientConnectSyncServerRpc();
        RepairGaugeServerRpc();
        if (recipeIndex != -1)
            SetRecipeServerRpc(recipeIndex);
        if (inventory != null)
            ItemSyncServerRpc();
    }

    public virtual void SetRecipe(Recipe _recipe, int index)
    {
        recipe = _recipe;
        recipeIndex = index;
        sInvenManager.ResetInvenOption();
        cooldown = recipe.cooldown;
        effiCooldown = cooldown;
        sInvenManager.progressBar.SetMaxProgress(effiCooldown - ((overclockOn ? effiCooldown * overclockPer / 100 : 0) + effiCooldownUpgradeAmount));
        sInvenManager.SetCooldownText(effiCooldown - ((overclockOn ? effiCooldown * overclockPer / 100 : 0) + effiCooldownUpgradeAmount));
        //sInvenManager.progressBar.SetMaxProgress(cooldown);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetRecipeServerRpc(int index)
    {
        //if (itemDic == null)
        //    itemDic = ItemList.instance.itemDic;
        SetRecipeClientRpc(index);
    }

    [ClientRpc]
    public void SetRecipeClientRpc(int index)
    {
        Recipe selectRecipe = RecipeList.instance.GetRecipeIndex(structureData.factoryName, index);

        if (selectRecipe.name != "UICancel")
        {
            if (recipe != null && recipe != selectRecipe)
            {
                if (IsServer)
                {
                    AddInvenItem();
                }
                inventory.ResetInven();

                if (isUIOpened)
                {
                    sInvenManager.ClearInvenOption();
                }
            }

            if (selectRecipe != null)
            {
                if (isUIOpened)
                    SetRecipe(selectRecipe, index);
                else
                {
                    recipe = selectRecipe;
                    recipeIndex = index;
                    cooldown = recipe.cooldown;
                    effiCooldown = cooldown;
                }
            }
        }
        else
        {
            ReSetUI();
        }

        if (selectRecipe.name != "UICancel" &&  selectRecipe != null)
        {
            if (isUIOpened)
            {
                SetRecipe(selectRecipe, index);
                SetOutput(selectRecipe);
            }
            else
            {
                recipe = selectRecipe;
                recipeIndex = index;
                cooldown = recipe.cooldown;
                effiCooldown = cooldown;
                SetOutput(recipe);
                sInvenManager.progressBar.SetProgress(0);
            }
        }
        else if (selectRecipe.name == "UICancel")
        {
            ReSetUI();
        }
    }

    void ReSetUI()
    {
        recipe = new Recipe();
        recipeIndex = -1;
        cooldown = structureData.Cooldown;
        effiCooldown = cooldown;
        prodTimer = 0;
        OperateStateSet(false);
        sInvenManager.SetCooldownText(0);
        if (IsServer)
        {
            AddInvenItem();
        }
        inventory.ResetInven();
        if (isUIOpened)
        {
            sInvenManager.ClearInvenOption();
        }
    }

    public virtual void SetOutput(Recipe recipe)
    {
        // 건물 레시피 따라서 output을 바꿔줘야 하는 경우 오버라이딩
    }

    public override void GameStartRecipeSet(int recipeId)
    {
        if (recipeId != -1)
            SetRecipeClientRpc(recipeId);
    }

    [ServerRpc(RequireOwnership = false)]
    public override void ItemSyncServerRpc()
    {
        ItemListClearClientRpc();
        List<int> slotNums = new List<int>();
        List<int> itemIndexs = new List<int>();
        List<int> itemAmounts = new List<int>();
        int index = 0;

        for (int i = 0; i < inventory.space; i++)
        {
            var slot = inventory.SlotCheck(i);

            int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(slot.item);
            if (itemIndex != -1)
            {
                index++;
                slotNums.Add(i);
                itemIndexs.Add(itemIndex);
                itemAmounts.Add(slot.amount);
            }
        }
        ItemSyncClientRpc(slotNums.ToArray(), itemIndexs.ToArray(), itemAmounts.ToArray(), index);
    }

    [ClientRpc]
    protected override void ItemListClearClientRpc()
    {
        if (!IsServer)
            inventory.ResetInven();
    }


    [ClientRpc]
    protected void ItemSyncClientRpc(int[] slotNum, int[] itemIndex, int[] itemAmount, int index, ClientRpcParams rpcParams = default)
    {
        if (IsServer)
            return;

        Item[] items = new Item[index];
        for (int i = 0; i < index; i++)
        {
            Item item = GeminiNetworkManager.instance.GetItemSOFromIndex(itemIndex[i]);
            items[i] = item;
        }
        inventory.NonNetSlotsAdd(slotNum, items, itemAmount, index);
    }

    public void GameStartItemSet(InventorySaveData data)
    {
        if (inventory != null)
            inventory.LoadData(data);
    }

    public virtual float GetProgress() { return prodTimer; }
    public virtual float GetFuel() { return fuel; }
    public virtual void OpenRecipe() { }
    public virtual void GetUIFunc() { }

    public virtual void OpenUI()
    {
        isUIOpened = true;
        ClientUISetServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    protected void ClientUISetServerRpc()
    {
        ClientUISetClientRpc(fuel, prodTimer, cooldown, effiCooldown);
    }

    [ClientRpc]
    protected void ClientUISetClientRpc(int syncFuel, float syncTimer, float syncCooldown, float syncEfficiency)
    {
        if (IsServer)
            return;

        fuel = syncFuel;
        prodTimer = syncTimer;
        cooldown = syncCooldown;
        effiCooldown = syncEfficiency;
    }

    public virtual void CloseUI()
    {
        Debug.Log("CloseUI call");
        isUIOpened = false;
        GameManager.instance.CheckAndCancelFocus(this);
    }

    protected override IEnumerator SetOutObjCoroutine(GameObject obj)
    {
        checkObj = false;
        yield return new WaitForSeconds(0.1f);

        if (obj.GetComponent<WallCtrl>())
            yield break;

        if (obj.TryGetComponent(out Structure structure) && !structure.isMainSource)
        {
            if (obj.TryGetComponent(out BeltCtrl belt))
            {
                if (obj.GetComponentInParent<BeltGroupMgr>().nextObj == this.gameObject)
                {
                    StartCoroutine(SetInObjCoroutine(obj));
                    yield break;
                }
                belt.FactoryPosCheck(GetComponentInParent<Structure>());
            }
            else
            {
                outSameList.Add(obj);
                StartCoroutine(OutCheck(obj));
            }
            outObj.Add(obj);
            StartCoroutine(UnderBeltConnectCheck(obj));
        }
        else
            checkObj = true;
    }

    protected override IEnumerator OutCheck(GameObject otherObj)
    {
        yield return new WaitForSeconds(0.1f);

        if (otherObj.TryGetComponent(out Structure otherFacCtrl))
        {
            if (otherObj.GetComponent<Production>())
                yield break;

            if (otherFacCtrl.outSameList.Contains(this.gameObject) && outSameList.Contains(otherObj))
            {
                StartCoroutine(SetInObjCoroutine(otherObj));
                outObj.Remove(otherObj);
                outSameList.Remove(otherObj);
                Invoke(nameof(RemoveSameOutList), 0.1f);
            }
        }
    }

    public override void OnFactoryItem(ItemProps itemProps)
    {
        if (IsServer)
        {
            for (int i = 0; i < inventory.space; i++)
            {
                if (itemDic[recipe.items[i]] == itemProps.item)
                {
                    inventory.SlotAdd(i, itemProps.item, itemProps.amount);
                }
            }
        }

        base.OnFactoryItem(itemProps);
    }

    public override void OnFactoryItem(Item item)
    {
        if (IsServer)
        {
            for (int i = 0; i < inventory.space; i++)
            {
                if (itemDic[recipe.items[i]] == item)
                {
                    inventory.SlotAdd(i, item, 1);
                }
            }
        }
    }

    protected override void SubFromInventory()
    {
        if (IsServer)
            inventory.SlotSubServerRpc(inventory.space - 1, 1);
    }

    public virtual bool CanTakeItem(Item item)
    {
        if (recipe == null || recipe.items == null)
            return false;

        for (int i = 0; i < inventory.space - 1; i++)
        {
            var slot = inventory.SlotCheck(i);
            if (itemDic[recipe.items[i]] == item && slot.amount < 99)
                return true;
        }
        return false;
    }

    public override bool CheckOutItemNum()
    {
        var slot = inventory.SlotCheck(inventory.space - 1);
        if (slot.amount > 0)
            return true;
        else
            return false;
    }

    //public virtual (Item, int) QuickPullOut()
    //{
    //    var slot = inventory.SlotCheck(inventory.space - 1);
    //    if (slot.amount > 0)
    //        inventory.SubServerRpc(inventory.space - 1, slot.amount);
    //    return slot;
    //}

    //[ClientRpc]
    //protected override void GetItemClientRpc(int inObjIndex)
    //{
    //    itemGetDelay = true;

    //    if (inObj[inObjIndex].TryGetComponent(out BeltCtrl belt))
    //    {
    //        if (belt.itemObjList.Count > 0 && CanTakeItem(belt.itemObjList[0].item))
    //        {
    //            OnFactoryItem(belt.itemObjList[0]);
    //            belt.itemObjList[0].transform.position = this.transform.position;
    //            belt.isItemStop = false;
    //            belt.itemObjList.RemoveAt(0);
    //            if (IsServer)
    //                belt.beltGroupMgr.groupItem.RemoveAt(0);
    //            belt.ItemNumCheck();

    //            DelayGetItem();
    //            //Invoke(nameof(DelayGetItem), structureData.SendDelay);
    //        }
    //        else
    //            DelayGetItem();
    //    }
    //}

    protected override void GetItemFunc(int inObjIndex)
    {
        if (inObj[inObjIndex].TryGetComponent(out BeltCtrl belt))
        {
            if (belt.itemObjList.Count > 0 && CanTakeItem(belt.itemObjList[0].item))
            {
                OnFactoryItem(belt.itemObjList[0]);
                belt.isItemStop = false;
                belt.itemObjList.RemoveAt(0);
                if (belt.beltGroupMgr.groupItem.Count != 0)
                    belt.beltGroupMgr.groupItem.RemoveAt(0);
                belt.ItemNumCheck();
            }
            DelayGetItem();
        }
    }

    public override void AddInvenItem()
    {
        base.AddInvenItem();
        for (int i = 0; i < inventory.space; i++)
        {
            var invenItem = inventory.SlotCheck(i);

            if (invenItem.item != null && invenItem.amount > 0)
            {
                playerInven.Add(invenItem.item, invenItem.amount);
            }
        }
    }

    public void SortInven()
    {
        if (!ItemDragManager.instance.isDrag)
        {
            inventory.SortServerRpc();
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

    public bool UnloadItemCheck(Item item)
    {
        bool canUnload = false;
        for (int i = 0; i < inventory.space; i++)
        {
            var invenItem = inventory.SlotCheck(i);
            if (invenItem.item == item && invenItem.amount > 0)
            {
                canUnload = true;
                //if(IsServer)
                //    inventory.SubServerRpc(i, 1);
                break;
            }
        }
        return canUnload;
    }

    public void UnloadItem(Item item)
    {
        if (IsServer)
            inventory.Sub(item, 1);
    }

    public void LineRendererSet(Vector2 endPos)
    {
        if (endPos != Vector2.zero && lineRenderer == null)
        {
            startLine = new Vector3(transform.position.x, transform.position.y, -1);
            endLine = new Vector3(endPos.x, endPos.y, -1);

            GameObject currentLine = Instantiate(lineObj, startLine, Quaternion.identity);
            lineRenderer = currentLine.GetComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, startLine);
            lineRenderer.SetPosition(1, endLine);
        }
    }

    public virtual void DestroyLineRenderer()
    {
        if (lineRenderer != null)
        {
            Destroy(lineRenderer.gameObject);
            lineRenderer = null;
        }
    }

    public void ResetLine(Vector2 endPos)
    {
        DestroyLineRenderer();
        LineRendererSet(endPos);
    }

    public override void EfficiencyCheck()
    {
        // conn != null && conn.group != null && conn.group.efficiency > 0
        // 인 경우에만 사용할 것. 조건 검사를 여러번 반복하는걸 방지하기 위해서 여기에서는 뺌
        if (efficiency != conn.group.efficiency)
        {
            efficiency = conn.group.efficiency;
            if (efficiency != 0)
            {
                effiCooldown = cooldown / efficiency;
            }
            else
            {
                effiCooldown = cooldown;
            }

            if (isUIOpened)
            {
                sInvenManager.progressBar.SetMaxProgress(effiCooldown - ((overclockOn ? effiCooldown * overclockPer / 100 : 0) + effiCooldownUpgradeAmount));
                sInvenManager.SetCooldownText(effiCooldown - ((overclockOn ? effiCooldown * overclockPer / 100 : 0) + effiCooldownUpgradeAmount));
            }
        }
    }

    [ServerRpc]
    public void OverclockSyncServerRpc(bool isOn)
    {
        OverclockSyncClientRpc(isOn);
    }

    [ClientRpc]
    void OverclockSyncClientRpc(bool isOn)
    {
        overclockOn = isOn;
        if (isUIOpened)
        {
            sInvenManager.SetCooldownText(effiCooldown - ((overclockOn ? effiCooldown * overclockPer / 100 : 0) + effiCooldownUpgradeAmount));
        }
    }

    protected override void ItemDrop()
    {
        if (inventory == null)
        {
            return;
        }

        for (int i = 0; i < inventory.space; i++)
        {
            var invenItem = inventory.SlotCheck(i);

            if (invenItem.item != null && invenItem.amount > 0)
            {
                ItemToItemProps(invenItem.item, invenItem.amount);
            }
        }
    }

    public override StructureSaveData SaveData()
    {
        StructureSaveData data = base.SaveData();

        if(TryGetComponent(out Inventory inventory))
        {
            data.inven = inventory.SaveData();
        }

        data.recipeId = recipeIndex;

        return data;
    }
}
