using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TrUnitState
{
    idle,
    trMove,
    returnBuild
}

public class TransportUnit : UnitCommonAi
{
    [HideInInspector]
    public TransportBuild mainTrBuild;
    Vector3 startPos;
    [HideInInspector]
    public TransportBuild othTrBuild;
    Vector3 endPos;

    Dictionary<Item, int> itemDic = new Dictionary<Item, int>();

    [HideInInspector]
    public TrUnitState trUnitState = TrUnitState.idle;

    bool mainBuildRemove = false;

    protected override void Update()
    {

    }

    protected override void UnitAiCtrl()
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
    }

    public void MovePosSet(TransportBuild _mainTrBuild, TransportBuild _othTrBuild, Dictionary<Item, int> _itemDic)
    {
        mainTrBuild = _mainTrBuild;
        startPos = mainTrBuild.transform.position;
        othTrBuild = _othTrBuild;
        endPos = othTrBuild.transform.position;

        itemDic = _itemDic;
        trUnitState = TrUnitState.trMove;
    }

    void MoveFunc()
    {
        tr.position = Vector3.MoveTowards(tr.position, endPos, Time.deltaTime * unitCommonData.MoveSpeed);
        if (tr.position == endPos)
        {
            trUnitState = TrUnitState.idle;
            othTrBuild.TakeTransportItem(this, itemDic);
            itemDic.Clear();
        }
    }

    public void TakeItemEnd()
    {
        if (!mainBuildRemove)
            trUnitState = TrUnitState.returnBuild;
        else
            Destroy(gameObject);
    }

    void ReturnToBuild()
    {
        tr.position = Vector3.MoveTowards(tr.position, startPos, Time.deltaTime * unitCommonData.MoveSpeed * 2);
        if (tr.position == startPos)
        {
            if(itemDic.Count > 0)
                mainTrBuild.TakeTransportItem(this, itemDic);
            else
                mainTrBuild.RemoveUnit(this.gameObject);
        }

        if(mainBuildRemove)
            Destroy(gameObject);
    }

    public void MainTrBuildRemove()
    {
        mainBuildRemove = true;
    }
}