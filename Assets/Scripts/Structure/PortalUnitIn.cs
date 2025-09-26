using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Unity.Netcode;
using System.Security.Cryptography;

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
            slot.SetInputItem(itemLists.FindData("SpinRobot2"));
            slot.SetInputItem(itemLists.FindData("SentryCopter"));
            slot.SetInputItem(itemLists.FindData("SentryCopter2"));
            slot.SetInputItem(itemLists.FindData("BounceRobot"));
            slot.SetInputItem(itemLists.FindData("BounceRobot2"));
            slot.SetInputItem(itemLists.FindData("CorrosionDrone"));
            slot.SetInputItem(itemLists.FindData("RepairerDrone"));
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
        base.OpenUI();
        sInvenManager.SetInven(inventory, ui);
        sInvenManager.SetProd(this);
        sInvenManager.progressBar.SetMaxProgress(cooldown);
        sInvenManager.SetCooldownText(cooldown);
        DisplaySlotChange();
    }

    public override void CloseUI()
    {
        base.CloseUI();
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
            ConnectObjClientRpc(portalUnitOut.NetworkObject);
        }
        if (myPortalUnitOut != null)
        {
            ConnectMyObjClientRpc(myPortalUnitOut.NetworkObject);
        }
    }

    [ServerRpc]
    public override void ConnectObjServerRpc(NetworkObjectReference networkObjectReference)
    {
        ConnectObjClientRpc(networkObjectReference);
    }

    [ClientRpc]
    public override void ConnectObjClientRpc(NetworkObjectReference networkObjectReference)
    {
        networkObjectReference.TryGet(out NetworkObject networkObject);
        portalUnitOut = networkObject.GetComponent<PortalUnitOut>();
    }

    [ServerRpc]
    public override void ConnectMyObjServerRpc(NetworkObjectReference networkObjectReference)
    {
        ConnectMyObjClientRpc(networkObjectReference);
    }

    [ClientRpc]
    public override void ConnectMyObjClientRpc(NetworkObjectReference networkObjectReference)
    {
        networkObjectReference.TryGet(out NetworkObject networkObject);
        myPortalUnitOut = networkObject.GetComponent<PortalUnitOut>();
    }

    public override void DestroyLineRenderer()
    {
        base.DestroyLineRenderer();
    }

    [ServerRpc(RequireOwnership = false)]
    public override void ItemSyncServerRpc()
    {
        for (int i = 0; i < displaySlots.Length; i++)
        {
            if (sendUnitList.Count > i)
            {
                int itemIndex = -1;
                itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(displaySlots[i].item);
                ItemSyncClientRpc(i, itemIndex, 1, sendUnitList[i].GetComponent<NetworkObject>());
            }
            else
                break;
        }
    }

    [ClientRpc]
    protected void ItemSyncClientRpc(int slotNum, int itemIndex, int itemAmount, NetworkObjectReference networkObjectReference, ClientRpcParams rpcParams = default)
    {
        if (IsServer)
            return;
        networkObjectReference.TryGet(out NetworkObject networkObject);
        GameObject unit = networkObject.gameObject;
        sendUnitList.Add(unit);

        DisplaySlotChange();
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
                    sendUnitList[i].TryGetComponent(out UnitAi unit);
                    string objName = unit.unitName;
                    displaySlots[i].AddItem(itemLists.FindDataGetLevel(objName, unit.unitLevel + 1), 1);
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

        for (int i = 0; i < sendUnitList.Count; i++)
        {
            SpawnUnitCheck(sendUnitList[i]);
        }
        UnitListClearServerRpc();
    }

    public void SpawnUnitCheck(GameObject unit)
    {
        if (IsServer)
        {
            unit.TryGetComponent(out NetworkObject netObj);
            if (!netObj.IsSpawned) unit.GetComponent<NetworkObject>().Spawn(true);
        }

        UnitAi unitAi = unit.GetComponent<UnitAi>();
        unitAi.PortalUnitOutFuncServerRpc(isInHostMap, transform.position);
        unitAi.MovePosSetServerRpc(transform.position, 0, true);
    }

    [ServerRpc]
    public void UnitListAddServerRpc(NetworkObjectReference networkObjectReference)
    {
        UnitListAddClientRpc(networkObjectReference);
    }

    [ClientRpc]
    public void UnitListAddClientRpc(NetworkObjectReference networkObjectReference)
    {
        networkObjectReference.TryGet(out NetworkObject networkObject);
        sendUnitList.Add(networkObject.gameObject);
        DisplaySlotChange();
    }


    //[ServerRpc]
    //public void UnitListAddServerRpc(ulong unitId)
    //{
    //    UnitListAddClientRpc(unitId);
    //}

    //[ClientRpc]
    //public void UnitListAddClientRpc(ulong unitId)
    //{
    //    GameObject unit = NetworkObjManager.instance.FindNetworkObj(unitId).gameObject;
    //    sendUnitList.Add(unit);
    //    DisplaySlotChange();
    //}

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

    public void LoadUnitData(GameObject unit)
    {
        sendUnitList.Add(unit);
        unit.TryGetComponent(out UnitAi unitAi);
        unitAi.transform.position = new Vector3(-100, -100, 0);
        unitAi.PortalUnitInFuncServerRpc(isInHostMap);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsServer)
            return;

        if (collision.collider.TryGetComponent(out UnitAi unitAi) && unitAi.playerUnitPortalIn && !sendUnitList.Contains(collision.gameObject) && sendUnitList.Count < 18)
        {
            UnitListAddServerRpc(collision.gameObject.GetComponent<NetworkObject>());
            unitAi.transform.position = new Vector3(-100, -100, 0);

            unitAi.PortalUnitInFuncServerRpc(isInHostMap);
        }
    }
}
