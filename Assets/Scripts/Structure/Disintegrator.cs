using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class Disintegrator : Production
{
    [SerializeField] MerchandiseListSO merchandiseList;
    Button confirmBtn;
    Toggle autoToggle;
    public Scrap scrap;

    protected override void Start()
    {
        base.Start();
        isStorageBuilding = true;
        cooldown = 10f;
        effiCooldown = cooldown;
    }

    protected override void Update()
    {
        base.Update();
        if (!isPreBuilding)
        {
            if (isAuto)
            {
                prodTimer += Time.deltaTime;
                if (prodTimer > effiCooldown - ((overclockOn ? effiCooldown * overclockPer / 100 : 0) + effiCooldownUpgradeAmount))
                {
                    if (IsServer)
                        ConfirmBtnClicked();
                }
            }
        }
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
        GrindServerRpc();
    }

    [ServerRpc (RequireOwnership = false)]
    public void GrindServerRpc()
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
            PlayAnimClientRpc();
            ResetTimerClientRpc();
        }
    }

    [ClientRpc]
    public void PlayAnimClientRpc()
    {
        animator.Play("StartAction", -1, 0);
        soundManager.PlaySFX(gameObject, "structureSFX", "Disintegrator");
    }

    [ClientRpc]
    public void ResetTimerClientRpc()
    {
        prodTimer = 0;
    }

    public void SetAuto(bool auto)
    {
        SetAutoServerRpc(auto);
    }

    [ServerRpc (RequireOwnership = false)]
    public void SetAutoServerRpc(bool auto)
    {
        SetAutoClientRpc(auto);
        ResetTimerClientRpc();
    }

    [ClientRpc]
    public void SetAutoClientRpc(bool auto)
    {
        isAuto = auto;
    }

    [ServerRpc(RequireOwnership = false)]
    public override void ClientConnectSyncServerRpc()
    {
        base.ClientConnectSyncServerRpc();
        SetAutoClientRpc(isAuto);
    }

    public override void OpenUI()
    {
        base.OpenUI();
        sInvenManager.SetInven(inventory, ui);
        sInvenManager.SetProd(this);
        sInvenManager.progressBar.SetMaxProgress(effiCooldown - ((overclockOn ? effiCooldown * overclockPer / 100 : 0) + effiCooldownUpgradeAmount));
        sInvenManager.SetCooldownText(effiCooldown - ((overclockOn ? effiCooldown * overclockPer / 100 : 0) + effiCooldownUpgradeAmount));

        scrap = ui.GetComponentInChildren<Scrap>();
        if (confirmBtn == null)
            confirmBtn = ui.transform.Find("ConfirmBtn").GetComponent<Button>();
        confirmBtn.onClick.AddListener(ConfirmBtnClicked);
        if (autoToggle == null)
            autoToggle = ui.transform.Find("AutoToggle").GetComponent<Toggle>();
        autoToggle.isOn = isAuto;
        autoToggle.onValueChanged.AddListener(SetAuto);
        inventory.onItemChangedCallback += CheckTotalAmount;
        CheckTotalAmount();
    }

    public override void CloseUI()
    {
        base.CloseUI();
        sInvenManager.ReleaseInven();

        inventory.onItemChangedCallback -= CheckTotalAmount;
        confirmBtn.onClick.RemoveAllListeners();
        autoToggle.onValueChanged.RemoveAllListeners();
        autoToggle.isOn = false;
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
