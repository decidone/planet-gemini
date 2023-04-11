using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StructureClickEvent : MonoBehaviour
{
    [SerializeField]
    GameObject structureInfoUI;
    [SerializeField]
    StructureInvenManager sInvenManager;
    Button closeBtn;
    Production prod;

    void Start()
    {
        closeBtn = structureInfoUI.transform.Find("CloseButton").gameObject.GetComponent<Button>();
        closeBtn.onClick.AddListener(CloseUI);

        prod = this.transform.GetComponent<Production>();
    }

    public void OpenUI()
    {
        prod.OpenUI();
        sInvenManager.OpenUI();
    }

    public void CloseUI()
    {
        prod.CloseUI();
        sInvenManager.CloseUI();
    }
}
