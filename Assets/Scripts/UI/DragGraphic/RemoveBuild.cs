using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
public class RemoveBuild : DragFunc
{
    protected GameObject canvas;
    BuildingData buildingData;
    Inventory inventory;
    int structureLayer;
    public bool isRemovePopUpOn = false;

    protected override void Start()
    {
        base.Start();
        canvas = gameManager.GetComponent<GameManager>().inventoryUiCanvas;
        structureLayer = LayerMask.NameToLayer("Obj");
    }

    public override void LeftMouseUp(Vector2 startPos, Vector2 endPos)
    {
        GroupSelectedObjects(startPos, endPos, structureLayer);
    }

    protected override void GroupSelectedObjects(Vector2 startPosition, Vector2 endPosition, int layer)
    {
        base.GroupSelectedObjects(startPosition, endPosition, layer);
        if (selectedObjects.Length > 0) 
            gameManager.inventoryUiCanvas.GetComponent<PopUpManager>().removeConfirm.OpenUI();
    }

    //void ObjRemoveFunc(GameObject obj)
    //{
    //    if (obj.TryGetComponent(out Structure structure) && !structure.isPreBuilding)
    //    {
    //        structure.AddInvenItem();
    //        structure.RemoveObjServerRpc();
    //        RefundCost(structure);
    //    }
    //}

    //public void ConfirmEnd(bool isOk)
    //{
    //    if (isOk)
    //    {
    //        foreach (GameObject obj in selectedObjects)
    //        {
    //            ObjRemoveFunc(obj);
    //        }
    //        gameManager.BuildAndSciUiReset();
    //        soundManager.PlayUISFX("BuildingRemove");
    //    }
    //    selectedObjects = new GameObject[0];
    //}

    public void ConfirmEnd(bool isOk)
    {
        if (isOk)
        {
            foreach (GameObject obj in selectedObjects)
            {
                if (obj.TryGetComponent(out Structure structure))
                {
                    structure.DestroyStart();
                }
            }
        }
        selectedObjects = new GameObject[0];
    }

    //void RefundCost(Structure obj)
    //{
    //    if (obj.isPortalBuild)
    //    {
    //        return;
    //    }
    //    else
    //    {
    //        buildingData = new BuildingData();
    //        buildingData = BuildingDataGet.instance.GetBuildingName(obj.buildName, obj.level + 1);
    //        if(obj.GetComponent<Structure>().isInHostMap)
    //            inventory = gameManager.hostMapInven;
    //        else
    //            inventory = gameManager.clientMapInven;

    //        for (int i = 0; i < buildingData.GetItemCount(); i++)
    //        {
    //            inventory.Add(ItemList.instance.itemDic[buildingData.items[i]], buildingData.amounts[i]);
    //            Overall.instance.OverallConsumptionCancel(ItemList.instance.itemDic[buildingData.items[i]], buildingData.amounts[i]);
    //        }
    //    }
    //}
}
