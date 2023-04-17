using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Furnace : Production
{
    protected override void Start()
    {
        base.Start();
        maxFuel = 100;
    }

    void Update()
    {
        var slot = inventory.SlotCheck(0);
        var slot1 = inventory.SlotCheck(1);
        var slot2 = inventory.SlotCheck(2);

        if (fuel == 0 && slot1.item == itemDic["Coal"] && slot1.amount > 0)
        {
            inventory.Sub(1, 1);
            fuel = maxFuel;
        }

        if (slot.item != null)
        {
            foreach (Recipe _recipe in recipes)
            {
                if (slot.item == _recipe.items[0])
                {
                    recipe = _recipe;
                    output = recipe.items[recipe.items.Count - 1];
                }
            }

            if (fuel > 0 && slot.amount >= recipe.amounts[0] && (slot2.amount + recipe.amounts[recipe.amounts.Count - 1]) <= maxAmount)
            {
                if (slot2.item == output || slot2.item == null)
                {
                    prodTimer += Time.deltaTime;
                    if (prodTimer > cooldown)
                    {
                        fuel -= 25;
                        inventory.Sub(0, recipe.amounts[0]);
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
        else
        {
            prodTimer = 0;
        }
    }

    public override void OpenUI()
    {
        sInvenManager.SetInven(inventory, ui);
        sInvenManager.SetProd(this);
        sInvenManager.progressBar.SetMaxProgress(cooldown);

        sInvenManager.energyBar.SetMaxProgress(maxFuel);
        recipes = rManager.GetRecipeList("Furnace", this);
        List<Item> items = new List<Item>();
        foreach (Recipe recipe in recipes)
        {
            items.Add(recipe.items[0]);
        }
        sInvenManager.slots[0].SetInputItem(items);
        sInvenManager.slots[1].SetInputItem(ItemList.instance.itemDic["Coal"]);
        sInvenManager.slots[2].outputSlot = true;
    }

    public override void CloseUI()
    {
        sInvenManager.ReleaseInven();
    }
}
