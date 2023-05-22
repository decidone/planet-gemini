using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SplitterClickEvent : MonoBehaviour
{
    [SerializeField]
    GameObject spilterInfoUI;
    [SerializeField]
    SplitterFilterManager sFilterManager;
    Button closeBtn;
    [SerializeField]
    GameObject gameManager;
    // Start is called before the first frame update

    void Start()
    {
        gameManager = GameObject.Find("GameManager");

        GameObject canvas = gameManager.GetComponent<GameManager>().inventoryUiCanvas;
        InventoryList inventoryList = canvas.GetComponent<InventoryList>();
        foreach (GameObject list in inventoryList.InventoryArr)
        {
            if (list.name == "SplitterMenu")
                spilterInfoUI = list;
        }

        sFilterManager = canvas.GetComponent<SplitterFilterManager>();
        closeBtn = spilterInfoUI.transform.Find("CloseButton").gameObject.GetComponent<Button>();
        closeBtn.onClick.AddListener(CloseUI);
    }
    public void OpenUI()
    {
        sFilterManager.splitter = this.gameObject.GetComponent<SplitterCtrl>();
        sFilterManager.OpenUI();
    }

    public void CloseUI()
    {
        sFilterManager.splitter = null;
        sFilterManager.CloseUI();
    }
}
