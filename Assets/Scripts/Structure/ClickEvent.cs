using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClickEvent : MonoBehaviour
{
    [SerializeField]
    GameObject structureInfoUI;
    [SerializeField]
    StructureInvenManager sInvenManager;
    Button closeBtn;

    void Start()
    {
        closeBtn = structureInfoUI.transform.Find("CloseButton").gameObject.GetComponent<Button>();
        closeBtn.onClick.AddListener(CloseUI);
    }

    public void OpenUI()
    {
        Miner miner = this.transform.GetComponent<Miner>();
        Furnace furnace = this.transform.GetComponent<Furnace>();

        if (miner != null)
        {
            miner.OpenUI();
        }
        else if (furnace != null)
        {
            furnace.OpenUI();
        }
        sInvenManager.OpenUI();
    }

    public void CloseUI()
    {
        sInvenManager.CloseUI();
    }
}
