using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeBuild : DragFunc
{
    protected GameObject canvas;
    BuildingData buildingData;
    Inventory inventory;
    int structureLayer;

    void Start()
    {
        GameManager gameManager = GameManager.instance;
        canvas = gameManager.GetComponent<GameManager>().inventoryUiCanvas;
        inventory = gameManager.GetComponent<Inventory>();
        structureLayer = LayerMask.NameToLayer("Obj");
    }

    public override void LeftMouseUp(Vector2 startPos, Vector2 endPos)
    {
        if (startPos != endPos)
            GroupSelectedObjects(startPos, endPos, structureLayer);
        else
            UpgradeClick(startPos);
    }

    protected override List<GameObject> GroupSelectedObjects(Vector2 startPosition, Vector2 endPosition, int layer)
    {
        List<GameObject> List = base.GroupSelectedObjects(startPosition, endPosition, layer);
        selectedObjects = List.ToArray();

        foreach (GameObject obj in selectedObjects)
        {
            ObjUpgradeFunc(obj);
        }

        return null;
    }

    void UpgradeClick(Vector2 mousePos)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);
        if (hits.Length > 0)
        {
            foreach (RaycastHit2D hit in hits)
            {
                ObjUpgradeFunc(hit.collider.gameObject);
            }
        }
    }

    void ObjUpgradeFunc(GameObject obj)
    {
        if (obj.TryGetComponent(out BeltCtrl structure) && !structure.isPreBuilding) // 일단 임시로 벨트로 적용
        {
            if (structure.structureData.MaxLevel == structure.level + 1)
                return;

            buildingData = new BuildingData();
            buildingData = BuildingDataGet.instance.GetBuildingName(structure.buildName, structure.level + 1);
            BuildingData buildUpgradeData = new BuildingData();
            buildUpgradeData = BuildingDataGet.instance.GetBuildingName(structure.buildName, structure.level + 2);

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
                structure.level++;
            }
            GameManager.instance.BuildAndSciUiReset();
        }
    }
}
