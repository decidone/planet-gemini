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
        if (startPos != endPos)
            GroupSelectedObjects(startPos, endPos, structureLayer);
        else
            RemoveClick(startPos);
    }

    protected override void GroupSelectedObjects(Vector2 startPosition, Vector2 endPosition, int layer)
    {
        base.GroupSelectedObjects(startPosition, endPosition, layer);

        if(selectedObjects.Length > 1)
            gameManager.inventoryUiCanvas.GetComponent<PopUpManager>().removeConfirm.OpenUI();
        else if(selectedObjects.Length == 1 && !isRemovePopUpOn)
            ObjRemoveFunc(selectedObjects[0]);
    }

    void RemoveClick(Vector2 mousePos)
    {
        if (!isRemovePopUpOn)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);
            if (hits.Length > 0)
            {
                foreach (RaycastHit2D hit in hits)
                {
                    ObjRemoveFunc(hit.collider.gameObject);
                }
            }
            gameManager.BuildAndSciUiReset();
        }
    }

    void ObjRemoveFunc(GameObject obj)
    {
        if (obj.TryGetComponent(out Structure structure) && !structure.isPreBuilding)
        {
            //UiCheck(structure);
            structure.AddInvenItem();
            structure.RemoveObjServerRpc();
            RefundCost(structure);
        }
    }

    public void ConfirmEnd(bool isOk)
    {
        if (isOk)
        {
            foreach (GameObject obj in selectedObjects)
            {
                ObjRemoveFunc(obj);
            }
            gameManager.BuildAndSciUiReset();
            soundManager.PlayUISFX("BuildingRemove");
        }
        selectedObjects = new GameObject[0];
    }

    void RefundCost(Structure obj)
    {
        if (obj.isTempBuild)
        {
            gameManager.playerController.RemoveTempBuild();
        }
        else if (obj.isPortalBuild)
        {
            return;
        }
        else
        {
            buildingData = new BuildingData();
            buildingData = BuildingDataGet.instance.GetBuildingName(obj.buildName, obj.level + 1);
            if(obj.GetComponent<Structure>().isInHostMap)
                inventory = gameManager.hostMapInven;
            else
                inventory = gameManager.clientMapInven;

            for (int i = 0; i < buildingData.GetItemCount(); i++)
            {
                inventory.Add(ItemList.instance.itemDic[buildingData.items[i]], buildingData.amounts[i]);
            }
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
            if(obj.TryGetComponent(out Transporter trBuild))
            {
                if (trBuild.lineRenderer != null)
                    Destroy(trBuild.lineRenderer);
            }
            else if (obj.TryGetComponent(out UnitFactory unitFactory))
            {
                if (unitFactory.lineRenderer != null)
                    Destroy(unitFactory.lineRenderer);
            }
        }
    }
}
