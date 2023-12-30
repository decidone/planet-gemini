using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PortalUnitIn : PortalObj
{
    PortalUnitOut portalUnitOut;
    PortalUnitOut myPortalUnitOut;
    public Vector2[] nearPos = new Vector2[8];
    bool isSetPos;

    public List<GameObject> sendUnitList = new List<GameObject>();

    Slot[] displaySlots;
    Button withdrawBtn;
    ItemList itemLists;

    protected override void Start()
    {
        base.Start();
        isPortalBuild = true;
        displaySlots = GameObject.Find("Canvas").transform.Find("StructureInfo").transform.Find("Storage")
            .transform.Find("PortalUnit").transform.Find("DisplaySlots").GetComponentsInChildren<Slot>();
        for (int i = 0; i < displaySlots.Length; i++)
        {
            Slot slot = displaySlots[i];
            slot.slotNum = i;

            slot.amountText.gameObject.SetActive(false);
            slot.GetComponentInChildren<Button>().onClick.AddListener(() => UnitWithdraw(slot));
        }
        withdrawBtn = GameObject.Find("Canvas").transform.Find("StructureInfo").transform.Find("Storage")
            .transform.Find("PortalUnit").transform.Find("Button (Legacy)").GetComponent<Button>();
        withdrawBtn.onClick.AddListener(() => WithdrawBtnFunc());
        maxFuel = 100;
        itemLists = ItemList.instance;
        foreach (Slot slot in displaySlots)
        {
            slot.SetInputItem(itemLists.FindData("SpinRobot"));
            slot.SetInputItem(itemLists.FindData("SentryCopter"));
            slot.SetInputItem(itemLists.FindData("BounceRobot"));
        }
        isStorageBuilding = true;
    }

    protected override void Update()
    {
        base.Update();
        if (!isPreBuilding)
        {
            if (portalUnitOut != null)
            {
                prodTimer += Time.deltaTime;
                if (prodTimer > cooldown)
                {
                    SendUnitCheck(portalUnitOut);
                    prodTimer = 0;
                }
            }
            else
                prodTimer = 0;
        }
    }

    public override void OpenUI()
    {
        sInvenManager.SetInven(inventory, ui);
        sInvenManager.SetProd(this);
        sInvenManager.progressBar.SetMaxProgress(cooldown);
        DisplaySlotChange();
    }

    public override void CloseUI()
    {
        sInvenManager.ReleaseInven();
    }

    public override void GetUIFunc()
    {
        InventoryList inventoryList = canvas.GetComponent<InventoryList>();

        foreach (GameObject list in inventoryList.StructureStorageArr)
        {
            if (list.name == "PortalUnit")
            {
                ui = list;
            }
        }
    }

    void SendUnitCheck(PortalUnitOut portalUnitOut)
    {
        if(sendUnitList.Count > 0)
        {
            portalUnitOut.SpawnUnitCheck(sendUnitList[0]);
            sendUnitList.RemoveAt(0);
            DisplaySlotChange();
        }
    }

    public override void ConnectObj(GameObject othObj)
    {
        portalUnitOut = othObj.GetComponent<PortalUnitOut>();
    }

    public override void ConnectMyObj(GameObject myObj)
    {
        myPortalUnitOut = myObj.GetComponent<PortalUnitOut>();
    }

    public override void DestroyLineRenderer()
    {
        base.DestroyLineRenderer();
        isSetPos = false;
    }

    void DisplaySlotChange()
    {
        if (sInvenManager.prod == this)
        {
            for (int i = 0; i < displaySlots.Length; i++)
            {
                displaySlots[i].ClearSlot();
                if (sendUnitList.Count > i)
                {
                    string objName = sendUnitList[i].GetComponent<UnitAi>().unitName;
                    displaySlots[i].AddItem(itemLists.FindData(objName), 1);
                }
            }
        }
    }

    void UnitWithdraw(Slot slot)
    {
        if (sInvenManager.prod == this && myPortalUnitOut != null)
        {
            myPortalUnitOut.SpawnUnitCheck(sendUnitList[slot.slotNum]);
            sendUnitList.RemoveAt(slot.slotNum);
            DisplaySlotChange();
        }
    }

    void WithdrawBtnFunc()
    {
        if (sInvenManager.prod == this && myPortalUnitOut != null)
        {
            for (int i = 0; i < sendUnitList.Count; i++)
            {
                myPortalUnitOut.SpawnUnitCheck(sendUnitList[i]);
            }
            sendUnitList.Clear();
            DisplaySlotChange();
        }
    }

    public override void RemovePortalData()
    {
        base.RemovePortalData();
        WithdrawBtnFunc();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.TryGetComponent(out UnitAi unitAi) && !sendUnitList.Contains(collision.gameObject) && sendUnitList.Count < 18)
        {
            sendUnitList.Add(collision.gameObject);
            unitAi.UnitSelImg(false);
            UnitGroupCtrl unitGroup = GameManager.instance.gameObject.GetComponent<UnitGroupCtrl>();
            unitGroup.DieUnitCheck(collision.gameObject);
            DisplaySlotChange();
            collision.gameObject.SetActive(false);
        }
    }
}
