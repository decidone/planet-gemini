using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Assembler : Production
{
    protected override void Update()
    {
        base.Update();
        if (!isPreBuilding)
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

            if (slot2.amount > 0 && outObj.Count > 0 && !itemSetDelay && checkObj)
            {
                SetItem();
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

    public override bool CanTakeItem(Item item)
    {
        if (recipe.name == null)
            return false;

        var slot = inventory.SlotCheck(0);
        var slot1 = inventory.SlotCheck(1);

        if (itemDic[recipe.items[0]] == item && slot.amount < 99)
            return true;

        if (itemDic[recipe.items[1]] == item && slot1.amount < 99)
            return true;

        return false;
    }

    public override void OnFactoryItem(ItemProps itemProps)
    {
        if(itemDic[recipe.items[0]] == itemProps.item)
        {
            inventory.SlotAdd(0, itemProps.item, itemProps.amount);
        }
        else if(itemDic[recipe.items[1]] == itemProps.item)
        {
            inventory.SlotAdd(1, itemProps.item, itemProps.amount);
        }

        OnDestroyItem(itemProps);
    }

    public override void OnFactoryItem(Item item)
    {
        if (itemDic[recipe.items[0]] == item)
        {
            inventory.SlotAdd(0, item, 1);
        }
        else if (itemDic[recipe.items[1]] == item)
        {
            inventory.SlotAdd(1, item, 1);
        }
    }

    protected override void SubFromInventory()
    {
        inventory.Sub(2, 1);
    }

    public override bool CheckOutItemNum()
    {
        var slot2 = inventory.SlotCheck(2);
        if (slot2.amount > 0)
            return true;
        else
            return false;
    }

    public override void ItemNumCheck()
    {
        var slot2 = inventory.SlotCheck(2);

        if (slot2.amount < maxAmount)
            isFull = false;
        else
            isFull = true;
    }

    public override (Item, int) QuickPullOut()
    {
        var slot2 = inventory.SlotCheck(2);
        if (slot2.amount > 0)
            inventory.Sub(2, slot2.amount);
        return slot2;
    }

    public override void GetUIFunc() 
    {
        InventoryList inventoryList = canvas.GetComponent<InventoryList>();

        foreach (GameObject list in inventoryList.StructureStorageArr)
        {
            if (list.name == "Assembler")
            {
                ui = list;
            }
        }
    }

    protected override void AddInvenItem()
    {
        var slot = inventory.SlotCheck(0);
        var slot1 = inventory.SlotCheck(1);
        var slot2 = inventory.SlotCheck(2);

        if(slot.item != null)
        {
            playerInven.Add(slot.item, slot.amount);
        }
        if (slot1.item != null)
        {
            playerInven.Add(slot1.item, slot1.amount);
        }
        if (slot2.item != null)
        {
            playerInven.Add(slot2.item, slot2.amount);
        }
    }
}
