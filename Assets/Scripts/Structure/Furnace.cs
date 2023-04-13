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
        else if (fuel > 0 && slot.amount > 0 && slot2.amount < maxAmount)
        {
            switch (slot.item.name)
            {
                case "Gold":
                    output = itemDic["GoldBar"];
                    break;
                case "Silver":
                    output = itemDic["SilverBar"];
                    break;
            }

            if (slot2.item == output || slot2.item == null)
            {
                prodTimer += Time.deltaTime;
                if (prodTimer > cooldown)
                {
                    fuel -= 25;
                    inventory.Sub(0, 1);
                    inventory.SlotAdd(2, output, 1);
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
        sInvenManager.slots[0].SetInputItem(ItemList.instance.itemDic["Gold"]);
        sInvenManager.slots[0].SetInputItem(ItemList.instance.itemDic["Silver"]);
        sInvenManager.slots[1].SetInputItem(ItemList.instance.itemDic["Coal"]);
        sInvenManager.slots[2].outputSlot = true;
    }

    public override void CloseUI()
    {
        sInvenManager.ReleaseInven();
    }
}
