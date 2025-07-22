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
    public Production prod;
    DragGraphic drag;

    SoundManager soundManager;

    public bool openUI;

    private void Start()
    {
        soundManager = SoundManager.instance;
    }

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
                closeBtn.onClick.RemoveAllListeners();
                closeBtn.onClick.AddListener(CloseUI);
            }
        }
        sInvenManager = canvas.GetComponent<StructureInvenManager>();
    }

    public void OpenUI()
    {
        openUI = true;
        prod.OpenUI();
        if(prod.isGetLine)
            drag.SelectBuild(this.gameObject);
        gameManager.SelectPointSpawn(prod.gameObject);
        sInvenManager.OpenUI();
        soundManager.PlayUISFX("SidebarClick");
    }

    public void CloseUI()
    {
        openUI = false;
        prod.CloseUI();
        if (prod.isGetLine)
            drag.CancelBuild();
        gameManager.SelectPointRemove();
        sInvenManager.CloseUI();
        soundManager.PlayUISFX("CloseUI");
    }

    public void CloseUINoSound()
    {
        openUI = false;
        prod.CloseUI();
        if (prod.isGetLine)
            drag.CancelBuild();
        gameManager.SelectPointRemove();
        sInvenManager.CloseUI();
    }
}
