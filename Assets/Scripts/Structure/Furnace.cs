using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Furnace : Production
{
    [SerializeField]
    int maxAmount;
    [SerializeField]
    float cooldown;

    public string recipeUI;
    int amount;
    Inventory inventory;
    Item item;
    float timer;

    void Start()
    {
        inventory = this.GetComponent<Inventory>();
        amount = 0;
        item = ItemList.instance.itemDic["Amethyst"];
        // ������ �����ϴ� �κ� �ӽ� ����.
        // ���߿� �÷��̾ ������ �����ϴ� ����� ����� �ش� �޼���� ����
        SetRecipe();
    }

    void Update()
    {
        if (inventory.SlotCheck(0) != 0 && inventory.SlotCheck(1) != 0 && inventory.SlotCheck(2) < maxAmount)
        {
            timer += Time.deltaTime;
            if (amount < maxAmount)
            {
                if (timer > cooldown)
                {
                    inventory.Sub(0, 1);
                    inventory.Sub(1, 1);
                    inventory.SlotAdd(2, item, 1);
                    timer = 0;
                }
            }
        }
    }

    void SetRecipe()
    {
        recipeUI = "Furnace";
    }
}
