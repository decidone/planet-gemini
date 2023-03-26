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
    bool activeUI;

    void Start()
    {
        inventory = this.GetComponent<Inventory>();
        itemDic = ItemList.instance.itemDic;
        // 레시피 설정하는 부분 임시 설정.
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

        if (activeUI)
            sInvenManager.progressBar.SetProgress(prodTimer);
    }

    public void OpenUI()
    {
        if (recipeUI == "Miner")
        {
            sInvenManager.SetInven(inventory, miner);
            sInvenManager.slots[0].outputSlot = true;
            sInvenManager.progressBar.SetMaxProgress(cooldown);
            activeUI = true;
        }
    }

    public void CloseUI()
    {
        if (recipeUI == "Miner")
        {
            sInvenManager.ReleaseInven();
            activeUI = false;
        }
    }

    void SetResource(Item _item)
    {
        item = _item;
        // 매장된 자원 확인, 생산 자원을 지정
    }

    void SetRecipe()
    {
        recipeUI = "Miner";
    }
}
