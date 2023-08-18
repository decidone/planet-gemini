using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class LogisticsCtrl : Structure
{
    protected void Awake()
    {
        GameManager gameManager = GameManager.instance;
        playerInven = gameManager.GetComponent<Inventory>();
        buildName = structureData.FactoryName;
        box2D = GetComponent<BoxCollider2D>();
        hp = structureData.MaxHp[level];
        hpBar.fillAmount = hp / structureData.MaxHp[level];
        repairBar.fillAmount = 0;

        itemPool = new ObjectPool<ItemProps>(CreateItemObj, OnGetItem, OnReleaseItem, OnDestroyItem, maxSize: 100);
    }

    protected virtual void Update()
    {
        if(!removeState)
        {
            if (isRuin && isRepair)
            {
                RepairFunc(false);
            }
            else if (isPreBuilding && isSetBuildingOk && !isRuin)
            {
                RepairFunc(true);
            }
        }
    }

    public List<Item> PlayerGetItemList() //나중에 프러덕션도 추가하는걸로
    {
        List<Item> itemListCopy = new List<Item>(itemList);
        itemList.Clear();
        ItemNumCheck();

        return itemListCopy;
    }

    public void BeltGroupSendItem(ItemProps itemObj)
    {
        itemObjList.Add(itemObj);
        itemObj.setOnBelt = GetComponent<BeltCtrl>();
        if (itemObjList.Count >= structureData.MaxItemStorageLimit)        
            isFull = true;        
        else
            isFull = false;
    }

    public bool OnBeltItem(ItemProps itemObj)
    {
        if(itemObjList.Count < structureData.MaxItemStorageLimit)
        {
            itemObjList.Add(itemObj);

            if (GetComponent<BeltCtrl>())
                GetComponent<BeltCtrl>().beltGroupMgr.groupItem.Add(itemObj);

            if (itemObjList.Count >= structureData.MaxItemStorageLimit)
                isFull = true;
            else
                isFull = false;

            return true;
        }
        return false;
    }

    public override void OnFactoryItem(ItemProps itemProps)
    {
        itemList.Add(itemProps.item);

        OnDestroyItem(itemProps);
        if (itemList.Count >= structureData.MaxItemStorageLimit)
        {
            isFull = true;
        }
    }

    public override void OnFactoryItem(Item item)
    {
        itemList.Add(item);

        if (itemList.Count >= structureData.MaxItemStorageLimit)
        {
            isFull = true;
        }
    }

    public override void ItemNumCheck()
    {
        if (GetComponent<BeltCtrl>())
        {
            if (itemObjList.Count >= structureData.MaxItemStorageLimit)
            {
                isFull = true;
            }
            else
                isFull = false;
        }
        else
        {
            if (itemList.Count >= structureData.MaxItemStorageLimit)
            {
                isFull = true;
            }
            else
                isFull = false;
        }
    }

    protected override void AddInvenItem()
    {
        if (GetComponent<BeltCtrl>())
        {
            if (itemObjList.Count > 0)
            {
                foreach (ItemProps itemProps in itemObjList)
                {
                    playerInven.Add(itemProps.item, itemProps.amount);
                    OnDestroyItem(itemProps);
                }
            }
        }
        else
        {
            if (itemList.Count > 0)
            {
                foreach (Item item in itemList)
                {
                    playerInven.Add(item, 1);
                }
            }
        }
    }
}
