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
        inventory.onItemChangedCallback += SetFurnaceRecipe;
        SetFurnaceRecipe(0);
    }

    protected override void Update()
    {
        base.Update();
        if (!isPreBuilding)
        {
            if (slot.Item1 != null && conn != null && conn.group != null && conn.group.efficiency > 0)
            {
                if (slot.Item2 >= recipe.amounts[0] && (slot1.Item2 + recipe.amounts[recipe.amounts.Count - 1]) <= maxAmount)
                {
                    if (slot1.Item1 == output || slot1.Item1 == null)
                    {
                        OperateStateSet(true);
                        prodTimer += Time.deltaTime;
                        if (prodTimer > cooldown)
                        {
                            if (IsServer)
                            {
                                Overall.instance.OverallConsumption(slot.Item1, recipe.amounts[0]);

                                inventory.SlotSubServerRpc(0, recipe.amounts[0]);
                                inventory.SlotAdd(1, output, recipe.amounts[recipe.amounts.Count - 1]);

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

            if (IsServer && slot1.Item2 > 0 && outObj.Count > 0 && !itemSetDelay)
            {
                int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(output);
                SendItem(itemIndex);
            }
        }
    }

    public override void CheckSlotState(int slotindex)
    {
        // update에서 검사해야 하는 특정 슬롯들 상태를 인벤토리 콜백이 있을 때 미리 저장
        slot = inventory.SlotCheck(0);
        slot1 = inventory.SlotCheck(1);
    }

    public override void CheckInvenIsFull(int slotIndex)
    {
        // output slot을 제외하고 나머지 슬롯이 가득 차 있는지 체크
        if (inventory.SlotAmountCheck(0) < inventory.maxAmount)
        {
            isInvenFull = false;
            return;
        }

        isInvenFull = true;
    }

    public void SetFurnaceRecipe(int slotindex)
    {
        foreach (Recipe _recipe in recipes)
        {
            if (slot.Item1 == itemDic[_recipe.items[0]])
            {
                recipe = _recipe;
                output = itemDic[recipe.items[recipe.items.Count - 1]];
                FactoryOverlay();
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
        if (isInvenFull) return false;

        if (slot.Item1 == null)
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
        else if (slot.Item1 == item && slot.Item2 < 99)
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
                {
                    inventory.SlotAdd(0, itemProps.item, itemProps.amount);
                    break;
                }
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
                {
                    inventory.SlotAdd(0, item, 1);
                    break;
                }
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
    protected override void NonOperateStateSet(bool isOn)
    {
        setModel.sprite = strImg[isOn ? 1 : 0];
        smokeCtrl.SetSmokeActive(isOn);
    }
}
