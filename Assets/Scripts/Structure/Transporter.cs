using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Netcode;

public class Transporter : Production
{
    public Transporter takeBuild;
    public List<Transporter> sendBuildList = new List<Transporter>();

    [SerializeField]
    GameObject trUnit;
    List<GameObject> sendItemUnit = new List<GameObject>();

    int maxSendAmount;
    public bool isToggleOn = false;
    public int sendAmount;
    List<Dictionary<Item, int>> unitItemList = new List<Dictionary<Item, int>>();
    List<TransportUnit> getItemUnit = new List<TransportUnit>();

    float transportInterval;
    float transportTimeer;

    Animator animator;
    Transporter othBuildAni;
    Dictionary<Item, int> invItemCheckDicAni = new Dictionary<Item, int>();

    protected override void Start()
    {
        base.Start();
        maxFuel = 100;
        transportTimeer = 1.0f;
        isStorageBuilding = true;
        animator = GetComponent<Animator>();
    }

    protected override void Update()
    {
        base.Update();
        if (!isPreBuilding)
        {
            if (takeBuild != null)
            {
                prodTimer += Time.deltaTime;
                if (prodTimer > cooldown)
                {
                    if (!isToggleOn)
                    {
                        if (IsServer && sendItemUnit.Count < 3 && takeBuild.TryGetComponent(out Transporter othBuild) && othBuild.unitItemList.Count == 0)
                        {
                            SendTransportItemDicCheck(othBuild);
                        }
                        prodTimer = 0;
                    }
                    else
                    {
                        if (sendAmount != 0 && inventory.TotalItemsAmountLimitCheck(sendAmount))
                        {
                            if (IsServer && sendItemUnit.Count < 3 && takeBuild.TryGetComponent(out Transporter othBuild) && othBuild.unitItemList.Count == 0)
                            {
                                SendTransportItemDicCheck(othBuild);
                            }
                            prodTimer = 0;
                        }
                    }
                }
            }
            //if (takeBuild != null && sendItemUnit.Count <= 3 && takeBuild.TryGetComponent(out Transporter othBuild) && othBuild.unitItemList.Count == 0)
            //{
            //    prodTimer += Time.deltaTime;
            //    if (prodTimer > cooldown)
            //    {
            //        if (sendItemUnit.Count < 3)
            //        {
            //            if (!isToggleOn)
            //            {
            //                if(IsServer)
            //                    SendTransportItemDicCheck(othBuild);
            //                prodTimer = 0;
            //            }
            //            else
            //            {
            //                if (sendAmount != 0 && inventory.TotalItemsAmountLimitCheck(sendAmount))
            //                {
            //                    if (IsServer)
            //                        SendTransportItemDicCheck(othBuild);
            //                    prodTimer = 0;
            //                }
            //            }
            //        }
            //    }
            //}
            else
                prodTimer = 0;


            if (IsServer) 
            {
                if (unitItemList.Count > 0)
                {
                    transportInterval += Time.deltaTime;
                    if (transportInterval > transportTimeer)
                    {
                        if (IsServer)
                            ExStorageCheck();
                        transportInterval = 0;
                    }
                }
                else
                    transportInterval = 0;
            }
        }
    }

    public override void OpenUI()
    {
        base.OpenUI();
        sInvenManager.SetInven(inventory, ui);
        sInvenManager.SetProd(this);
        sInvenManager.progressBar.SetMaxProgress(cooldown);
        sInvenManager.TransporterSetting(isToggleOn, sendAmount);

        //if (takeBuild != null)
        //    LineRendererSet(takeBuild.transform.position);
    }

    public override void CloseUI()
    {
        base.CloseUI();
        sInvenManager.ReleaseInven();

        base.DestroyLineRenderer();
    }

    protected override void OnClientConnectedCallback(ulong clientId)
    {
        base.OnClientConnectedCallback(clientId);
        ConnectedSetServerRpc();
    }


    [ServerRpc(RequireOwnership = false)]
    void ConnectedSetServerRpc()
    {
        if (takeBuild != null)
        {
            ConnectedSetClientRpc(takeBuild.transform.position);
        }
    }

    [ClientRpc]
    void ConnectedSetClientRpc(Vector3 pos)
    {
        if (IsServer)
            return;

        StartCoroutine(SetInvoke(pos));
    }

    IEnumerator SetInvoke(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        Map map;
        if (isInHostMap)
            map = GameManager.instance.hostMap;
        else
            map = GameManager.instance.clientMap;

        Cell cell = map.GetCellDataFromPos(x, y);

        while (cell.structure == null)
        {
            yield return null;
        }

        GameObject findObj = cell.structure;
        if (findObj != null && findObj.TryGetComponent(out Transporter takeTransporter))
        {
            if (TryGetComponent(out MapClickEvent mapClick) && takeTransporter.TryGetComponent(out MapClickEvent othMapClick))
            {
                mapClick.GameStartSetRenderer(othMapClick);
            } 
        }
    }

    void SendTransportItemDicCheck(Transporter othBuild)
    {
        if (!isToggleOn)
            maxSendAmount = 99;
        else
            maxSendAmount = sendAmount;

        int Sendcalculate = 0;
        Dictionary<Item, int> invItemCheckDic = new Dictionary<Item, int>();

        for (int i = 0; i < 18; i++)
        {
            var invenItem = inventory.SlotCheck(i);

            if (invenItem.item != null)
            {
                int availableAmount = Mathf.Min(invenItem.amount, maxSendAmount - Sendcalculate);

                if (!invItemCheckDic.ContainsKey(invenItem.item))
                {
                    invItemCheckDic.Add(invenItem.item, availableAmount);
                }
                else
                {
                    invItemCheckDic[invenItem.item] += availableAmount;
                }

                Sendcalculate += availableAmount;

                if (Sendcalculate >= maxSendAmount)
                    break;
            }
        }

        othBuildAni = othBuild;
        invItemCheckDicAni = invItemCheckDic;

        animator.Play("Open", -1, 0);
    }

    public void RemoveUnit(GameObject returnUnit)
    {
        sendItemUnit.Remove(returnUnit);
        Destroy(returnUnit);
        animator.Play("ItemGetOpen", -1, 0);
    }

    public void UnitSendOpen()
    {
        if (invItemCheckDicAni != null && invItemCheckDicAni.Count > 0)
        {
            GameObject unit = Instantiate(trUnit, transform.position, Quaternion.identity);
            unit.TryGetComponent(out NetworkObject netObj);
            if (!netObj.IsSpawned) unit.GetComponent<NetworkObject>().Spawn();
            
            sendItemUnit.Add(unit);
            unit.GetComponent<TransportUnit>().MovePosSet(this, othBuildAni, invItemCheckDicAni);
            foreach (var dicData in invItemCheckDicAni)
            {
                inventory.Sub(dicData.Key, dicData.Value);
            }
        }
    }

    public override bool CanTakeItem(Item item)
    {
        bool canTake = false;
        int containableAmount = inventory.SpaceCheck(item);

        if (1 <= containableAmount)
        {
            canTake = true;
        }
        else if (containableAmount != 0)
        {
            canTake = true;
        }
        else
        {
            canTake = false;
        }

        return canTake;
    }

    public override void OnFactoryItem(ItemProps itemProps)
    {
        if (IsServer)
            inventory.Add(itemProps.item, itemProps.amount);
        itemProps.itemPool.Release(itemProps.gameObject);
    }

    public override void OnFactoryItem(Item item)
    {
        if (IsServer)
            inventory.Add(item, 1);
    }

    public override (Item, int) QuickPullOut()
    {
        return (null, 0);
    }

    public override void GetUIFunc()
    {
        InventoryList inventoryList = canvas.GetComponent<InventoryList>();

        foreach (GameObject list in inventoryList.StructureStorageArr)
        {
            if (list.name == "Transporter")
            {
                ui = list;
            }
        }
    }

    public override void AddInvenItem()
    {
        base.AddInvenItem();
        for (int i = 0; i < 18; i++)
        {
            var invenItem = inventory.SlotCheck(i);

            if (invenItem.item != null && invenItem.amount > 0)
            {
                playerInven.Add(invenItem.item, invenItem.amount);
            }
        }
    }

    public override Dictionary<Item, int> PopUpItemCheck()
    {
        Dictionary<Item, int> returnDic = new Dictionary<Item, int>();

        int itemsCount = 0;
        //다른 슬롯의 같은 아이템도 개수 추가하도록
        for (int i = 0; i < 18; i++)
        {
            var invenItem = inventory.SlotCheck(i);

            if (invenItem.item != null && invenItem.amount > 0)
            {
                if (!returnDic.ContainsKey(invenItem.item))
                {
                    returnDic.Add(invenItem.item, invenItem.amount);
                }
                else
                {
                    returnDic[invenItem.item] += invenItem.amount;
                }
                itemsCount++;
                if (itemsCount > 5)
                    break;
            }
        }

        if (returnDic.Count > 0)
        {
            return returnDic;
        }
        else
            return null;
    }

    public void TakeTransportItem(TransportUnit takeUnit, Dictionary<Item, int> _itemDic)
    {
        if (_itemDic != null && _itemDic.Count > 0)
        {
            unitItemList.Add(new Dictionary<Item, int>(_itemDic));
            getItemUnit.Add(takeUnit);
            ExStorageCheck();
            animator.Play("ItemGetOpen", -1, 0);
        }
    }

    void ExStorageCheck()
    {
        foreach (var exStorage in unitItemList[0].ToList()) // ToList()를 사용하여 복제
        {
            int containableAmount = inventory.SpaceCheck(exStorage.Key);
            if (exStorage.Value <= containableAmount)
            {
                inventory.Add(exStorage.Key, exStorage.Value);
                unitItemList[0].Remove(exStorage.Key);
            }
            else if (containableAmount != 0)
            {
                inventory.Add(exStorage.Key, containableAmount);
                unitItemList[0][exStorage.Key] -= containableAmount; // 원래 변수 수정
            }
            else
            {
                Debug.Log("not enough space");
                break;
            }
        }

        Debug.Log(unitItemList[0].Count);

        if (unitItemList[0].Count == 0)
        {
            unitItemList.RemoveAt(0);
            getItemUnit[0].TakeItemEnd();
            getItemUnit.RemoveAt(0);
        }
    }

    public void SendFuncSet(bool toggleOn, int amount)
    {
        isToggleOn = toggleOn;
        sendAmount = amount;
    }

    public override void DestroyLineRenderer()
    {
        base.DestroyLineRenderer();
        takeBuild = null;
    }

    public void TakeBuildReset()
    {
        takeBuild = null;
    }

    public void TakeBuildSet(Transporter trBuild)
    {
        takeBuild = trBuild;
        trBuild.sendBuildList.Add(this);
    }

    public void RemoveFunc()
    {
        foreach (GameObject trUnit in sendItemUnit)
        {
            trUnit.GetComponent<TransportUnit>().MainTrBuildRemove();
        }

        foreach (Transporter transport in sendBuildList)
        {
            transport.DestroyLineRenderer();
            foreach (GameObject trUnit in transport.sendItemUnit)
            {
                trUnit.GetComponent<TransportUnit>().TakeItemEnd();
            }
        }
    }

    protected override void ItemDrop()
    {
        if (itemList.Count > 0)
        {
            foreach (Item item in itemList)
            {
                ItemToItemProps(item, 1);
            }
        }

        if (itemObjList.Count > 0)
        {
            foreach (ItemProps itemProps in itemObjList)
            {
                itemProps.ResetItemProps();
            }
        }
    }

    public override StructureSaveData SaveData()
    {
        StructureSaveData data = base.SaveData();

        if(takeBuild != null)
        {
            data.connectedStrPos.Add(Vector3Extensions.FromVector3(takeBuild.tileSetPos));
        }

        return data;
    }
}
