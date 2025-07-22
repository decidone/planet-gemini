using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Storage : Production
{
    int [] invenSize;

    protected override void Start()
    {
        base.Start();
        //setModel = GetComponent<SpriteRenderer>();
        isStorageBuilding = true;
        invenSize = new int[5] { 6, 12, 18, 24, 30 };
    }

    //protected override void Update()
    //{
    //    base.Update();
    //    //SetDirNum();

    //    // 여기 업글시 수정되는걸로 바꿔야 함
    //    //CheckPos();
    //    //setModel.sprite = modelNum[dirNum + level];
    //    //inventory.space = invenSize[level];
    //}

    [ClientRpc]
    public override void UpgradeFuncClientRpc()
    {
        base.UpgradeFuncClientRpc();
        setModel.sprite = modelNum[dirNum + level];
        inventory.space = invenSize[level];
    }


    public override void OpenUI()
    {
        base.OpenUI();
        sInvenManager.SetInven(inventory, ui, invenSize[level]);
        sInvenManager.SetProd(this);
        sInvenManager.progressBar.gameObject.SetActive(false);
        sInvenManager.energyBar.gameObject.SetActive(false);
        sInvenManager.sortBtn.gameObject.SetActive(true);
        sInvenManager.sortBtn.onClick.AddListener(SortInven);
    }

    public override void CloseUI()
    {
        base.CloseUI();
        sInvenManager.progressBar.gameObject.SetActive(true);
        sInvenManager.energyBar.gameObject.SetActive(true);
        sInvenManager.sortBtn.gameObject.SetActive(false);
        sInvenManager.sortBtn.onClick.RemoveAllListeners();
        sInvenManager.ReleaseInven();
    }

    public override bool CanTakeItem(Item item)
    {
        if (isInvenFull) return false;

        bool canTake;
        int containableAmount = inventory.SpaceCheck(item);

        if (1 <= containableAmount)
        {
            canTake = true;
        }
        else if (containableAmount != 0)
        {
            canTake = true;
        }
        else
        {
            canTake = false;
        }

        return canTake;
    }

    public override void OnFactoryItem(ItemProps itemProps)
    {
        if(IsServer)
            inventory.StorageAdd(itemProps.item, itemProps.amount);
        itemProps.itemPool.Release(itemProps.gameObject);
    }

    public override void OnFactoryItem(Item item)
    {
        if (IsServer)
            inventory.StorageAdd(item, 1);
    }

    public override void GetUIFunc()
    {
        InventoryList inventoryList = canvas.GetComponent<InventoryList>();

        foreach (GameObject list in inventoryList.StructureStorageArr)
        {
            if (list.name == "Storage")
            {
                ui = list;
            }
        }
    }
}
