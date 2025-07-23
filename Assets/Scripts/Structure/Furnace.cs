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
        inventory.onItemChangedCallback += SetFurnaceRecipe;
        SetFurnaceRecipe(0);
    }

    protected override void Update()
    {
        base.Update();
        if (!isPreBuilding)
        {
            if (fuel == 0 && slot1.Item1 == itemDic["Coal"] && slot1.Item2 > 0)
            {
                if(IsServer)
                    inventory.SlotSubServerRpc(1, 1);
                fuel = maxFuel;
            }

            if (slot.Item1 != null)
            {
                if (fuel > 0 && slot.Item2 >= recipe.amounts[0] && (slot2.Item2 + recipe.amounts[recipe.amounts.Count - 1]) <= maxAmount)
                {
                    if (slot2.Item1 == output || slot2.Item1 == null)
                    {
                        OperateStateSet(true);
                        prodTimer += Time.deltaTime;
                        if (prodTimer > cooldown)
                        {
                            fuel -= 25;
                            if (IsServer)
                            {
                                Overall.instance.OverallConsumption(slot.Item1, recipe.amounts[0]);

                                inventory.SlotSubServerRpc(0, recipe.amounts[0]);
                                inventory.SlotAdd(2, output, recipe.amounts[recipe.amounts.Count - 1]);

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

            if (IsServer && slot2.Item2 > 0 && outObj.Count > 0 && !itemSetDelay)
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
        slot1 = inventory.SlotCheck(1);
        slot2 = inventory.SlotCheck(2);
    }

    public override void CheckInvenIsFull(int slotIndex)
    {
        // output slot을 제외하고 나머지 슬롯이 가득 차 있는지 체크
        for (int i = 0; i < 2; i++)
        {
            if (inventory.SlotAmountCheck(i) < inventory.maxAmount)
            {
                isInvenFull = false;
                return;
            }
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
            }
        }
    }

    protected override IEnumerator CheckWarning()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(1f);

            if (!isPreBuilding && !removeState)
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
        if (isInvenFull) return false;

        if (itemDic["Coal"] == item && slot1.Item2 < 99)
            return true;

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
        if(IsServer)
        {
            if (itemDic["Coal"] == itemProps.item)
                inventory.SlotAdd(1, itemProps.item, itemProps.amount);
            else
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
                    {
                        inventory.SlotAdd(0, item, 1);
                        break;
                    }
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
        smokeCtrl.SetSmokeActive(isOn);
    }
}
