using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FurnaceUpgrade : Production
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

            if (slot.item != null && conn != null && conn.group != null && conn.group.efficiency > 0)
            {
                foreach (Recipe _recipe in recipes)
                {
                    if (slot.item == itemDic[_recipe.items[0]])
                    {
                        recipe = _recipe;
                        output = itemDic[recipe.items[recipe.items.Count - 1]];
                    }
                }

                if (slot.amount >= recipe.amounts[0] && (slot1.amount + recipe.amounts[recipe.amounts.Count - 1]) <= maxAmount)
                {
                    if (slot1.item == output || slot1.item == null)
                    {
                        prodTimer += Time.deltaTime;
                        if (prodTimer > cooldown)
                        {
                            if (IsServer)
                            {
                                inventory.SubServerRpc(0, recipe.amounts[0]);
                                inventory.SlotAdd(1, output, recipe.amounts[recipe.amounts.Count - 1]);

                                Overall.instance.OverallConsumption(slot.item, recipe.amounts[0]);
                                Overall.instance.OverallProd(output, recipe.amounts[recipe.amounts.Count - 1]);
                            }
                            soundManager.PlaySFX(gameObject, "structureSFX", "Flames");
                            prodTimer = 0;
                        }
                    }
                    else
                    {
                        prodTimer = 0;
                    }
                }
                else
                {
                    prodTimer = 0;
                }
            }
            else
            {
                prodTimer = 0;
            }

            if (IsServer && slot1.amount > 0 && outObj.Count > 0 && !itemSetDelay && checkObj)
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
        sInvenManager.slots[1].outputSlot = true;
    }

    public override void CloseUI()
    {
        base.CloseUI();
        sInvenManager.ReleaseInven();
    }

    public override bool CanTakeItem(Item item)
    {
        var slot = inventory.SlotCheck(0);

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
        if (IsServer)
        {
            foreach (Recipe _recipe in recipes)
            {
                if (itemProps.item == itemDic[_recipe.items[0]])
                    inventory.SlotAdd(0, itemProps.item, itemProps.amount);
            }
        }

        itemProps.itemPool.Release(itemProps.gameObject);
    }

    public override void OnFactoryItem(Item item)
    {
        if (IsServer)
        {
            foreach (Recipe _recipe in recipes)
            {
                if (item == itemDic[_recipe.items[0]])
                    inventory.SlotAdd(0, item, 1);
            }
        }
    }

    public override void GetUIFunc()
    {
        InventoryList inventoryList = canvas.GetComponent<InventoryList>();
        foreach (GameObject list in inventoryList.StructureStorageArr)
        {
            if (list.name == "FurnaceLv2")
            {
                ui = list;
            }
        }
    }
}