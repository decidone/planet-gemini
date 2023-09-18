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
            RemoveClick(startPos);
    }

    protected override List<GameObject> GroupSelectedObjects(Vector2 startPosition, Vector2 endPosition, int layer)
    {
        List<GameObject> List = base.GroupSelectedObjects(startPosition, endPosition, layer);
        selectedObjects = List.ToArray();

        foreach (GameObject obj in selectedObjects)
        {
            ObjRemoveFunc(obj);
        }
        GameManager.instance.BuildAndSciUiReset();

        return null;
    }

    void RemoveClick(Vector2 mousePos)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);
        if (hits.Length > 0)
        {
            foreach (RaycastHit2D hit in hits)
            {
                ObjRemoveFunc(hit.collider.gameObject);
            }
        }
        GameManager.instance.BuildAndSciUiReset();
    }

    void ObjRemoveFunc(GameObject obj)
    {
        if (obj.TryGetComponent(out Structure structure) && !structure.isPreBuilding)
        {
            UiCheck(structure);
            structure.RemoveObj();
            refundCost(structure);
        }
    }

    void refundCost(Structure obj)
    {
        buildingData = new BuildingData();
        buildingData = BuildingDataGet.instance.GetBuildingName(obj.buildName, obj.level + 1);
        for (int i = 0; i < buildingData.GetItemCount(); i++)
        {
            inventory.Add(ItemList.instance.itemDic[buildingData.items[i]], buildingData.amounts[i]);
        }
    }

    void UiCheck(Structure obj)
    {
        if(obj.TryGetComponent(out LogisticsClickEvent solidFacClickEvent))
        {
            if(solidFacClickEvent.LogisticsUI != null)
            {
                if (solidFacClickEvent.LogisticsUI.activeSelf)
                {
                    if(solidFacClickEvent.sFilterManager != null)
                        solidFacClickEvent.sFilterManager.CloseUI();
                    else if (solidFacClickEvent.itemSpManager != null)
                        solidFacClickEvent.itemSpManager.CloseUI();
                }
            }
        }
        else if (obj.TryGetComponent(out StructureClickEvent structureClickEvent))
        {
            if (structureClickEvent.structureInfoUI != null)
            {
                if (structureClickEvent.structureInfoUI.activeSelf)
                {
                    if (structureClickEvent.sInvenManager != null)
                    structureClickEvent.sInvenManager.CloseUI();
                }
            }
        }
    }
}
