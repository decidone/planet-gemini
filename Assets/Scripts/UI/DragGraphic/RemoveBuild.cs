using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
public class RemoveBuild : DragFunc
{
    protected GameObject canvas;
    BuildingData buildingData;
    Inventory inventory;
    public bool isRemovePopUpOn = false;

    protected override void Start()
    {
        base.Start();
        canvas = gameManager.GetComponent<GameManager>().inventoryUiCanvas;
    }

    public override void LeftMouseUp(Vector2 startPos, Vector2 endPos)
    {
        GroupSelectedObjects(startPos, endPos);

        if (startPos != endPos)
            GroupSelectedObjects(startPos, endPos);
        else
            RemoveClick(startPos);
    }

    protected override void GroupSelectedObjects(Vector2 startPosition, Vector2 endPosition)
    {
        Collider2D[] colliders = Physics2D.OverlapAreaAll(startPosition, endPosition, 1 << interactLayer);
        //Collider2D[] colliders = Physics2D.OverlapAreaAll(startPosition, endPosition, (1 << LayerMask.NameToLayer("Obj")) | (1 << LayerMask.NameToLayer("LocalPortal")));

        List<GameObject> selectedObjectsList = new List<GameObject>();

        foreach (Collider2D collider in colliders)
        {
            var structure = collider.GetComponentInParent<Structure>();
            var portal = collider.GetComponentInParent<Portal>();
            var portalObj = collider.GetComponentInParent<PortalObj>();
            var scienceBuilding = collider.GetComponentInParent<ScienceBuilding>();

            // Structure가 없으면 제외
            if (structure == null)
                continue;

            // Portal이 있는 경우, PortalObj가 없거나 ScienceBuilding이 있으면 제외
            if (portal != null && (portalObj == null || scienceBuilding != null))
                continue;

            selectedObjectsList.Add(structure.gameObject);
        }

        selectedObjects = selectedObjectsList.ToArray();

        if (selectedObjects.Length > 0) 
            gameManager.inventoryUiCanvas.GetComponent<PopUpManager>().removeConfirm.OpenUI();
    }

    void RemoveClick(Vector2 mousePos)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);
        selectedObjects = new GameObject[1];
        if (hits.Length > 0)
        {
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider.TryGetComponent(out Structure structure) && !structure.isPreBuilding)
                {
                    if (!(structure.GetComponent<Portal>() || structure.GetComponent<ScienceBuilding>()))
                    {
                        selectedObjects[0] = hit.collider.gameObject;
                        gameManager.inventoryUiCanvas.GetComponent<PopUpManager>().removeConfirm.OpenUI();
                    }
                }
            }
        }
    }

    public void RemoveBtnClicked(Structure str)
    {
        if (!(str.GetComponent<Portal>() || str.GetComponent<ScienceBuilding>()))
        {
            selectedObjects = new GameObject[1];
            selectedObjects[0] = str.gameObject;
            gameManager.inventoryUiCanvas.GetComponent<PopUpManager>().removeConfirm.OpenUI();
        }
    }

    public void ConfirmEnd(bool isOk)
    {
        if (isOk)
        {
            for(int i = 0;  i < selectedObjects.Length; i++)
            {
                if (selectedObjects[i] != null)
                {
                    selectedObjects[i].TryGetComponent(out Structure structure);
                    structure.DestroyServerRpc();
                }
            }
        }
        selectedObjects = new GameObject[0];
    }
}
