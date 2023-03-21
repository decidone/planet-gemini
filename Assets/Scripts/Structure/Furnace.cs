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
    [SerializeField]
    GameObject furnace;
    string recipeUI;
    public int fuel;
    Inventory inventory;
    float prodTimer;
    Dictionary<string, Item> itemDic;

    void Start()
    {
        inventory = this.GetComponent<Inventory>();
        itemDic = ItemList.instance.itemDic;
        // 레시피 설정하는 부분 임시 설정.
        // 나중에 플레이어가 레시피 설정하는 기능이 생기면 해당 메서드는 제거
        SetRecipe();
    }

    void Update()
    {
        if (fuel == 0)
        {
            var slot = inventory.SlotCheck(0);
            if (slot.item == itemDic["Coal"] && slot.amount > 0)
            {
                inventory.Sub(0, 1);
                fuel = 100;
            }
        }
        else
        {
            var slot1 = inventory.SlotCheck(1);
            var slot2 = inventory.SlotCheck(2);

            if (slot1.amount > 0 && slot2.amount < maxAmount)
            {
                prodTimer += Time.deltaTime;
                if (prodTimer > cooldown)
                {
                    if (slot1.item == itemDic["Gold"])
                    {
                        if (slot2.item == itemDic["GoldBar"] || slot2.item == null)
                        {
                            fuel -= 25;
                            inventory.Sub(1, 1);
                            inventory.SlotAdd(2, itemDic["GoldBar"], 1);
                            prodTimer = 0;
                        }
                    }
                    else if (slot1.item == itemDic["Silver"])
                    {
                        if (slot2.item == itemDic["SilverBar"] || slot2.item == null)
                        {
                            fuel -= 25;
                            inventory.Sub(1, 1);
                            inventory.SlotAdd(2, itemDic["SilverBar"], 1);
                            prodTimer = 0;
                        }
                    }
                }
            }
        }
    }

    public void OpenUI()
    {
        if (recipeUI == "Furnace")
        {
            furnace.SetActive(true);
            sInvenManager.SetInven(inventory, furnace);
            sInvenManager.slots[0].SetInputItem(ItemList.instance.itemDic["Coal"]);
            sInvenManager.slots[1].SetInputItem(ItemList.instance.itemDic["Gold"]);
            sInvenManager.slots[1].SetInputItem(ItemList.instance.itemDic["Silver"]);
            sInvenManager.slots[2].outputSlot = true;
        }
    }

    void SetRecipe()
    {
        recipeUI = "Furnace";
    }
}
