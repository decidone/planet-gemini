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
        GroupSelectedObjects(startPos, endPos, 0);
    }

    protected override void GroupSelectedObjects(Vector2 startPosition, Vector2 endPosition, int layer)
    {
        Collider2D[] colliders = Physics2D.OverlapAreaAll(startPosition, endPosition, (1 << LayerMask.NameToLayer("Obj")) | (1 << LayerMask.NameToLayer("LocalPortal")));

        List<GameObject> selectedObjectsList = new List<GameObject>();

        foreach (Collider2D collider in colliders)
        {
            if (collider.GetComponent<Portal>() || collider.GetComponent<ScienceBuilding>())
                continue;
            selectedObjectsList.Add(collider.gameObject);
        }

        selectedObjects = selectedObjectsList.ToArray();

        if (selectedObjects.Length > 0) 
            gameManager.inventoryUiCanvas.GetComponent<PopUpManager>().removeConfirm.OpenUI();
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
