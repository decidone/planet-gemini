using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constructor : Production
{
    void Update()
    {
        var slot = inventory.SlotCheck(0);
        var slot1 = inventory.SlotCheck(1);

        if (recipe.name != null)
        {
            if (slot.amount >= recipe.amounts[0] && (slot1.amount + recipe.amounts[recipe.amounts.Count - 1]) <= maxAmount)
            {
                output = recipe.items[recipe.items.Count - 1];

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
        rManager.SetRecipeUI("Constructor", this);
    }

    public override void SetRecipe(Recipe _recipe)
    {
        recipe = _recipe;
        Debug.Log("recipe : " + recipe.name);
        sInvenManager.ResetInvenOption();
        sInvenManager.slots[0].SetInputItem(recipe.items[0]);
        sInvenManager.slots[1].outputSlot = true;
        sInvenManager.progressBar.SetMaxProgress(recipe.cooldown);
    }
}
