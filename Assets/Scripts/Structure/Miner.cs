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
    Item item;   // �ӽ÷� ����Ƽ���� ���� ��. ���߿� ���� ��ɵ��� ����� setResource���� ó���� ��
    int amount;
    Inventory inventory;
    Dictionary<string, Item> itemDic;
    float prodTimer;

    void Start()
    {
        inventory = this.GetComponent<Inventory>();
        amount = 0;
        itemDic = ItemList.instance.itemDic;
        // ������ �����ϴ� �κ� �ӽ� ����.
        // ���߿� �÷��̾ ������ �����ϴ� ����� ����� �ش� �޼���� ����
        SetRecipe();
        SetResource(itemDic["Coal"]);
    }

    void Update()
    {
        amount = inventory.totalItems[item];
        prodTimer += Time.deltaTime;
        if (amount < maxAmount)
        {
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
        // �Ƹ� �Ǽ��� �� üũ �� �� �ڷδ� ��� �� ��. �׷��� �̸��� setResource�� ����
    }

    void SetRecipe()
    {
        recipeUI = "Miner";
    }
}
