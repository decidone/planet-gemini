using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UpgradeBuild : DragFunc
{
    protected GameObject canvas;
    BuildingData buildingData;

    Dictionary<Item, int> upgradeItemDic = new Dictionary<Item, int>();
    Dictionary<Item, int> enoughItemDic = new Dictionary<Item, int>();
    Dictionary<Item, int> notEnoughItemDic = new Dictionary<Item, int>();

    protected override void Start()
    {
        base.Start();
        canvas = gameManager.GetComponent<GameManager>().inventoryUiCanvas;
    }

    public override void LeftMouseUp(Vector2 startPos, Vector2 endPos)
    {
        upgradeItemDic.Clear();
        enoughItemDic.Clear();
        notEnoughItemDic.Clear();

        if (startPos != endPos)
            GroupSelectedObjects(startPos, endPos);
        else
            UpgradeClick(startPos);
    }

    protected override void GroupSelectedObjects(Vector2 startPosition, Vector2 endPosition)
    {
        Collider2D[] colliders = Physics2D.OverlapAreaAll(startPosition, endPosition, 1 << interactLayer);

        List<WorldObj> selectedObjectsList = new List<WorldObj>();

        foreach (Collider2D collider in colliders)
        {
            WorldObj worldObj = collider.GetComponentInParent<WorldObj>();
            if (!worldObj)
                continue; 
            
            Structure structure = worldObj.Get<Structure>();

            if (structure == null)
                continue;
            if (structure.isPreBuilding)
                continue;
            if (worldObj.Get<Portal>() || worldObj.Get<ScienceBuilding>())
                continue;
            if (structure.structureData.MaxLevel == structure.level + 1 || !ScienceDb.instance.IsLevelExists(structure.buildName, structure.level + 2))
                continue;

            selectedObjectsList.Add(structure);
        }

        selectedObjects = selectedObjectsList.ToArray();

        foreach (WorldObj obj in selectedObjects)
        {
            GroupUpgradeCost(obj.Get<Structure>());
        }
        UpgradeCheck();
    }

    void UpgradeClick(Vector2 mousePos)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);
        selectedObjects = new WorldObj[1];
        if (hits.Length > 0)
        {
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider.TryGetComponent(out InfoInteract info))
                {
                    WorldObj worldObj = info.GetComponentInParent<WorldObj>();
                    if (worldObj && worldObj.TryGet(out Structure structure) && !structure.isPreBuilding)
                    {
                        if (!(structure.Get<Portal>() || structure.Get<ScienceBuilding>()))
                        {
                            if (structure.structureData.MaxLevel != structure.level + 1)
                            {
                                if (ScienceDb.instance.IsLevelExists(structure.buildName, structure.level + 2))
                                {
                                    // 업그레이드 가능
                                    selectedObjects[0] = structure;
                                    GroupUpgradeCost(structure);
                                    UpgradeCheck();
                                }
                                else
                                {
                                    // 상위 테크 건물은 있는데 아직 연구가 완료되지 않은 경우
                                    Debug.Log("need to research next level building");
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public void UpgradeBtnClicked(Structure str)
    {
        upgradeItemDic.Clear();
        enoughItemDic.Clear();
        notEnoughItemDic.Clear();

        if (!str.isPreBuilding)
        {
            if (!(str.Get<Portal>() || str.Get<ScienceBuilding>()))
            {
                if (str.structureData.MaxLevel != str.level + 1)
                {
                    if (ScienceDb.instance.IsLevelExists(str.buildName, str.level + 2))
                    {
                        // 업그레이드 가능
                        selectedObjects = new WorldObj[1];
                        selectedObjects[0] = str;
                        GroupUpgradeCost(str);
                        UpgradeCheck();
                    }
                    else
                    {
                        // 상위 테크 건물은 있는데 아직 연구가 완료되지 않은 경우
                        // 여기서는 ui동기화에 문제가 생겨서 이미 업그레이드가 됐는데 버튼이 남아있는 경우를 처리
                        Debug.Log("need to research next level building");
                        InfoUI.instance.RefreshStrInfo();
                    }
                }
            }
        }
    }

    void UpgradeCheck()
    {
        if (selectedObjects.Length > 0)
        {
            EnoughCheck();
            if (enoughItemDic.Count == 0 && notEnoughItemDic.Count == 0)
                ConfirmEnd(false);
            else
                gameManager.inventoryUiCanvas.GetComponent<PopUpManager>().upgradeConfirm.GetData(enoughItemDic, notEnoughItemDic, "UpgradeBuild");
        }
    }

    public void ConfirmEnd(bool isOk)
    {
        if (isOk)
        {
            gameManager.StructureUpgrade(selectedObjects);
            InfoUI.instance.RefreshStrInfo();
        }
        selectedObjects = new WorldObj[0];
        upgradeItemDic.Clear();
        enoughItemDic.Clear();
        notEnoughItemDic.Clear();
    }

    void GroupUpgradeCost(Structure obj)   // 업그레이드 가격 측정
    {
        BuildingData buildUpgradeData = new BuildingData();
        buildUpgradeData = CanUpgradeCheck(obj);

        if (buildUpgradeData != null)
        {
            BuildingData UpgradeCost = new BuildingData(new List<string>(), new List<int>());

            int index;
            int difference;

            foreach (string item in buildingData.items)
            {
                if (buildUpgradeData.items.Contains(item)) // 아이템이 겹치는게 존재할 경우 차액만큼 인벤토리에서 차감
                {
                    index = buildingData.items.IndexOf(item);
                    difference = buildUpgradeData.amounts[index] - buildingData.amounts[index];
                    UpgradeCost.items.Add(item);
                    UpgradeCost.amounts.Add(difference);
                }
            }
            foreach (string item in buildUpgradeData.items)
            {
                if (!buildingData.items.Contains(item)) // 업그레이드에 필요한 추가적인 아이템이 존재할 경우 인벤토리에서 차감
                {
                    index = buildUpgradeData.items.IndexOf(item);
                    difference = buildUpgradeData.amounts[index];
                    UpgradeCost.items.Add(item);
                    UpgradeCost.amounts.Add(difference);
                }
            }

            for (int i = 0; i < UpgradeCost.GetItemCount(); i++)
            {
                Item item = ItemList.instance.itemDic[UpgradeCost.items[i]];

                if (UpgradeCost.amounts[i] == 0)
                {
                    continue;
                }
                else if (upgradeItemDic.ContainsKey(item))
                {
                    int currentValue = upgradeItemDic[item];
                    int newValue = currentValue + UpgradeCost.amounts[i];
                    upgradeItemDic[item] = newValue;
                }
                else
                {
                    upgradeItemDic.Add(item, UpgradeCost.amounts[i]);
                }
            }
        }
        else
            return;
    }

    void EnoughCheck()
    {
        bool isEnough;

        foreach (var kvp in upgradeItemDic)
        {
            Item key = kvp.Key;
            int value = kvp.Value;

            int amount;
            bool hasItem = gameManager.inventory.totalItems.TryGetValue(key, out amount);
            isEnough = hasItem && amount >= value;

            if (!isEnough)
                notEnoughItemDic.Add(key, value - amount);
            else
                enoughItemDic.Add(key, value);
        }
    }

    BuildingData CanUpgradeCheck(Structure structure)
    {   // 벨트 스프리터 벽 창고
        if (structure.canUpgrade && !structure.isPreBuilding)
        {
            buildingData = new BuildingData();
            buildingData = BuildingDataGet.instance.GetBuildingName(structure.buildName, structure.level + 1);
            BuildingData buildUpgradeData = new BuildingData();
            buildUpgradeData = BuildingDataGet.instance.GetBuildingName(structure.buildName, structure.level + 2);
            return buildUpgradeData;
        }

        return null;
    }
}