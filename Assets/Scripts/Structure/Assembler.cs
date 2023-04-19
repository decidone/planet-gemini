using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Assembler : Production
{
    void Update()
    {
        var slot = inventory.SlotCheck(0);
        var slot1 = inventory.SlotCheck(1);
        var slot2 = inventory.SlotCheck(2);

        if (recipe.name != null)
        {
            if (slot.amount >= recipe.amounts[0] && slot1.amount >= recipe.amounts[1]
                && (slot2.amount + recipe.amounts[recipe.amounts.Count - 1]) <= maxAmount)
            {
                output = itemDic[recipe.items[recipe.items.Count - 1]];

                if (slot2.item == output || slot2.item == null)
                {
                    prodTimer += Time.deltaTime;
                    if (prodTimer > cooldown)
                    {
                        inventory.Sub(0, recipe.amounts[0]);
                        inventory.Sub(1, recipe.amounts[1]);
                        inventory.SlotAdd(2, output, recipe.amounts[recipe.amounts.Count - 1]);
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
    }

    public override void OpenUI()
    {
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
        sInvenManager.ReleaseInven();

        rManager.recipeBtn.onClick.RemoveAllListeners();
        rManager.recipeBtn.gameObject.SetActive(false);
    }

    public override void OpenRecipe()
    {
        rManager.OpenUI();
        rManager.SetRecipeUI("Assembler", this);
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
        sInvenManager.slots[2].outputSlot = true;
        sInvenManager.progressBar.SetMaxProgress(recipe.cooldown);
    }
}
