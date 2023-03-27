using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Miner : Production
{
    [SerializeField]
    int maxAmount;
    [SerializeField]
    float cooldown;
    [SerializeField]
    StructureInvenManager sInvenManager;
    [SerializeField]
    GameObject miner;
    string recipeUI;
    Item item;
    Inventory inventory;
    Dictionary<string, Item> itemDic;
    float prodTimer;

    void Start()
    {
        inventory = this.GetComponent<Inventory>();
        itemDic = ItemList.instance.itemDic;
        // ������ �����ϴ� �κ� �ӽ� ����.
        SetRecipe();
        SetResource(itemDic["Coal"]);
    }

    void Update()
    {
        var slot = inventory.SlotCheck(0);
        if (slot.amount < maxAmount)
        {
            prodTimer += Time.deltaTime;
            if (prodTimer > cooldown)
            {
                inventory.Add(item, 1);
                prodTimer = 0;
            }
        }
    }

    public void OpenUI()
    {
        if (recipeUI == "Miner")
        {
            miner.SetActive(true);
            sInvenManager.SetInven(inventory, miner);
            sInvenManager.slots[0].outputSlot = true;
        }
    }

    void SetResource(Item _item)
    {
        item = _item;
        // ����� �ڿ� Ȯ��, ���� �ڿ��� ����
    }

    void SetRecipe()
    {
        recipeUI = "Miner";
    }
}
