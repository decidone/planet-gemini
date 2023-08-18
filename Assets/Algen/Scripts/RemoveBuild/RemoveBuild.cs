using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveBuild : MonoBehaviour
{
    protected GameObject canvas;
    BuildingData buildingData;
    Inventory inventory;

    void Start()
    {
        GameManager gameManager = GameManager.instance;
        canvas = gameManager.GetComponent<GameManager>().inventoryUiCanvas;
        inventory = gameManager.GetComponent<Inventory>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1) && Input.GetKey(KeyCode.LeftControl))
        {
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D[] hits = Physics2D.RaycastAll(pos, Vector2.zero);
            if (hits.Length > 0)
            {
                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.collider.TryGetComponent(out Structure structure))
                    {
                        UiCheck(structure);
                        structure.RemoveObj();
                        refundCost(structure);
                    }
                }
            }
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
