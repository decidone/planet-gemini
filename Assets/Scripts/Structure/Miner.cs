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
    float timer;

    void Start()
    {
        inventory = this.GetComponent<Inventory>();
        amount = 0;

        // ������ �����ϴ� �κ� �ӽ� ����.
        // ���߿� �÷��̾ ������ �����ϴ� ����� ����� �ش� �޼���� ����
        SetRecipe();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (amount < maxAmount)
        {
            // 0.1�ʸ��� �Է� ���¸� ����
            if (timer > cooldown)
            {
                inventory.Add(item, 1);
                timer = 0;
            }
        }
    }

    void SetResource()
    {
        // ����� �ڿ� Ȯ��, ���� �ڿ��� ����
        // �Ƹ� �Ǽ��� �� üũ �� �� �ڷδ� ��� �� ��. �׷��� �̸��� setResource�� ����
    }

    void SetRecipe()
    {
        recipeUI = "Miner";
    }
}
