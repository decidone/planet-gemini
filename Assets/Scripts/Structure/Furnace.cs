using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Furnace : Production
{
    [SerializeField]
    int maxAmount;
    [SerializeField]
    float cooldown;
    [SerializeField]
    StructureInvenManager sInvenManager;

    int fuel;
    int maxFuel;
    Inventory inventory;
    float prodTimer;
    Dictionary<string, Item> itemDic;
    bool activeUI;

    void Start()
    {
        inventory = this.GetComponent<Inventory>();
        itemDic = ItemList.instance.itemDic;
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
            Item output = null;
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

        if (activeUI)
        {
            sInvenManager.progressBar.SetProgress(prodTimer);
            sInvenManager.energyBar.SetProgress(fuel);
        }
    }

    public override void OpenUI()
    {
        sInvenManager.SetInven(inventory, ui);
        sInvenManager.slots[0].SetInputItem(ItemList.instance.itemDic["Gold"]);
        sInvenManager.slots[0].SetInputItem(ItemList.instance.itemDic["Silver"]);
        sInvenManager.slots[1].SetInputItem(ItemList.instance.itemDic["Coal"]);
        sInvenManager.slots[2].outputSlot = true;
        sInvenManager.progressBar.SetMaxProgress(cooldown);
        sInvenManager.energyBar.SetMaxProgress(maxFuel);
        activeUI = true;
    }

    public override void CloseUI()
    {
        sInvenManager.ReleaseInven();
        activeUI = false;
    }
}
