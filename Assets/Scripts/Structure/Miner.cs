using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Miner : Production
{
    protected override void Start()
    {
        base.Start();
        SetResource(itemDic["Coal"]);
    }

    protected override void Update()
    {
        base.Update();
        var slot = inventory.SlotCheck(0);
        if (slot.amount < maxAmount)
        {
            prodTimer += Time.deltaTime;
            if (prodTimer > cooldown)
            {
                inventory.Add(output, 1);
                prodTimer = 0;
            }
        }

        if (slot.amount > 0 && outObj.Count > 0 && !itemSetDelay)
        {
            SetItem();
        }
    }

    public override void OpenUI()
    {
        sInvenManager.SetInven(inventory, ui);
        sInvenManager.SetProd(this);
        sInvenManager.progressBar.SetMaxProgress(cooldown);
        
        sInvenManager.slots[0].outputSlot = true;
    }

    public override void CloseUI()
    {
        sInvenManager.ReleaseInven();
    }

    void SetResource(Item item)
    {
        // 생산 자원을 지정
        output = item;
    }

    protected override void SubFromInventory()
    {
        inventory.Sub(0, 1);
    }

    protected override bool CheckOutItemNum() 
    {
        var slot = inventory.SlotCheck(0);
        if (slot.amount > 0)
            return true;
        else
            return false;
    }


    public override void ItemNumCheck()
    {
        var slot = inventory.SlotCheck(0);

        if (slot.amount < maxAmount)
            isFull = false;        
        else
            isFull = true;
    }
}
