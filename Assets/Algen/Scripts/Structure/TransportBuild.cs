using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TransportBuild : Production
{
    public TransportBuild takeBuild;
    public List<TransportBuild> sendBuildList = new List<TransportBuild>();

    [SerializeField]
    GameObject trUnit;
    List<GameObject> sendItemUnit = new List<GameObject>();

    int maxSendAmount;
    public bool isToggleOn = false;
    public int sendAmount;
    List<Dictionary<Item, int>> unitItemList = new List<Dictionary<Item, int>>();
    List<TransportUnit> getItemUnit = new List<TransportUnit>();

    float exTimer;
    float exTimeSet;
    
    protected override void Start()
    {
        base.Start();
        maxFuel = 100;
        exTimeSet = 1.0f;
        isStorageBuild = true;
        isGetLine = true;
    }

    protected override void Update()
    {
        base.Update();
        if (!isPreBuilding)
        {
            if (takeBuild != null && sendItemUnit.Count <= 3 && takeBuild.TryGetComponent(out TransportBuild othBuild) && othBuild.unitItemList.Count == 0)
            {
                prodTimer += Time.deltaTime;
                if (prodTimer > cooldown)
                {
                    if(sendItemUnit.Count < 3)
                    {
                       if (!isToggleOn)
                        {
                            SendTransportItemDicCheck(othBuild);
                            prodTimer = 0;
                        }
                        else
                        {
                            if (sendAmount != 0 && inventory.TotalItemsAmountLimitCheck(sendAmount))
                            {
                                SendTransportItemDicCheck(othBuild);
                                prodTimer = 0;
                            }
                        }
                    }
                }
            }
            else
                prodTimer = 0;

            if (unitItemList.Count > 0)
            {
                exTimer += Time.deltaTime;
                if (exTimer > exTimeSet)
                {
                    ExStorageCheck();
                    exTimer = 0;
                }
            }
            else
                exTimer = 0;

            if (sInvenManager.isOpened && sInvenManager.prod == GetComponent<Production>() && takeBuild != null)
                LineRendererSet(takeBuild.transform.position);
        }
    }

    public override void OpenUI()
    {
        sInvenManager.SetInven(inventory, ui);
        sInvenManager.SetProd(this);
        sInvenManager.progressBar.SetMaxProgress(cooldown);
        sInvenManager.TransportBuildSetting(isToggleOn, sendAmount);

        if(takeBuild != null)
            LineRendererSet(takeBuild.transform.position);
    }

    public override void CloseUI()
    {
        sInvenManager.ReleaseInven();

        base.ResetLineRenderer();
    }

    void SendTransportItemDicCheck(TransportBuild othBuild)
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

        if (invItemCheckDic != null && invItemCheckDic.Count > 0)
        {
            GameObject unit = Instantiate(trUnit, transform.position, Quaternion.identity);
            sendItemUnit.Add(unit);
            unit.GetComponent<TransportUnit>().MovePosSet(this, othBuild, invItemCheckDic);
            foreach (var dicData in invItemCheckDic)
            {
                inventory.Sub(dicData.Key, dicData.Value);
            }
        }
    }

    public void RemoveUnit(GameObject returnUnit)
    {
        sendItemUnit.Remove(returnUnit);
        Destroy(returnUnit);
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
        inventory.Add(itemProps.item, itemProps.amount);
        itemProps.Pool.Release(itemProps.gameObject);
    }

    public override void OnFactoryItem(Item item)
    {
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
            if (list.name == "TransportBuild")
            {
                ui = list;
            }
        }
    }

    protected override void AddInvenItem()
    {
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
            unitItemList.Add(_itemDic);
            getItemUnit.Add(takeUnit);
            ExStorageCheck();
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

    public override void ResetLineRenderer()
    {
        base.ResetLineRenderer();
        takeBuild = null;
    }

    public void TakeBuildSet(TransportBuild trBuild)
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

        foreach (TransportBuild transport in sendBuildList)
        {
            transport.ResetLineRenderer();
            foreach (GameObject trUnit in transport.sendItemUnit)
            {
                trUnit.GetComponent<TransportUnit>().TakeItemEnd();
            }
        }
    }
}
