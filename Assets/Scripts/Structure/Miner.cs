using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
public class Miner : Production
{
    protected override void Start()
    {
        base.Start();
        Init();
    }

    protected override void Update()
    {
        base.Update();
        if (!isPreBuilding)
        {
            var slot = inventory.SlotCheck(0);
            if (output != null && slot.amount < maxAmount)
            {
                prodTimer += Time.deltaTime;
                if (prodTimer > cooldown)
                {
                    inventory.Add(output, 1);
                    prodTimer = 0;
                }
            }

            if (slot.amount > 0 && outObj.Count > 0 && !itemSetDelay && checkObj)
            {
                SendItem(output);
            }
        }
    }

    void Init()
    {
        Map map = GameManager.instance.map;
        int x = Mathf.FloorToInt(this.gameObject.transform.position.x);
        int y = Mathf.FloorToInt(this.gameObject.transform.position.y);
        if (map.IsOnMap(x, y))
        {
            if (map.mapData[x][y].resource != null && map.mapData[x][y].resource != "")
            {
                Item item = itemDic[map.mapData[x][y].resource];
                if (item != null)
                {
                    SetResource(item);
                }
                //ObjData objData = map.mapData[x][y].obj.gameObject.GetComponent<ObjData>();
                //if (objData != null)
                //{
                //    if (objData.type == "Ore" && objData.item != null)
                //        SetResource(objData.item);
                //}
            }
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
        output = item;
    }

    protected override void SubFromInventory()
    {
        inventory.Sub(0, 1);
    }

    public override bool CheckOutItemNum() 
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

    public override (Item, int) QuickPullOut()
    {
        var slot = inventory.SlotCheck(0);
        if(slot.amount > 0)
            inventory.Sub(0, slot.amount);
        return slot;
    }

    public override void GetUIFunc()
    {
        InventoryList inventoryList = canvas.GetComponent<InventoryList>();

        foreach (GameObject list in inventoryList.StructureStorageArr)
        {
            if (list.name == "Miner")
            {
                ui = list;
            }
        }
    }
    protected override void AddInvenItem()
    {
        var slot = inventory.SlotCheck(0);

        if (slot.item != null)
        {
            playerInven.Add(slot.item, slot.amount);
        }
    }
}
