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
        // �̰� �޼���� ���� ��� �� ��
        if (this.transform.GetComponent<Miner>())
        {
            Miner miner = this.transform.GetComponent<Miner>();
            miner.OpenUI();
        }
        else if (this.transform.GetComponent<Furnace>())
        {
            Furnace furnace = this.transform.GetComponent<Furnace>();
            furnace.OpenUI();
        }

        sInvenManager.OpenUI();
    }

    public void CloseUI()
    {
        sInvenManager.CloseUI();
    }
}
