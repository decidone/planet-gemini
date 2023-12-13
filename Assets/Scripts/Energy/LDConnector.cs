using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LDConnector : Structure
{
    public EnergyGroupConnector connector;
    [SerializeField]
    SpriteRenderer view;
    bool isBuildDone;
    bool isPlaced;

    protected void Start()
    {
        isBuildDone = false;
        isPlaced = false;
    }

    protected void Update()
    {
        if (!removeState)
        {
            if (isRuin && isRepair)
            {
                RepairFunc(false);
            }
            else if (isPreBuilding && isSetBuildingOk && !isRuin)
            {
                RepairFunc(true);
            }
        }

        if (!isPlaced)
        {
            if (isSetBuildingOk)
            {
                view.enabled = false;
                isPlaced = true;
            }
        }

        if (!isPreBuilding)
        {
            if (!isBuildDone)
            {
                connector.Init();
                isBuildDone = true;
            }
        }
    }

    public override void RemoveObj()
    {
        //여기서 건물 철거 전 처리(삭제가 아니여도 비활성화가 필요하니 그거 생각해서 만들 것)
        connector.RemoveFromGroup();

        base.RemoveObj();
    }
}
