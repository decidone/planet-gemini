using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// UTF-8 설정
public class StructureClickEvent : MonoBehaviour
{
    public GameObject structureInfoUI;
    public StructureInvenManager sInvenManager;
    GameManager gameManager;
    Button closeBtn;
    Production prod;
    DragGraphic drag;

    public void StructureClick()
    {
        gameManager = GameManager.instance;
        GameObject canvas = gameManager.GetComponent<GameManager>().inventoryUiCanvas;
        InventoryList inventoryList = canvas.GetComponent<InventoryList>();
        prod = this.transform.GetComponent<Production>();
        drag = DragGraphic.instance;

        foreach (GameObject list in inventoryList.InventoryArr)
        {
            if (list.name == "StructureInfo")
            {
                structureInfoUI = list;
                closeBtn = structureInfoUI.transform.Find("CloseButton").gameObject.GetComponent<Button>();
                closeBtn.onClick.AddListener(CloseUI);
            }
        }
        sInvenManager = canvas.GetComponent<StructureInvenManager>();
    }

    public void OpenUI()
    {
        prod.OpenUI();
        if(prod.isGetLine)
            drag.SelectBuild(this.gameObject);
        sInvenManager.OpenUI();
    }

    public void CloseUI()
    {
        prod.CloseUI();
        if (prod.isGetLine)
            drag.cancelBuild();
        sInvenManager.CloseUI();
    }
}
