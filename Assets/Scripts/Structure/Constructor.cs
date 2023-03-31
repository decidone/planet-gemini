using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constructor : Production
{
    [SerializeField]
    int maxAmount;
    [SerializeField]
    float cooldown;
    [SerializeField]
    StructureInvenManager sInvenManager;
    [SerializeField]
    GameObject constructor;

    string recipeUI;
    Inventory inventory;
    float prodTimer;
    Dictionary<string, Item> itemDic;
    bool activeUI;

    void Start()
    {
        inventory = this.GetComponent<Inventory>();
        itemDic = ItemList.instance.itemDic;
        // 레시피 설정하는 부분 임시 설정.
        SetRecipe();
    }

    void Update()
    {
        var slot = inventory.SlotCheck(0);
        var slot1 = inventory.SlotCheck(1);

        if (slot.amount > 0 && slot1.amount < maxAmount)
        {
            Item output = null;
            switch (slot.item.name)
            {
                case "GoldBar":
                    output = itemDic["Gold"];
                    break;
                case "SilverBar":
                    output = itemDic["Silver"];
                    break;
            }

            if (slot1.item == output || slot1.item == null)
            {
                prodTimer += Time.deltaTime;
                if (prodTimer > cooldown)
                {
                    inventory.Sub(0, 1);
                    inventory.SlotAdd(1, output, 1);
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

    public void OpenUI()
    {
        if (recipeUI == "Constructor")
        {
            sInvenManager.SetInven(inventory, constructor);
            sInvenManager.slots[0].SetInputItem(ItemList.instance.itemDic["GoldBar"]);
            sInvenManager.slots[0].SetInputItem(ItemList.instance.itemDic["SilverBar"]);
            sInvenManager.slots[1].outputSlot = true;
            sInvenManager.progressBar.SetMaxProgress(cooldown);
            activeUI = true;
        }
    }

    public void CloseUI()
    {
        if (recipeUI == "Constructor")
        {
            sInvenManager.ReleaseInven();
            activeUI = false;
        }
    }

    void SetRecipe()
    {
        recipeUI = "Constructor";
    }
}
