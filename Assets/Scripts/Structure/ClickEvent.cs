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

    Miner miner;
    Furnace furnace;

    void Start()
    {
        closeBtn = structureInfoUI.transform.Find("CloseButton").gameObject.GetComponent<Button>();
        closeBtn.onClick.AddListener(CloseUI);

        miner = this.transform.GetComponent<Miner>();
        furnace = this.transform.GetComponent<Furnace>();
    }

    public void OpenUI()
    {
        if (miner) miner.OpenUI();
        else if (furnace) furnace.OpenUI();

        sInvenManager.OpenUI();
    }

    public void CloseUI()
    {
        if (miner) miner.CloseUI();
        else if (furnace) furnace.CloseUI();

        sInvenManager.CloseUI();
    }
}
