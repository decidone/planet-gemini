using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Unity.Netcode;

public class PortalUnitIn : PortalObj
{
    public PortalUnitOut portalUnitOut;
    public PortalUnitOut myPortalUnitOut;
    public Vector2[] nearPos = new Vector2[8];

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
            if (sendUnitList.Count > 0 && portalUnitOut != null)
            {
                prodTimer += Time.deltaTime;
                if (prodTimer > cooldown)
                {
                    if (IsServer)
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
        sInvenManager.SetCooldownText(cooldown);
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
            UnitListRemoveServerRpc(0);
        }
    }


    [ServerRpc(RequireOwnership = false)]
    protected override void PortalObjConnectServerRpc()
    {
        base.PortalObjConnectServerRpc();
        if (portalUnitOut != null)
        {
            ulong objID = NetworkObjManager.instance.FindNetObjID(portalUnitOut.gameObject);
            ConnectObjClientRpc(objID);
        }
        if (myPortalUnitOut != null)
        {
            ulong myObjID = NetworkObjManager.instance.FindNetObjID(myPortalUnitOut.gameObject);
            ConnectMyObjClientRpc(myObjID);
        }
    }

    [ServerRpc]
    public override void ConnectObjServerRpc(ulong objId)
    {
        ConnectObjClientRpc(objId);
    }
    [ClientRpc]
    public override void ConnectObjClientRpc(ulong objId)
    {
        portalUnitOut = NetworkObjManager.instance.FindNetworkObj(objId).GetComponent<PortalUnitOut>();
    }
    [ServerRpc]
    public override void ConnectMyObjServerRpc(ulong objId)
    {
        ConnectMyObjClientRpc(objId);
    }

    [ClientRpc]
    public override void ConnectMyObjClientRpc(ulong objId)
    {
        myPortalUnitOut = NetworkObjManager.instance.FindNetworkObj(objId).GetComponent<PortalUnitOut>();
    }
    public override void DestroyLineRenderer()
    {
        base.DestroyLineRenderer();
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
        if (sInvenManager.prod == this && myPortalUnitOut != null && slot.item != null)
        {
            myPortalUnitOut.SpawnUnitCheck(sendUnitList[slot.slotNum]);
            UnitListRemoveServerRpc(slot.slotNum);
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
            UnitListClearServerRpc();
        }
    }

    public override void RemovePortalData()
    {
        base.RemovePortalData();
        WithdrawBtnFunc();
    }

    [ServerRpc]
    public void UnitListAddServerRpc(ulong unitId)
    {
        UnitListAddClientRpc(unitId);
    }

    [ClientRpc]
    public void UnitListAddClientRpc(ulong unitId)
    {
        GameObject unit = NetworkObjManager.instance.FindNetworkObj(unitId).gameObject;
        sendUnitList.Add(unit);
        DisplaySlotChange();
    }

    [ServerRpc(RequireOwnership = false)]
    public void UnitListRemoveServerRpc(int slotNum)
    {
        UnitListRemoveClientRpc(slotNum);
    }

    [ClientRpc]
    public void UnitListRemoveClientRpc(int slotNum)
    {
        sendUnitList.RemoveAt(slotNum);
        DisplaySlotChange();
    }

    [ServerRpc(RequireOwnership = false)]
    public void UnitListClearServerRpc()
    {
        UnitListClearClientRpc();
    }

    [ClientRpc]
    public void UnitListClearClientRpc()
    {
        sendUnitList.Clear();
        DisplaySlotChange();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsServer)
            return;

        if (collision.collider.TryGetComponent(out UnitAi unitAi) && !sendUnitList.Contains(collision.gameObject) && sendUnitList.Count < 18)
        {
            var objID = NetworkObjManager.instance.FindNetObjID(collision.gameObject);
            UnitListAddServerRpc(objID);
            unitAi.PortalUnitInFuncServerRpc();
        }
    }
}
