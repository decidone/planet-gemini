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
        displaySlots = canvas.transform.Find("StructureInfo").transform.Find("Storage")
            .transform.Find("PortalUnit").transform.Find("DisplaySlots").GetComponentsInChildren<Slot>();
        for (int i = 0; i < displaySlots.Length; i++)
        {
            Slot slot = displaySlots[i];
            slot.slotNum = i;

            slot.amountText.gameObject.SetActive(false);
            slot.GetComponentInChildren<Button>().onClick.AddListener(() => UnitWithdraw(slot));
        }
        withdrawBtn = canvas.transform.Find("StructureInfo").transform.Find("Storage")
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

    protected override void PortalObjConnectServer()
    {
        //base.PortalObjConnectServerRpc();
        PortalObjConnectClientRpc(transform.position);

        if (portalUnitOut != null)
        {
            ConnectObjClientRpc(portalUnitOut.NetworkObject);
        }
        if (myPortalUnitOut != null)
        {
            ConnectMyObjClientRpc(myPortalUnitOut.NetworkObject);
        }
    }

    public override void ConnectObj(NetworkObjectReference networkObjectReference)
    {
        networkObjectReference.TryGet(out NetworkObject networkObject);
        portalUnitOut = networkObject.GetComponent<PortalUnitOut>();
    }

    public override void ConnectMyObj(NetworkObjectReference networkObjectReference)
    {
        networkObjectReference.TryGet(out NetworkObject networkObject);
        myPortalUnitOut = networkObject.GetComponent<PortalUnitOut>();
    }

    public override void DestroyLineRenderer()
    {
        base.DestroyLineRenderer();
    }

    public override void OnClientConnectedCallback()
    {
        base.OnClientConnectedCallback();

        if (IsServer)
            UnitSync();
    }

    public void UnitSync()
    {
        int[] itemindexs = new int[displaySlots.Length];
        NetworkObjectReference[] sendUnit = new NetworkObjectReference[displaySlots.Length];
        for (int i = 0; i < displaySlots.Length; i++)
        {
            if (sendUnitList.Count > i)
            {
                int itemIndex = -1;
                itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(displaySlots[i].item);

                itemindexs[i] = itemIndex;
                sendUnit[i] = sendUnitList[i].GetComponent<NetworkObject>();
            }
            else
                break;
        }

        UnitSyncClientRpc(itemindexs, sendUnit);
    }

    //[ClientRpc]
    //protected void UnitSyncClientRpc(int[] itemIndex, NetworkObjectReference[] networkObjectReference, ClientRpcParams rpcParams = default)
    //{
    //    if (IsServer)
    //        return;

    //    for (int i = 0; i < itemIndex.Length; i++)
    //    {
    //        if (itemIndex[i] != -1)
    //        {
    //            networkObjectReference[i].TryGet(out NetworkObject networkObject);
    //            GameObject unit = networkObject.gameObject;
    //            sendUnitList.Add(unit);
    //        }
    //    }

    //    DisplaySlotChange();
    //}

    [ClientRpc]
    protected void UnitSyncClientRpc(int[] itemIndex, NetworkObjectReference[] networkObjectReference, ClientRpcParams rpcParams = default)
    {
        if (IsServer) return;

        StartCoroutine(WaitForSyncComplete(itemIndex, networkObjectReference));
    }

    IEnumerator WaitForSyncComplete(int[] itemIndex, NetworkObjectReference[] unitRefs)
    {
        // 클라이언트 동기화 완료까지 대기
        yield return new WaitUntil(() => NetworkObjManager.instance.clientSyncComplete);

        sendUnitList.Clear();
        for (int i = 0; i < itemIndex.Length; i++)
        {
            if (itemIndex[i] == -1) continue;

            if (unitRefs[i].TryGet(out NetworkObject networkObject))
            {
                sendUnitList.Add(networkObject.gameObject);
            }
        }

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
            if (!netObj.IsSpawned) netObj.Spawn(true);
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
            UnitListAddServerRpc(unitAi.NetworkObject);
            unitAi.transform.position = new Vector3(-100, -100, 0);

            unitAi.PortalUnitInFuncServerRpc(isInHostMap);
        }
    }
}
