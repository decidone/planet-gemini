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

    public void ConfirmEnd(bool isOk)
    {
        if (isOk)
        {
            foreach (GameObject obj in selectedObjects)
            {
                if (obj.TryGetComponent(out Structure structure))
                {
                    structure.DestroyServerRpc();
                }
            }
        }
        selectedObjects = new GameObject[0];
    }
}
