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
    Constructor constructor;
    Assembler assembler;

    void Start()
    {
        closeBtn = structureInfoUI.transform.Find("CloseButton").gameObject.GetComponent<Button>();
        closeBtn.onClick.AddListener(CloseUI);

        miner = this.transform.GetComponent<Miner>();
        furnace = this.transform.GetComponent<Furnace>();
        constructor = this.transform.GetComponent<Constructor>();
        assembler = this.transform.GetComponent<Assembler>();
    }

    public void OpenUI()
    {
        if (miner) miner.OpenUI();
        else if (furnace) furnace.OpenUI();
        else if (constructor) constructor.OpenUI();
        else if (assembler) assembler.OpenUI();

        sInvenManager.OpenUI();
    }

    public void CloseUI()
    {
        if (miner) miner.CloseUI();
        else if (furnace) furnace.CloseUI();
        else if (constructor) constructor.CloseUI();
        else if (assembler) assembler.CloseUI();

        sInvenManager.CloseUI();
    }
}
