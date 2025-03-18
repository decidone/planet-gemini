using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public enum TrUnitState
{
    idle,
    trMove,
    returnBuild
}

public class TransportUnit : UnitCommonAi
{
    [HideInInspector]
    public Transporter mainTrBuild;
    public Vector3 startPos;
    [HideInInspector]
    public Transporter othTrBuild;
    public Vector3 endPos;

    [HideInInspector]
    AutoSeller autoSeller;
    [HideInInspector]
    AutoBuyer autoBuyer;
    public bool isSellerUnit = false;
    public bool isBuyerUnit = false;
    public bool isHomelessUnit = false;
    int price;
    [HideInInspector]
    public Dictionary<Item, int> itemDic = new Dictionary<Item, int>();

    [HideInInspector]
    public TrUnitState trUnitState = TrUnitState.idle;

    bool mainBuildRemove = false;

    public float visionRadius;
    float fogTimer;

    protected override void Update()
    {
        fogTimer += Time.deltaTime;
        if (fogTimer > MapGenerator.instance.fogCheckCooldown)
        {
            MapGenerator.instance.RemoveFogTile(transform.position, visionRadius);
            fogTimer = 0;
        }
    }

    protected override void UnitAiCtrl()
    {
        if (IsServer)
        {
            switch (trUnitState)
            {
                case TrUnitState.trMove:
                    MoveFunc();
                    break;
                case TrUnitState.returnBuild:
                    ReturnToBuild();
                    break;
            }

            if (isHomelessUnit)
                HomelessDroneMove();
        }
    }

    public void SetHomelessDrone(HomelessDroneSaveData saveData)
    {
        isHomelessUnit = true;
        Vector3 dataStartPos = new Vector3();
        Vector3 dataEndPos = new Vector3();
        Vector3 dataPos = new Vector3();
        dataStartPos = Vector3Extensions.ToVector3(saveData.startPos);
        dataEndPos = Vector3Extensions.ToVector3(saveData.endPos);
        dataPos = Vector3Extensions.ToVector3(saveData.pos);

        if (saveData.state == "idle")
        {
            startPos = dataPos;
            endPos = dataPos;
        }
        else if (saveData.state == "move")
        {
            startPos = dataStartPos;
            endPos = dataEndPos;
        }
        else if (saveData.state == "return")
        {
            startPos = dataEndPos;
            endPos = dataStartPos;
        }
        else
        {
            // homeless drone re-load
            startPos = dataStartPos;
            endPos = dataEndPos;
        }

        foreach (var data in saveData.invenItemData)
        {
            itemDic.Add(GeminiNetworkManager.instance.GetItemSOFromIndex(data.Key), data.Value);
        }
    }

    public void HomelessDroneMove()
    {
        // homeless drone은 이동 후 아이템을 뿌려주기만 하면 되기 때문에 TrUnitState를 쓰지 않음
        tr.position = Vector3.MoveTowards(tr.position, endPos, Time.deltaTime * unitCommonData.MoveSpeed / 2);

        if (tr.position == endPos)
        {
            DestroyTrUnit();
        }
    }

    public void MovePosSet(Transporter _mainTrBuild, Transporter _othTrBuild, Dictionary<Item, int> _itemDic)
    {
        mainTrBuild = _mainTrBuild;
        startPos = mainTrBuild.transform.position;
        othTrBuild = _othTrBuild;
        endPos = othTrBuild.transform.position;

        itemDic = _itemDic;
        trUnitState = TrUnitState.trMove;
    }

    public void MovePosSet(AutoSeller _autoSeller, Vector3 portalPos, Dictionary<Item, int> _itemDic, int _price)
    {
        isSellerUnit = true;
        autoSeller = _autoSeller;
        startPos = _autoSeller.transform.position;
        endPos = portalPos;

        itemDic = _itemDic;
        price = _price;
        trUnitState = TrUnitState.trMove;
    }

    public void MovePosSet(AutoBuyer _autoBuyer, Vector3 portalPos, Dictionary<Item, int> _itemDic)
    {
        isBuyerUnit = true;
        autoBuyer = _autoBuyer;
        startPos = _autoBuyer.transform.position;
        endPos = portalPos;

        itemDic = _itemDic;
        trUnitState = TrUnitState.trMove;
    }

    void MoveFunc()
    {
        if (isBuyerUnit)
            tr.position = Vector3.MoveTowards(tr.position, endPos, Time.deltaTime * unitCommonData.MoveSpeed / 2);
        else
            tr.position = Vector3.MoveTowards(tr.position, endPos, Time.deltaTime * unitCommonData.MoveSpeed / 2);

        if (tr.position == endPos)
        {
            if (isSellerUnit)
            {
                Debug.Log("Seller Unit arrived at the Portal");
                GameManager.instance.AddFinanceServerRpc(price);
                price = 0;
                itemDic.Clear();
                TakeItemEnd(false);
            }
            else if (isBuyerUnit)
            {
                Debug.Log("Buyer Unit arrived at the Portal");

                // 세이브, 로드 시 드론이 반환점을 돌았는지 확인하기 위해서 넣어둔 확인용 아이템 제거
                if (itemDic.Count > 1)
                {
                    itemDic.Remove(ItemList.instance.itemDic["CopperGoblet"]);
                }
                TakeItemEnd(false);
            }
            else
            {
                trUnitState = TrUnitState.idle;
                if (othTrBuild != null)
                {
                    othTrBuild.TakeTransportItem(this, itemDic);
                    itemDic.Clear();
                }
                else
                {
                    DestroyTrUnit();
                }
            }
        }
    }

    //public void TakeItemEnd()
    //{
    //    if (!mainBuildRemove)
    //        trUnitState = TrUnitState.returnBuild;
    //    else
    //        DestroyTpUnit();
    //}

    public void TakeItemEnd(bool isUnitItemClear)
    {
        if (isUnitItemClear)
            itemDic.Clear();

        if (!mainBuildRemove)
            trUnitState = TrUnitState.returnBuild;
        else
            DestroyTrUnit();
    }

    void ReturnToBuild()
    {
        if (isBuyerUnit)
            tr.position = Vector3.MoveTowards(tr.position, startPos, Time.deltaTime * unitCommonData.MoveSpeed / 2);
        else
            tr.position = Vector3.MoveTowards(tr.position, startPos, Time.deltaTime * unitCommonData.MoveSpeed / 2);

        if (tr.position == startPos)
        {
            if (mainBuildRemove)
            {
                DestroyTrUnit();
                return;
            }

            if (isSellerUnit)
            {
                Debug.Log("Seller Unit arrived at the AutoSeller");
                autoSeller.RemoveUnit(this.gameObject);
            }
            else if (isBuyerUnit)
            {
                Debug.Log("Buyer Unit arrived at the AutoBuyer");
                if (itemDic.Count > 0)
                {
                    autoBuyer.TakeTransportItem(this, itemDic);
                    itemDic.Clear();
                }
                else
                    autoBuyer.RemoveUnit(this.gameObject);
            }
            else
            {
                if (itemDic.Count > 0)
                {
                    mainTrBuild.TakeTransportItem(this, itemDic);
                    itemDic.Clear();
                }
                else
                    mainTrBuild.RemoveUnit(this.gameObject);
            }
        }
    }

    void DestroyTrUnit()
    {
        HomelessDroneManager.instance.RemoveDrone(this);

        if (itemDic.Count > 0)
        {
            foreach (var item in itemDic)
            {
                ItemToItemProps(item.Key, item.Value);
            }

            itemDic.Clear();
        }

        Destroy(gameObject);
    }

    public void MainTrBuildRemove()
    {
        mainBuildRemove = true;
    }

    void ItemToItemProps(Item item, int itemAmount)
    {
        int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(item);
        GeminiNetworkManager.instance.ItemSpawnServerRpc(itemIndex, itemAmount, transform.position);
    }
}
