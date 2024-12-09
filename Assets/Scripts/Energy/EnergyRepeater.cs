using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class EnergyRepeater : Structure
{
    public EnergyGroupConnector connector;
    [SerializeField]
    SpriteRenderer view;
    public bool isImprovedRepeater;
    bool isBuildDone;
    bool isPlaced;
    GameManager gameManager;
    PreBuilding preBuilding;
    Structure preBuildingStr;
    bool preBuildingCheck;

    protected void Start()
    {
        isBuildDone = false;
        isPlaced = false;
        gameManager = GameManager.instance;
        preBuilding = PreBuilding.instance;
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
            if (preBuilding.isBuildingOn && !removeState)
            {
                if (!preBuildingCheck)
                {
                    preBuildingCheck = true;
                    if (preBuilding.isEnergyUse || preBuilding.isEnergyStr)
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

    [ServerRpc(RequireOwnership = false)]
    public override void RemoveObjServerRpc()
    {
        DisableFocused();
        connector.RemoveFromGroup();
        base.RemoveObjServerRpc();
    }

    public override (bool, bool, bool, EnergyGroup, float) PopUpEnergyCheck()
    {
        if (connector != null && connector.group != null)
        {
            return (energyUse, isEnergyStr, false, connector.group, energyConsumption);
        }

        return (false, false, false, null, 0);
    }
}
