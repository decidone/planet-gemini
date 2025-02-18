using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UpgradeBuild : DragFunc
{
    protected GameObject canvas;
    BuildingData buildingData;
    int structureLayer;

    Dictionary<Item, int> upgradeItemDic = new Dictionary<Item, int>();
    Dictionary<Item, int> enoughItemDic = new Dictionary<Item, int>();
    Dictionary<Item, int> notEnoughItemDic = new Dictionary<Item, int>();

    protected override void Start()
    {
        base.Start();
        canvas = gameManager.GetComponent<GameManager>().inventoryUiCanvas;
        structureLayer = LayerMask.NameToLayer("Obj");
    }

    public override void LeftMouseUp(Vector2 startPos, Vector2 endPos)
    {
        upgradeItemDic.Clear();
        enoughItemDic.Clear();
        notEnoughItemDic.Clear();

        if (startPos != endPos)
            GroupSelectedObjects(startPos, endPos, structureLayer);
        else
            UpgradeClick(startPos);
    }

    protected override void GroupSelectedObjects(Vector2 startPosition, Vector2 endPosition, int layer)
    {
        Collider2D[] colliders = Physics2D.OverlapAreaAll(startPosition, endPosition, 1 << layer);

        List<GameObject> selectedObjectsList = new List<GameObject>();

        foreach (Collider2D collider in colliders)
        {
            if (layer == LayerMask.NameToLayer("Obj") && collider.GetComponent<Structure>() == null)
                continue;
            Structure structure = collider.GetComponent<Structure>();
            if (collider.GetComponent<Portal>() || collider.GetComponent<ScienceBuilding>())
                continue;
            if (structure.structureData.MaxLevel == structure.level + 1 || !ScienceDb.instance.IsLevelExists(structure.buildName, structure.level + 2))
                continue;

            selectedObjectsList.Add(collider.gameObject);
        }

        selectedObjects = selectedObjectsList.ToArray();

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
        if (selectedObjects.Length > 0)
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
            gameManager.StructureUpgrade(selectedObjects);
            //foreach (GameObject obj in selectedObjects)
            //{
            //    ObjUpgradeFunc(obj);
            //}
            gameManager.BuildAndSciUiReset();
        }
        selectedObjects = new GameObject[0];
        upgradeItemDic.Clear();
        enoughItemDic.Clear();
        notEnoughItemDic.Clear();
    }

    void GroupUpgradeCost(GameObject obj)   // 업그레이드 가격 측정
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

    BuildingData CanUpgradeCheck(GameObject obj)
    {   // 벨트 스프리터 벽 창고
        if (obj.TryGetComponent(out Structure structure) && structure.canUpgrade && !structure.isPreBuilding)
        {
            buildingData = new BuildingData();
            buildingData = BuildingDataGet.instance.GetBuildingName(structure.buildName, structure.level + 1);
            BuildingData buildUpgradeData = new BuildingData();
            buildUpgradeData = BuildingDataGet.instance.GetBuildingName(structure.buildName, structure.level + 2);
            return buildUpgradeData;
        }

        return null;
    }

    public void ObjUpgradeFunc(GameObject obj) // 인벤토리에서 가격을 확인 후 처리
    {                                   // 이부분을 서버에서 확인 후 처리로 변경
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
                bool hasItem = gameManager.inventory.totalItems.TryGetValue(ItemList.instance.itemDic[UpgradeCost.items[i]], out value);
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
                    gameManager.inventory.Add(ItemList.instance.itemDic[ReturnUpgradeCost.items[i]], ReturnUpgradeCost.amounts[i]);
                    Overall.instance.OverallConsumptionCancel(ItemList.instance.itemDic[ReturnUpgradeCost.items[i]], ReturnUpgradeCost.amounts[i]);
                }

                for (int i = 0; i < UpgradeCost.GetItemCount(); i++)
                {
                    gameManager.inventory.Sub(ItemList.instance.itemDic[UpgradeCost.items[i]], UpgradeCost.amounts[i]);
                    Overall.instance.OverallConsumption(ItemList.instance.itemDic[UpgradeCost.items[i]], UpgradeCost.amounts[i]);
                }
                obj.GetComponent<Structure>().UpgradeFuncServerRpc();
            }
            gameManager.BuildAndSciUiReset();
        }
        else
            return;
    }
}