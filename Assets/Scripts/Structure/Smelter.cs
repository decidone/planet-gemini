using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
public class Smelter : Production
{
    protected override void Update()
    {
        base.Update();
        if (!isPreBuilding)
        {
            if (recipe.name != null)
            {
                if (conn != null && conn.group != null && conn.group.efficiency > 0)
                {
                    EfficiencyCheck();

                    if (slot.Item2 >= recipe.amounts[0] && slot1.Item2 >= recipe.amounts[1]
                    && (slot2.Item2 + recipe.amounts[recipe.amounts.Count - 1]) <= maxAmount)
                    {
                        //output = itemDic[recipe.items[recipe.items.Count - 1]];

                        if (slot2.Item1 == output || slot2.Item1 == null)
                        {
                            OperateStateSet(true);
                            prodTimer += Time.deltaTime;
                            if (prodTimer > effiCooldown - ((overclockOn ? effiCooldown * overclockPer / 100 : 0) + effiCooldownUpgradeAmount))
                            {
                                if (IsServer)
                                {
                                    Overall.instance.OverallConsumption(slot.Item1, recipe.amounts[0]);
                                    Overall.instance.OverallConsumption(slot1.Item1, recipe.amounts[1]);

                                    inventory.SlotSubServerRpc(0, recipe.amounts[0]);
                                    inventory.SlotSubServerRpc(1, recipe.amounts[1]);
                                    inventory.SlotAdd(2, output, recipe.amounts[recipe.amounts.Count - 1]);

                                    Overall.instance.OverallProd(output, recipe.amounts[recipe.amounts.Count - 1]);
                                }

                                soundManager.PlaySFX(gameObject, "structureSFX", "Structure");
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
            }

            if (IsServer && slot2.Item2 > 0 && outObj.Count > 0 && !itemSetDelay && checkObj)
            {
                int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(output);
                SendItem(itemIndex);
                //SendItem(output);
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

    public override void OpenUI()
    {
        base.OpenUI();
        sInvenManager.SetInven(inventory, ui);
        sInvenManager.SetProd(this);
        sInvenManager.progressBar.SetMaxProgress(effiCooldown - ((overclockOn ? effiCooldown * overclockPer / 100 : 0) + effiCooldownUpgradeAmount));
        sInvenManager.SetCooldownText(effiCooldown - ((overclockOn ? effiCooldown * overclockPer / 100 : 0) + effiCooldownUpgradeAmount));
        //sInvenManager.progressBar.SetMaxProgress(cooldown);

        rManager.recipeBtn.gameObject.SetActive(true);
        rManager.recipeBtn.onClick.RemoveAllListeners();
        rManager.recipeBtn.onClick.AddListener(OpenRecipe);

        sInvenManager.InvenInit();
        if (recipe.name != null)
            SetRecipe(recipe, recipeIndex);
    }

    public override void CloseUI()
    {
        base.CloseUI();
        sInvenManager.ReleaseInven();

        rManager.recipeBtn.onClick.RemoveAllListeners();
        rManager.recipeBtn.gameObject.SetActive(false);
    }

    public override void OpenRecipe()
    {
        rManager.OpenUI();
        rManager.SetRecipeUI("Smelter", this);
    }

    public override void SetRecipe(Recipe _recipe, int index)
    {
        base.SetRecipe(_recipe, index);
        sInvenManager.slots[0].SetInputItem(itemDic[recipe.items[0]]);
        sInvenManager.slots[0].SetNeedAmount(recipe.amounts[0]);
        sInvenManager.slots[1].SetInputItem(itemDic[recipe.items[1]]);
        sInvenManager.slots[1].SetNeedAmount(recipe.amounts[1]);
        sInvenManager.slots[2].SetInputItem(itemDic[recipe.items[2]]);
        sInvenManager.slots[2].SetNeedAmount(recipe.amounts[2]);
        sInvenManager.slots[2].outputSlot = true;
    }

    public override void SetOutput(Recipe recipe)
    {
        output = itemDic[recipe.items[recipe.items.Count - 1]];
    }

    public override void GetUIFunc()
    {
        InventoryList inventoryList = canvas.GetComponent<InventoryList>();

        foreach (GameObject list in inventoryList.StructureStorageArr)
        {
            if (list.name == "Smelter")
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
