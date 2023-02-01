using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Miner : Production
{
    // ����� �ڿ� Ȯ��, ������
    public string recipeUI;
    // �ӽ÷� ����Ƽ���� ���� ��. ���߿� ���� ��ɵ��� ����� setResource���� ó���� ��
    public Item item;
    public int maxAmount;
    public float cooldown;

    int amount;
    Inventory inventory;
    private float timer;

    private void Start()
    {
        // ������ �����ϴ� �κ� �ӽ� ����.
        // ���߿� �÷��̾ ������ �����ϴ� ����� ����� �ش� �޼���� ����
        SetRecipe();

        inventory = this.GetComponent<Inventory>();
        amount = 0;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (amount < maxAmount)
        {
            // 0.1�ʸ��� �Է� ���¸� ����
            if (timer > cooldown)
            {
                inventory.Add(item, 1, true);
                timer = 0;
            }
        }
    }

    public void SetResource()
    {
        // ����� �ڿ� Ȯ��, ���� �ڿ��� ����
        // �Ƹ� �Ǽ��� �� üũ �� �� �ڷδ� ��� �� ��. �׷��� �̸��� setResource�� ����
    }

    public void SetRecipe()
    {
        recipeUI = "OneStorage";
    }
}
