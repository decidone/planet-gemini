using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyRepeater : Structure
{
    public EnergyGroupConnector connector;
    [SerializeField]
    SpriteRenderer view;
    bool isBuildDone;
    bool isPlaced;
    GameManager gameManager;
    [HideInInspector]
    public GameObject preBuildingObj;
    Structure preBuildingStr;
    bool preBuildingCheck;

    protected void Start()
    {
        isBuildDone = false;
        isPlaced = false;
        gameManager = GameManager.instance;
        preBuildingObj = gameManager.preBuildingObj;
    }

    protected override void Update()
    {
        base.Update();

        if (!isPlaced)
        {
            if (isSetBuildingOk)
            {
                view.enabled = false;
                isPlaced = true;
            }
        }
        if (gameManager.focusedStructure == null)
        {
            if (preBuildingObj.activeSelf)
            {
                if (!preBuildingCheck)
                {
                    preBuildingCheck = true;
                    preBuildingStr = preBuildingObj.GetComponentInChildren<Structure>();
                    if (preBuildingStr != null && (preBuildingStr.energyUse || preBuildingStr.isEnergyStr))
                    {
                        view.enabled = true;
                    }
                }
            }
            else
            {
                if (preBuildingCheck)
                {
                    preBuildingCheck = false;
                    view.enabled = false;
                }
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

    public override void Focused()
    {
        if (connector.group != null)
        {
            connector.group.TerritoryViewOn();
        }
    }

    public override void DisableFocused()
    {
        if (connector.group != null)
        {
            connector.group.TerritoryViewOff();
        }
    }

    public override void RemoveObj()
    {
        connector.RemoveFromGroup();
        base.RemoveObj();
    }
}
