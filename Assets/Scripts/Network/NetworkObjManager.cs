using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Pathfinding;

public class NetworkObjManager : NetworkBehaviour
{
    public List<Portal> netPortals = new List<Portal>();
    public List<Structure> netStructures = new List<Structure>();
    public List<BeltGroupMgr> netBeltGroupMgrs = new List<BeltGroupMgr>();
    public List<UnitCommonAi> netUnitCommonAis = new List<UnitCommonAi>();
    public List<BeltCtrl> networkBelts = new List<BeltCtrl>();

    public delegate void OnStructureChanged(int type);
    public OnStructureChanged onStructureChangedCallback;

    public delegate void OnUnitChanged(int type);
    public OnUnitChanged onUnitChangedCallback;

    #region Singleton
    public static NetworkObjManager instance;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }
    #endregion

    public void NetObjAdd(GameObject netObj)
    {
        if(netObj.TryGetComponent(out Portal portal))
        {
            netPortals.Add(portal);
        }
        else if (netObj.TryGetComponent(out Structure structure) && !netObj.GetComponent<BeltCtrl>())
        {
            netStructures.Add(structure);
            onStructureChangedCallback?.Invoke(20);
        }
        else if (netObj.TryGetComponent(out BeltGroupMgr beltGroupMgr))
        {
            netBeltGroupMgrs.Add(beltGroupMgr);
        }
        else if (netObj.TryGetComponent(out UnitCommonAi unitCommonAi))
        {
            netUnitCommonAis.Add(unitCommonAi);
            onUnitChangedCallback?.Invoke(23);
        }
        else
        {
            networkBelts.Add(netObj.GetComponent<BeltCtrl>());
        }
    }

    public void NetObjRemove(GameObject netObj)
    {
        if (netObj.GetComponent<BeltCtrl>())
        {
            networkBelts.Remove(netObj.GetComponent<BeltCtrl>());
        }
        else if (netObj.TryGetComponent(out Structure structure))
        {
            netStructures.Remove(structure);
        }
        else if (netObj.TryGetComponent(out BeltGroupMgr beltGroupMgr))
        {
            netBeltGroupMgrs.Remove(beltGroupMgr);
        }
        else if (netObj.TryGetComponent(out UnitCommonAi unitCommonAi))
        {
            netUnitCommonAis.Remove(unitCommonAi);
        }
    }

    public bool StructureCheck(StructureData strData)
    {
        for (int i = 0; i < netStructures.Count; i++)
        {
            if (netStructures[i].structureData == strData)
            {
                return true;
            }
        }

        return false;
    }

    public bool UnitCheck(UnitCommonData unitData)
    {
        for (int i = 0; i < netUnitCommonAis.Count; i++)
        {
            if (netUnitCommonAis[i].unitCommonData == unitData)
            {
                return true;
            }
        }

        return false;
    }

    public void InitConnectors()
    {
        Debug.Log("InitConnectors");
        for (int i = 0; i < netStructures.Count; i++)
        {
            EnergyGroupConnector connector = netStructures[i].GetComponentInChildren<EnergyGroupConnector>();
            if (connector != null)
            {
                connector.RemoveGroup();
            }
        }

        for (int i = 0; i < netStructures.Count; i++)
        {
            EnergyGroupConnector connector = netStructures[i].GetComponentInChildren<EnergyGroupConnector>();
            if (connector != null)
            {
                connector.Init();
            }
        }
    }
}
