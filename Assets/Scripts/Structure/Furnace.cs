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
    int amount;
    Inventory inventory;
    Item item;
    float prodTimer;

    void Start()
    {
        inventory = this.GetComponent<Inventory>();
        amount = 0;
        item = ItemList.instance.itemDic["Amethyst"];
        // 레시피 설정하는 부분 임시 설정.
        // 나중에 플레이어가 레시피 설정하는 기능이 생기면 해당 메서드는 제거
        SetRecipe();
    }

    void Update()
    {
        if (inventory.AmountCheck(0) != 0 && inventory.AmountCheck(1) != 0 && inventory.AmountCheck(2) < maxAmount)
        {
            prodTimer += Time.deltaTime;
            if (amount < maxAmount)
            {
                if (prodTimer > cooldown)
                {
                    inventory.Sub(0, 1);
                    inventory.Sub(1, 1);
                    inventory.SlotAdd(2, item, 1);
                    prodTimer = 0;
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
            sInvenManager.slots[2].outputSlot = true;
        }
    }

    void SetRecipe()
    {
        recipeUI = "Furnace";
    }
}
