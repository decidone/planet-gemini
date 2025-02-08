using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
public class Furnace : Production
{ 
    protected override void Start()
    {
        base.Start();
        maxFuel = 100;
        recipes = rManager.GetRecipeList("Furnace", this);
    }

    protected override void Update()
    {
        base.Update();
        if (!isPreBuilding)
        {
            var slot = inventory.SlotCheck(0);
            var slot1 = inventory.SlotCheck(1);
            var slot2 = inventory.SlotCheck(2);

            if (fuel == 0 && slot1.item == itemDic["Coal"] && slot1.amount > 0)
            {
                if(IsServer)
                    inventory.SubServerRpc(1, 1);
                fuel = maxFuel;
            }

            if (slot.item != null)
            {
                foreach (Recipe _recipe in recipes)
                {
                    if (slot.item == itemDic[_recipe.items[0]])
                    {
                        recipe = _recipe;
                        output = itemDic[recipe.items[recipe.items.Count - 1]];
                    }
                }
     
                if (fuel > 0 && slot.amount >= recipe.amounts[0] && (slot2.amount + recipe.amounts[recipe.amounts.Count - 1]) <= maxAmount)
                {
                    if (slot2.item == output || slot2.item == null)
                    {
                        OperateStateSet(true);
                        prodTimer += Time.deltaTime;
                        if (prodTimer > cooldown)
                        {
                            fuel -= 25;
                            if (IsServer)
                            {
                                inventory.SubServerRpc(0, recipe.amounts[0]);
                                inventory.SlotAdd(2, output, recipe.amounts[recipe.amounts.Count - 1]);

                                Overall.instance.OverallConsumption(slot.item, recipe.amounts[0]);
                                Overall.instance.OverallProd(output, recipe.amounts[recipe.amounts.Count - 1]);
                            }
                            soundManager.PlaySFX(gameObject, "structureSFX", "Flames");
                            prodTimer = 0;
                        }
                    }
                    else
                    {
                        OperateStateSet(false);
                        prodTimer = 0;
                    }
                }
                else
                {
                    OperateStateSet(false);
                    prodTimer = 0;
                }
            }
            else
            {
                OperateStateSet(false);
                prodTimer = 0;
            }

            if (IsServer && slot2.amount > 0 && outObj.Count > 0 && !itemSetDelay && checkObj)
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

    public override void WarningStateCheck()
    {
        if (!isPreBuilding && warningIcon != null)
        {
            if (fuel > 0)
            {
                if (warningIconCheck)
                {
                    if (warning != null)
                        StopCoroutine(warning);
                    warningIconCheck = false;
                    warningIcon.enabled = false;
                }
            }
            else
            {
                if (!warningIconCheck)
                {
                    if (warning != null)
                        StopCoroutine(warning);
                    warning = FlickeringIcon();
                    StartCoroutine(warning);
                    warningIconCheck = true;
                }
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

        sInvenManager.energyBar.SetMaxProgress(maxFuel);
        List<Item> items = new List<Item>();
        foreach (Recipe recipe in recipes)
        {
            items.Add(itemDic[recipe.items[0]]);
        }
        sInvenManager.slots[0].SetInputItem(items);
        sInvenManager.slots[1].SetInputItem(ItemList.instance.itemDic["Coal"]);
        sInvenManager.slots[2].outputSlot = true;
    }

    public override void CloseUI()
    {
        base.CloseUI();
        sInvenManager.ReleaseInven();
    }

    public override bool CanTakeItem(Item item)
    {
        var slot = inventory.SlotCheck(0);
        var slot1 = inventory.SlotCheck(1);

        if (itemDic["Coal"] == item && slot1.amount < 99)
            return true;

        if (slot.item == null)
        {
            if (recipes != null)
            {
                foreach (Recipe _recipe in recipes)
                {
                    if (item == itemDic[_recipe.items[0]])
                        return true;
                }
            }
        }
        else if (slot.item == item && slot.amount < 99)
            return true;

        return false;
    }

    public override void OnFactoryItem(ItemProps itemProps)
    {
        if(IsServer)
        {
            if (itemDic["Coal"] == itemProps.item)
                inventory.SlotAdd(1, itemProps.item, itemProps.amount);
            else
            {
                foreach (Recipe _recipe in recipes)
                {
                    if (itemProps.item == itemDic[_recipe.items[0]])
                        inventory.SlotAdd(0, itemProps.item, itemProps.amount);
                }
            }
        }

        itemProps.itemPool.Release(itemProps.gameObject);
    }

    public override void OnFactoryItem(Item item)
    {
        if (IsServer)
        {
            if (itemDic["Coal"] == item)
                inventory.SlotAdd(1, item, 1);
            else
            {
                foreach (Recipe _recipe in recipes)
                {
                    if (item == itemDic[_recipe.items[0]])
                        inventory.SlotAdd(0, item, 1);
                }
            }
        }
    }

    public override void GetUIFunc()
    {
        InventoryList inventoryList = canvas.GetComponent<InventoryList>();
        foreach (GameObject list in inventoryList.StructureStorageArr)
        {
            if (list.name == "FurnaceLv1")
            {
                ui = list;
            }
        }
    }

    protected override void NonOperateStateSet(bool isOn)
    {
        setModel.sprite = strImg[isOn ? 1 : 0];
    }
}
