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

    Item item;
    Inventory inventory;
    Dictionary<string, Item> itemDic;
    float prodTimer;
    bool activeUI;

    void Start()
    {
        inventory = this.GetComponent<Inventory>();
        itemDic = ItemList.instance.itemDic;
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

    void SetResource(Item _item)
    {
        // 생산 자원을 지정
        item = _item;
    }

    public override void OpenUI()
    {
        sInvenManager.SetInven(inventory, ui);
        sInvenManager.slots[0].outputSlot = true;
        sInvenManager.progressBar.SetMaxProgress(cooldown);
        activeUI = true;
    }

    public override void CloseUI()
    {
        sInvenManager.ReleaseInven();
        activeUI = false;
    }
}
