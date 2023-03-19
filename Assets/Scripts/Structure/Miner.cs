using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Miner : Production
{
    [SerializeField]
    int maxAmount;
    [SerializeField]
    float cooldown;

    public string recipeUI;
    public Item item;   // �ӽ÷� ����Ƽ���� ���� ��. ���߿� ���� ��ɵ��� ����� setResource���� ó���� ��
    int amount;
    Inventory inventory;
    public Dictionary<string, Item> itemDic;
    float timer;

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
        timer += Time.deltaTime;
        if (amount < maxAmount)
        {
            if (timer > cooldown)
            {
                inventory.Add(item, 1);
                timer = 0;
            }
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
