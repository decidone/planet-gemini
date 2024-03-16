using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UpgradeBuild : DragFunc
{
    protected GameObject canvas;
    BuildingData buildingData;
    Inventory inventory;
    int structureLayer;

    Dictionary<Item, int> upgradeItemDic = new Dictionary<Item, int>();
    Dictionary<Item, int> enoughItemDic = new Dictionary<Item, int>();
    Dictionary<Item, int> notEnoughItemDic = new Dictionary<Item, int>();

    protected override void Start()
    {
        base.Start();
        canvas = gameManager.GetComponent<GameManager>().inventoryUiCanvas;
        inventory = gameManager.inventory;
        structureLayer = LayerMask.NameToLayer("Obj");
    }

    public override void LeftMouseUp(Vector2 startPos, Vector2 endPos)
    {
        if (startPos != endPos)
            GroupSelectedObjects(startPos, endPos, structureLayer);
        else
            UpgradeClick(startPos);

        upgradeItemDic.Clear();
        enoughItemDic.Clear();
        notEnoughItemDic.Clear();
    }

    protected override void GroupSelectedObjects(Vector2 startPosition, Vector2 endPosition, int layer)
    {
        base.GroupSelectedObjects(startPosition, endPosition, layer);

        foreach (GameObject obj in selectedObjects)
        {
            GroupUpgradeCost(obj);
        }
        UpgradeCheck();
    }

    void UpgradeClick(Vector2 mousePos)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);
        selectedObjects = new GameObject[1];
        if (hits.Length > 0)
        {
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider.TryGetComponent(out BeltCtrl structure) && !structure.isPreBuilding)
                {
                    selectedObjects[0] = hit.collider.gameObject;
                    GroupUpgradeCost(selectedObjects[0]);
                    UpgradeCheck();
                }
            }
        }
    }

    void UpgradeCheck()
    {
        if(selectedObjects.Length > 0)
        {
            EnoughCheck();
            if (enoughItemDic.Count == 0 && notEnoughItemDic.Count == 0)
                ConfirmEnd(false);
            else
                gameManager.inventoryUiCanvas.GetComponent<PopUpManager>().upgradeConfirm.GetData(enoughItemDic, notEnoughItemDic);
        }
    }

    public void ConfirmEnd(bool isOk)
    {
        if (isOk)
        {
            foreach (GameObject obj in selectedObjects)
            {
                ObjUpgradeFunc(obj);
            }
            gameManager.BuildAndSciUiReset();
        }
        selectedObjects = new GameObject[0];
        upgradeItemDic.Clear();
        enoughItemDic.Clear();
        notEnoughItemDic.Clear();
    }

    void GroupUpgradeCost(GameObject obj)
    {
        BuildingData buildUpgradeData = new BuildingData();
        buildUpgradeData = CanUpgradeCheck(obj);

        if (buildUpgradeData != null)
        {
            BuildingData UpgradeCost = new BuildingData(new List<string>(), new List<int>(), 0);

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

                if (upgradeItemDic.ContainsKey(item))
                {
                    int currentValue = upgradeItemDic[item];
                    int newValue = currentValue + UpgradeCost.amounts[i];
                    upgradeItemDic[item] = newValue;
                }
                else
                    upgradeItemDic.Add(item, UpgradeCost.amounts[i]);
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
            bool hasItem = inventory.totalItems.TryGetValue(key, out amount);
            isEnough = hasItem && amount >= value;

            if (!isEnough)
                notEnoughItemDic.Add(key, value - amount);
            else
                enoughItemDic.Add(key, value);
        }
    }

    BuildingData CanUpgradeCheck(GameObject obj)
    {
        if (obj.TryGetComponent(out LogisticsCtrl logistics) && !logistics.isPreBuilding) // 일단 임시로 벨트로 적용
        {
            if (logistics.GetComponent<ItemSpawner>() || logistics.structureData.MaxLevel == logistics.level + 1)
                return null;

            buildingData = new BuildingData();
            buildingData = BuildingDataGet.instance.GetBuildingName(logistics.buildName, logistics.level + 1);
            BuildingData buildUpgradeData = new BuildingData();
            buildUpgradeData = BuildingDataGet.instance.GetBuildingName(logistics.buildName, logistics.level + 2);
            return buildUpgradeData;
        }

        return null;
    }

    void ObjUpgradeFunc(GameObject obj)
    {
        BuildingData buildUpgradeData = new BuildingData();
        buildUpgradeData = CanUpgradeCheck(obj);

        if (buildUpgradeData != null)
        {
            BuildingData UpgradeCost = new BuildingData(new List<string>(), new List<int>(), 0);
            BuildingData ReturnUpgradeCost = new BuildingData(new List<string>(), new List<int>(), 0);

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
                else // 아이템이 겹치는게 존재하지 않을 경우 인벤토리에 추가
                {
                    index = buildingData.items.IndexOf(item);
                    difference = buildingData.amounts[index];
                    ReturnUpgradeCost.items.Add(item);
                    ReturnUpgradeCost.amounts.Add(difference);
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

            bool totalAmountsEnough = true;
            bool isEnough;

            for (int i = 0; i < UpgradeCost.GetItemCount(); i++)
            {
                int value;
                bool hasItem = inventory.totalItems.TryGetValue(ItemList.instance.itemDic[UpgradeCost.items[i]], out value);
                isEnough = hasItem && value >= UpgradeCost.amounts[i];

                if (isEnough && totalAmountsEnough)
                    totalAmountsEnough = true;
                else
                    totalAmountsEnough = false;
            }

            if (totalAmountsEnough)
            {
                for (int i = 0; i < ReturnUpgradeCost.GetItemCount(); i++)
                {
                    inventory.Add(ItemList.instance.itemDic[ReturnUpgradeCost.items[i]], ReturnUpgradeCost.amounts[i]);
                }

                for (int i = 0; i < UpgradeCost.GetItemCount(); i++)
                {
                    inventory.Sub(ItemList.instance.itemDic[UpgradeCost.items[i]], UpgradeCost.amounts[i]);
                }
                obj.GetComponent<LogisticsCtrl>().level++;
            }
            gameManager.BuildAndSciUiReset();
        }
        else
            return;
        }
}