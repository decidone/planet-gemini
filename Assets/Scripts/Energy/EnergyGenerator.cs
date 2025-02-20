using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class EnergyGenerator : Production
{
    #region Memo
    /*
     * 에너지 발생기
     * 에너지 공급은 따로 스크립트 주고 컴포넌트 추가
     * 해당 스크립트에서는 에너지 생산 후 소속 에너지 그룹에 전달하는 역할만 담당
     * 
     * 아래는 상속받아서 만들어지는 스크립트/오브젝트
     * 아니면 상속 없이 수치만 조금 조정해서 같은 스크립트로 사용할 수도 있음
     * 
     * 0. 디버그나 샌드박스모드용 무한발전기
     * 1. 석탄을 사용하는 화력발전기
     * 2. 화력발전기에 충분한 물을 공급해 줄 수 있을 때(기본 화력발전기와는 분리)
     * 3. 석유 정제 연료나 마석을 사용한 발전기(물 사용여부는 밸런싱 작업에서 생각)
     * 
     * 기능
     * 1. 에너지 생산
     * 2. 생산된 에너지 소속그룹에 전달
    */
    #endregion

    public EnergyGroupConnector connector;
    public Item FuelItem;
    [SerializeField]
    SpriteRenderer view;
    bool isBuildDone;
    bool isPlaced;
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
        isPlaced = false;
        preBuildingCheck = false;
        gameManager = GameManager.instance;
        preBuilding = PreBuilding.instance;
    }

    protected override void Update()
    {
        base.Update();

        if (!isPlaced)
        {
            if (isSetBuildingOk)
            {
                view.enabled = false;
                isPlaced = true;
            }
        }
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

            var slot = inventory.SlotCheck(0);
            if (fuel <= 50 && slot.item == FuelItem && slot.amount > 0)
            {
                if (IsServer)
                {
                    inventory.SlotSubServerRpc(0, 1);
                    Overall.instance.OverallConsumption(slot.item, 1);
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

    public override void WarningStateCheck()
    {
        if (!isPreBuilding && warningIcon != null)
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
        var slot = inventory.SlotCheck(0);
        if (FuelItem == item && slot.amount < 99)
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
    }
}
