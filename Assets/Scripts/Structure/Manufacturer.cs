using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
public class Manufacturer : Production
{
    protected override void Update()
    {
        base.Update();
        if (!isPreBuilding)
        {
            var slot = inventory.SlotCheck(0);
            var slot1 = inventory.SlotCheck(1);
            var slot2 = inventory.SlotCheck(2);
            var slot3 = inventory.SlotCheck(3);

            if (recipe.name != null)
            {
                if (conn != null && conn.group != null && conn.group.efficiency > 0)
                {
                    EfficiencyCheck();

                    if (slot.amount >= recipe.amounts[0] && slot1.amount >= recipe.amounts[1]
                    && slot2.amount >= recipe.amounts[2]
                    && (slot3.amount + recipe.amounts[recipe.amounts.Count - 1]) <= maxAmount)
                    {
                        output = itemDic[recipe.items[recipe.items.Count - 1]];

                        if (slot3.item == output || slot3.item == null)
                        {
                            isOperate = true;
                            prodTimer += Time.deltaTime;
                            if (prodTimer > effiCooldown)
                            {
                                inventory.SubServerRpc(0, recipe.amounts[0]);
                                inventory.SubServerRpc(1, recipe.amounts[1]);
                                inventory.SubServerRpc(2, recipe.amounts[2]);
                                inventory.SlotAdd(3, output, recipe.amounts[recipe.amounts.Count - 1]);
                                soundManager.PlaySFX(gameObject, "structureSFX", "Machine");
                                prodTimer = 0;
                            }
                        }
                        else
                        {
                            isOperate = false;
                            prodTimer = 0;
                        }
                    }
                    else
                    {
                        isOperate = false;
                        prodTimer = 0;
                    }
                }
                else
                {
                    isOperate = false;
                    prodTimer = 0;
                }
            }

            if (IsServer && slot3.amount > 0 && outObj.Count > 0 && !itemSetDelay && checkObj)
            {
                int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(output);
                SendItemClientRpc(itemIndex);
                //SendItem(output);
            }
        }
    }

    public override void OpenUI()
    {
        base.OpenUI();
        sInvenManager.SetInven(inventory, ui);
        sInvenManager.SetProd(this);
        sInvenManager.progressBar.SetMaxProgress(cooldown);

        rManager.recipeBtn.gameObject.SetActive(true);
        rManager.recipeBtn.onClick.RemoveAllListeners();
        rManager.recipeBtn.onClick.AddListener(OpenRecipe);

        sInvenManager.InvenInit();
        if (recipe.name != null)
            SetRecipe(recipe);
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
        rManager.SetRecipeUI("Manufacturer", this);
    }

    public override void SetRecipe(Recipe _recipe)
    {
        if (recipe.name != null && recipe != _recipe)
        {
            sInvenManager.EmptySlot();
        }
        recipe = _recipe;
        sInvenManager.ResetInvenOption();
        sInvenManager.slots[0].SetInputItem(itemDic[recipe.items[0]]);
        sInvenManager.slots[1].SetInputItem(itemDic[recipe.items[1]]);
        sInvenManager.slots[2].SetInputItem(itemDic[recipe.items[2]]);
        sInvenManager.slots[3].outputSlot = true;
        cooldown = recipe.cooldown;
        sInvenManager.progressBar.SetMaxProgress(cooldown);
    }

    public override void GetUIFunc()
    {
        InventoryList inventoryList = canvas.GetComponent<InventoryList>();

        foreach (GameObject list in inventoryList.StructureStorageArr)
        {
            if (list.name == "Manufacturer")
            {
                ui = list;
            }
        }
    }
}
