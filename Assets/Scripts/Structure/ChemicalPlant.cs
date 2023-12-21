using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
public class ChemicalPlant : Production
{
    protected override void Update()
    {
        base.Update();
        if (!isPreBuilding)
        {
            var slot = inventory.SlotCheck(0);
            var slot1 = inventory.SlotCheck(1);

            if (recipe.name != null)
            {
                if (slot.amount >= recipe.amounts[0] && (slot1.amount + recipe.amounts[recipe.amounts.Count - 1]) <= maxAmount)
                {
                    output = itemDic[recipe.items[recipe.items.Count - 1]];

                    if (slot1.item == output || slot1.item == null)
                    {
                        prodTimer += Time.deltaTime;
                        if (prodTimer > cooldown)
                        {
                            inventory.Sub(0, recipe.amounts[0]);
                            inventory.SlotAdd(1, output, recipe.amounts[recipe.amounts.Count - 1]);
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

            if (slot1.amount > 0 && outObj.Count > 0 && !itemSetDelay && checkObj)
            {
                SendItem(output);
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
        rManager.SetRecipeUI("ChemicalPlant", this);
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
        sInvenManager.slots[1].outputSlot = true;
        sInvenManager.progressBar.SetMaxProgress(recipe.cooldown);
    }

    public override void GetUIFunc()
    {
        InventoryList inventoryList = canvas.GetComponent<InventoryList>();

        foreach (GameObject list in inventoryList.StructureStorageArr)
        {
            if (list.name == "ChemicalPlant")
            {
                ui = list;
            }
        }
    }
}