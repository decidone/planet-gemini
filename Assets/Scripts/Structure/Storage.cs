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
        inventory.space = invenSize[level];
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

    public override void NearStrBuilt()
    {
        // 건물을 지었을 때나 근처에 새로운 건물이 지어졌을 때 동작
        // 변경사항이 생기면 DelayNearStrBuiltCoroutine()에도 반영해야 함
        if (IsServer)
        {
            CheckPos();
            for (int i = 0; i < nearObj.Length; i++)
            {
                if (nearObj[i] == null && sizeOneByOne)
                {
                    CheckNearObj(checkPos[i], i, obj => StartCoroutine(SetOutObjCoroutine(obj)));
                }
                else if (nearObj[i] == null && !sizeOneByOne)
                {
                    CheckNearObj(i, obj => StartCoroutine(SetOutObjCoroutine(obj)));
                }
            }
            setModel.sprite = modelNum[level];
        }
        else
        {
            DelayNearStrBuilt();
        }
    }

    protected override IEnumerator DelayNearStrBuiltCoroutine()
    {
        // 동시 건설이나 그룹핑을 따로 예외처리 하는 경우가 아니면 NearStrBuilt()를 그대로 사용
        yield return new WaitForEndOfFrame();

        CheckPos();
        for (int i = 0; i < nearObj.Length; i++)
        {
            if (nearObj[i] == null && sizeOneByOne)
            {
                CheckNearObj(checkPos[i], i, obj => StartCoroutine(SetOutObjCoroutine(obj)));
            }
            else if (nearObj[i] == null && !sizeOneByOne)
            {
                CheckNearObj(i, obj => StartCoroutine(SetOutObjCoroutine(obj)));
            }
        }
        setModel.sprite = modelNum[level];
    }

    [ClientRpc]
    public override void UpgradeFuncClientRpc()
    {
        //base.UpgradeFuncClientRpc();
        UpgradeFunc();

        setModel.sprite = modelNum[level];
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
