using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    float prodDelay;
    public EnergyGroupConnector connector;
    [SerializeField]
    SpriteRenderer view;
    bool isBuildDone;
    bool isPlaced;
    GameManager gameManager;
    [HideInInspector]
    public GameObject preBuildingObj;
    Structure preBuildingStr;
    bool preBuildingCheck;

    //연료 시스템으로 가동, 석탄같은 연료 하나에 연료 게이지를 일정량 채우고 0이 되지 않도록 유지. 0이되면 off

    protected override void Start()
    {
        base.Start();
        prodDelay = 3f;
        maxFuel = 100;
        isBuildDone = false;
        isPlaced = false;
        preBuildingCheck = false;
        gameManager = GameManager.instance;
        preBuildingObj = gameManager.preBuildingObj;
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
            if (preBuildingObj.activeSelf)
            {
                if (!preBuildingCheck)
                {
                    preBuildingCheck = true;
                    preBuildingStr = preBuildingObj.GetComponentInChildren<Structure>();
                    if (preBuildingStr != null && (preBuildingStr.energyUse || preBuildingStr.isEnergyStr))
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
            if (isOperate && fuel <= 0)
            {
                isOperate = false;
            }

            var slot = inventory.SlotCheck(0);
            if (fuel <= 50 && slot.item == itemDic["Coal"] && slot.amount > 0)
            {
                inventory.Sub(0, 1);
                fuel += 50;
                isOperate = true;
            }
            if (isOperate)
            {
                prodTimer += Time.deltaTime;
                if (prodTimer > prodDelay)
                {
                    fuel -= 10;
                    prodTimer = 0;
                }
            }
        }
    }

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

    public override void RemoveObj()
    {
        //여기서 건물 철거 전 처리(삭제가 아니여도 비활성화가 필요하니 그거 생각해서 만들 것)
        connector.RemoveFromGroup();

        base.RemoveObj();
    }

    public override void OpenUI()
    {
        base.OpenUI();
        sInvenManager.SetInven(inventory, ui);
        sInvenManager.SetProd(this);
        sInvenManager.progressBar.SetMaxProgress(100);
        sInvenManager.slots[0].SetInputItem(ItemList.instance.itemDic["Coal"]);
    }

    public override void CloseUI()
    {
        base.CloseUI();
        sInvenManager.ReleaseInven();
    }

    public override float GetProgress() { return fuel; }

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
}
