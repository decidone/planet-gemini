using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class EnergyGenerator : Production
{
    public EnergyGroupConnector connector;
    public Item FuelItem;
    [SerializeField]
    SpriteRenderer view;
    bool isBuildDone;
    GameManager gameManager;
    PreBuilding preBuilding;
    Structure preBuildingStr;
    bool preBuildingCheck;
    public int fuelRequirement;

    protected override void Start()
    {
        base.Start();
        maxFuel = 100;
        isBuildDone = false;
        preBuildingCheck = false;
        gameManager = GameManager.instance;
        preBuilding = PreBuilding.instance;
        view.enabled = false;
    }

    protected override void Update()
    {
        base.Update();

        if (gameManager.focusedStructure == null)
        {
            if (preBuilding.isBuildingOn && !removeState)
            {
                if (!preBuildingCheck)
                {
                    preBuildingCheck = true;
                    if (preBuilding.isEnergyUse || preBuilding.isEnergyStr)
                    {
                        view.enabled = true;
                    }
                }
            }
            else
            {
                if (preBuildingCheck)
                {
                    preBuildingCheck = false;
                    view.enabled = false;
                }
            }
        }
        if (!isPreBuilding)
        {
            if (!isBuildDone)
            {
                connector.Init();
                isBuildDone = true;
            }

            if (fuel <= 50 && slot.Item1 == FuelItem && slot.Item2 > 0)
            {
                if (IsServer)
                {
                    Overall.instance.OverallConsumption(slot.Item1, 1);
                    inventory.SlotSubServerRpc(0, 1);
                }
                fuel += 50;
                soundManager.PlaySFX(gameObject, "structureSFX", "Flames");
            }

            prodTimer += Time.deltaTime;
            if (prodTimer > cooldown)
            {
                if (fuel >= fuelRequirement && !destroyStart)
                {
                    fuel -= fuelRequirement;
                    OperateStateSet(true);
                    prodTimer = 0;
                }
                else
                {
                    OperateStateSet(false);
                }
            }
        }
    }

    public override void CheckSlotState(int slotindex)
    {
        // update에서 검사해야 하는 특정 슬롯들 상태를 인벤토리 콜백이 있을 때 미리 저장
        slot = inventory.SlotCheck(0);
    }

    protected override IEnumerator CheckWarning()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(1f);

            if (!isPreBuilding && !removeState)
            {
                if (fuel > 0)
                {
                    if (warningIconCheck)
                    {
                        if (warning != null)
                            StopCoroutine(warning);
                        warningIconCheck = false;
                        warningIcon.enabled = false;
                    }
                }
                else
                {
                    if (!warningIconCheck)
                    {
                        if (warning != null)
                            StopCoroutine(warning);
                        warning = FlickeringIcon();
                        StartCoroutine(warning);
                        warningIconCheck = true;
                    }
                }
            }
        }
    }

    public override float GetProgress() { return fuel; }

    public override void Focused()
    {
        if (connector.group != null)
        {
            connector.group.TerritoryViewOn();
        }
    }

    public override void DisableFocused()
    {
        if (connector.group != null)
        {
            connector.group.TerritoryViewOff();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public override void RemoveObjServerRpc()
    {
        DisableFocused();
        connector.RemoveFromGroup();
        base.RemoveObjServerRpc();
    }

    public override void OpenUI()
    {
        base.OpenUI();
        sInvenManager.SetInven(inventory, ui);
        sInvenManager.SetProd(this);
        sInvenManager.progressBar.SetMaxProgress(100);
        sInvenManager.slots[0].SetInputItem(FuelItem);
    }

    public override void CloseUI()
    {
        base.CloseUI();
        sInvenManager.ReleaseInven();
    }

    public override bool CanTakeItem(Item item)
    {
        if (isInvenFull) return false;

        if (FuelItem == item && slot.Item2 < 99)
            return true;

        return false;
    }

    public override void OnFactoryItem(ItemProps itemProps)
    {
        if (IsServer && FuelItem == itemProps.item)
            inventory.SlotAdd(0, itemProps.item, itemProps.amount);

        itemProps.itemPool.Release(itemProps.gameObject);
    }

    public override void OnFactoryItem(Item item)
    {
        if (IsServer && FuelItem == item)
            inventory.SlotAdd(0, item, 1);
    }

    public override void GetUIFunc()
    {
        InventoryList inventoryList = canvas.GetComponent<InventoryList>();

        foreach (GameObject list in inventoryList.StructureStorageArr)
        {
            if (list.name == "Generator")
            {
                ui = list;
            }
        }
    }

    public override (bool, bool, bool, EnergyGroup, float) PopUpEnergyCheck()
    {
        if (connector != null && connector.group != null)
        {
            return (energyUse, isEnergyStr, false, connector.group, energyProduction);
        }

        return (false, false, false, null, 0);
    }

    protected override void NonOperateStateSet(bool isOn)
    {
        setModel.sprite = strImg[isOn ? 1 : 0];
        smokeCtrl.SetSmokeActive(isOn);
    }
}
