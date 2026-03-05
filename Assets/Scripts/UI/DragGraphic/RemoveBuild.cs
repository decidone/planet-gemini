using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
public class RemoveBuild : DragFunc
{
    protected GameObject canvas;
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

        List<WorldObj> selectedObjectsList = new List<WorldObj>();

        foreach (Collider2D collider in colliders)
        {
            WorldObj worldObj = collider.GetComponentInParent<WorldObj>();
            if (!worldObj)
                continue;

            var structure = worldObj.Get<Structure>();
            var portal = worldObj.Get<Portal>();
            var portalObj = worldObj.Get<PortalObj>();
            var scienceBuilding = worldObj.Get<ScienceBuilding>();

            // Structure가 없으면 제외
            if (!structure)
                continue;
            else if (portal)
                continue;
            else if (scienceBuilding)
                continue;

            selectedObjectsList.Add(structure);
        }

        selectedObjects = selectedObjectsList.ToArray();

        if (selectedObjects.Length > 0) 
            gameManager.inventoryUiCanvas.GetComponent<PopUpManager>().removeConfirm.OpenUI();
    }

    void RemoveClick(Vector2 mousePos)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);
        selectedObjects = new WorldObj[1];
        if (hits.Length > 0)
        {
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider.TryGetComponent(out Structure structure) && !structure.isPreBuilding)
                {
                    if (!(structure.Get<Portal>() || structure.Get<ScienceBuilding>()))
                    {
                        selectedObjects[0] = structure;
                        gameManager.inventoryUiCanvas.GetComponent<PopUpManager>().removeConfirm.OpenUI();
                    }
                }
            }
        }
    }

    public void RemoveBtnClicked(Structure str)
    {
        if (!(str.Get<Portal>() || str.Get<ScienceBuilding>()))
        {
            selectedObjects = new WorldObj[1];
            selectedObjects[0] = str;
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
                    selectedObjects[i].TryGet(out Structure structure);
                    structure.DestroyServerRpc();
                }
            }
        }
        selectedObjects = new WorldObj[0];
    }
}
