using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Disintegrator : Production
{
    [SerializeField] MerchandiseListSO merchandiseList;
    Button confirmBtn;
    public Scrap scrap;

    protected override void Start()
    {
        base.Start();
        isStorageBuilding = true;
    }

    protected override void Update()
    {
        base.Update();
    }

    public void CheckTotalAmount()
    {
        int totalAmount = 0;

        for (int i = 0; i < inventory.space; i++)
        {
            if (inventory.items.ContainsKey(i))
            {
                for (int j = 0; j < merchandiseList.MerchandiseSOList.Count; j++)
                {
                    if (inventory.items[i] == merchandiseList.MerchandiseSOList[j].item)
                    {
                        totalAmount += (merchandiseList.MerchandiseSOList[j].sellPrice * inventory.amounts[i]);
                        break;
                    }
                }
            }
        }

        scrap.SetScrap(totalAmount);
    }

    public void ConfirmBtnClicked()
    {
        bool confirm = false;
        for (int i = 0; i < inventory.space; i++)
        {
            if (inventory.items.ContainsKey(i))
            {
                for (int j = 0; j < merchandiseList.MerchandiseSOList.Count; j++)
                {
                    if (inventory.items[i] == merchandiseList.MerchandiseSOList[j].item)
                    {
                        GameManager.instance.AddScrapServerRpc(merchandiseList.MerchandiseSOList[j].sellPrice * inventory.amounts[i]);
                        inventory.RemoveServerRpc(i);
                        confirm = true;
                        break;
                    }
                }
            }
        }

        if (confirm)
        {
            animator.Play("StartAction", -1, 0);
            soundManager.PlaySFX(gameObject, "structureSFX", "Disintegrator");
        }
    }

    public override void OpenUI()
    {
        base.OpenUI();
        sInvenManager.SetInven(inventory, ui);
        sInvenManager.SetProd(this);
        sInvenManager.progressBar.gameObject.SetActive(false);
        sInvenManager.energyBar.gameObject.SetActive(false);

        scrap = ui.GetComponentInChildren<Scrap>();
        if (confirmBtn == null)
            confirmBtn = ui.transform.Find("ConfirmBtn").GetComponent<Button>();
        confirmBtn.onClick.AddListener(ConfirmBtnClicked);
        inventory.onItemChangedCallback += CheckTotalAmount;
        CheckTotalAmount();
    }

    public override void CloseUI()
    {
        base.CloseUI();
        sInvenManager.progressBar.gameObject.SetActive(true);
        sInvenManager.energyBar.gameObject.SetActive(true);
        sInvenManager.ReleaseInven();

        inventory.onItemChangedCallback -= CheckTotalAmount;
        confirmBtn.onClick.RemoveAllListeners();
        scrap = null;
    }

    public override bool CanTakeItem(Item item)
    {
        bool canTake = false;
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
        if (IsServer)
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
            if (list.name == "Disintegrator")
            {
                ui = list;
            }
        }
    }
}
