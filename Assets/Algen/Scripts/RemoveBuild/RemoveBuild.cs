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

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1) && Input.GetKey(KeyCode.LeftControl))
        {
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero);
            if (hit.collider != null && hit.collider.TryGetComponent(out Structure structure))
            {
                structure.RemoveObj();
                refundCost(structure);
            }
        }
    }

    void refundCost(Structure obj)
    {
        buildingData = new BuildingData();
        buildingData = BuildingDataGet.instance.GetBuildingName(obj.buildName, obj.level + 1);
        for (int i = 0; i < buildingData.GetItemCount(); i++)
        {
            Debug.Log(buildingData.items[i] + " : "+ buildingData.amounts[i]);
        }
    }
}
