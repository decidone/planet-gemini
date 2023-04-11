using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Assembler : Production
{
    [SerializeField]
    int maxAmount;
    [SerializeField]
    float cooldown;
    [SerializeField]
    StructureInvenManager sInvenManager;

    Inventory inventory;
    float prodTimer;
    Dictionary<string, Item> itemDic;
    bool activeUI;

    void Start()
    {
        inventory = this.GetComponent<Inventory>();
        itemDic = ItemList.instance.itemDic;
    }

    void Update()
    {
        var slot = inventory.SlotCheck(0);
        var slot1 = inventory.SlotCheck(1);
        var slot2 = inventory.SlotCheck(2);

        if (slot.amount > 0 && slot1.amount > 0 && slot2.amount < maxAmount && slot.item != slot1.item)
        {
            Item output = null;
            switch (slot.item.name)
            {
                case "GoldBar":
                    if (slot1.item.name == "SilverBar")
                        output = itemDic["Coal"];
                    break;
                case "SilverBar":
                    if (slot1.item.name == "GoldBar")
                        output = itemDic["Coal"];
                    break;
            }

            if (slot2.item == output || slot2.item == null)
            {
                prodTimer += Time.deltaTime;
                if (prodTimer > cooldown)
                {
                    inventory.Sub(0, 1);
                    inventory.Sub(1, 1);
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
        }
    }

    public override void OpenUI()
    {
        sInvenManager.SetInven(inventory, ui);
        sInvenManager.slots[0].SetInputItem(ItemList.instance.itemDic["GoldBar"]);
        sInvenManager.slots[0].SetInputItem(ItemList.instance.itemDic["SilverBar"]);
        sInvenManager.slots[1].SetInputItem(ItemList.instance.itemDic["GoldBar"]);
        sInvenManager.slots[1].SetInputItem(ItemList.instance.itemDic["SilverBar"]);
        sInvenManager.slots[2].outputSlot = true;
        sInvenManager.progressBar.SetMaxProgress(cooldown);
        activeUI = true;
    }

    public override void CloseUI()
    {
        sInvenManager.ReleaseInven();
        activeUI = false;
    }
}
