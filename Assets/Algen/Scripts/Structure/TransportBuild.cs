using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TransportBuild : Production
{
    [SerializeField]
    GameObject lineObj;
    [HideInInspector]
    public LineRenderer lineRenderer;

    public TransportBuild takeBuild;

    [SerializeField]
    GameObject trUnit;
    List<GameObject> sendItemUnit = new List<GameObject>();

    Vector3 startLine;
    Vector3 endLine;

    int maxSendAmount;
    public bool isToggleOn = false;
    public int sendAmount;
    List<Dictionary<Item, int>> unitItemList = new List<Dictionary<Item, int>>();
    List<TransportUnit> getItemUnit = new List<TransportUnit>();

    float exTimer;
    float exTimeSet;

    int invenIndex;
    
    protected override void Start()
    {
        base.Start();
        maxFuel = 100;
        exTimeSet = 1.0f;
        invenIndex = 0;
        canInsertItem = true;
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

            if (outObj.Count > 0 && !itemSetDelay && checkObj)
            {
                for (int i = 0; i < 18; i++)
                {
                    var invenItem = inventory.SlotCheck(i);

                    if (invenItem.item != null)
                    {
                        invenIndex = i;
                        SendItem(invenItem.item);
                        break;
                    }
                }
            }
        }
    }

    public override void OpenUI()
    {
        sInvenManager.SetInven(inventory, ui);
        sInvenManager.SetProd(this);
        sInvenManager.progressBar.SetMaxProgress(cooldown);
        sInvenManager.TransportBuildSetting(isToggleOn, sendAmount);

        if (takeBuild != null)
        {
            startLine = new Vector3(transform.position.x, transform.position.y, -1);
            endLine = new Vector3(takeBuild.transform.position.x, takeBuild.transform.position.y, -1);

            GameObject currentLine = Instantiate(lineObj, startLine, Quaternion.identity);
            lineRenderer = currentLine.GetComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, startLine);
            lineRenderer.SetPosition(1, endLine);
        }
    }

    public override void CloseUI()
    {
        sInvenManager.ReleaseInven();

        if (lineRenderer != null)
            Destroy(lineRenderer.gameObject);
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
        base.OnFactoryItem(itemProps);
    }

    public override void OnFactoryItem(Item item)
    {
        inventory.Add(item, 1);
    }

    protected override void SubFromInventory()
    {
        inventory.Sub(invenIndex, 1);
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
        foreach(GameObject trUnit in sendItemUnit)
        {
            trUnit.GetComponent<TransportUnit>().MainTrBuildRemove();
        }

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

    public void ResetTakeBuild()
    {
        if(lineRenderer != null)        
            Destroy(lineRenderer.gameObject);

        takeBuild = null;
    }
}
