using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Portal : Production
{
    private Tilemap tilemap;
    GameManager gameManager;
    [SerializeField]
    GameObject[] portalTile;

    PortalSciManager portalSci;
    public Dictionary<string, GameObject> portalObjList = new Dictionary<string, GameObject>();
    public Portal otherPortal;
    public GameObject scienceBuilding;

    protected override void Awake()
    {
        //myVision.SetActive(false);
        portalSci = PortalSciManager.instance;
        inventory = this.GetComponent<Inventory>();
    }

    protected override void Start()
    {
        gameManager = GameManager.instance;
        canvas = gameManager.GetComponent<GameManager>().inventoryUiCanvas;
        sInvenManager = canvas.GetComponent<StructureInvenManager>();
        GetUIFunc();
        tilemap = GameObject.Find("Tilemap").GetComponent<Tilemap>();
    }

    protected override void Update() { }

    public override void OpenUI()
    {
        base.OpenUI();
        PortalSciManager.instance.UISet();
        sInvenManager.SetInven(inventory, ui);
        sInvenManager.SetProd(this);
        sInvenManager.PortalProductionSet();
        sInvenManager.progressBar.gameObject.SetActive(false);
        sInvenManager.energyBar.gameObject.SetActive(false);
    }

    public override void CloseUI()
    {
        base.CloseUI();
        sInvenManager.progressBar.gameObject.SetActive(true);
        sInvenManager.energyBar.gameObject.SetActive(true);
        sInvenManager.ReleaseInven();
    }

    public override void GetUIFunc()
    {
        InventoryList inventoryList = canvas.GetComponent<InventoryList>();

        foreach (GameObject list in inventoryList.StructureStorageArr)
        {
            if (list.name == "Portal")
            {
                ui = list;
            }
        }
    }

    public void MapDataSet(Map map)
    {
        Vector2 pos = transform.position;
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);

        for (int a = -1; a < 1; a++)
        {
            for (int b = -1; b < 1; b++)
            {
                map.GetCellDataFromPos(x + b, y + a).structure = this.gameObject;
                map.GetCellDataFromPos(x + b, y + a).buildable.Clear();
                map.GetCellDataFromPos(x + b, y + a).buildable.Add("ScienceBuilding");
            }
        }

        for (int i = 0; i < portalTile.Length; i++)
        {
            pos = portalTile[i].transform.position;
            x = Mathf.FloorToInt(pos.x);
            y = Mathf.FloorToInt(pos.y);

            for (int a = -1; a < 1; a++)
            {
                for (int b = -1; b < 1; b++)
                {
                    map.GetCellDataFromPos(x + b, y + a).buildable.Clear();
                    map.GetCellDataFromPos(x + b, y + a).buildable.Add("PortalObj");
                }
            }
        }
    }

    public void OtherPortalSet(Portal _othPortal)
    {
        otherPortal = _othPortal;
    }

    public bool PortalObjFind(string objName)
    {
        bool find = false;

        Transform[] allChildren = GetComponentsInChildren<Transform>(true);

        foreach (Transform child in allChildren)
        {
            if (child.name.Contains(objName))
            {
                find = true;
            }
        }

        return find;
    }

    public GameObject ReturnObj(string objName)
    {
        portalObjList.TryGetValue(objName , out GameObject portalObj);
        if (portalObj)
            return portalObj;
        else
            return null;
    }

    public void SetPortalObjEnd(string objName, GameObject obj)
    {
        if (!portalObjList.ContainsKey(objName))
        {
            portalObjList.Add(objName, obj);
            //ObjSyncServerRpc(objName, NetworkObjManager.instance.FindNetObjID(obj));
            var objId = NetworkObjManager.instance.FindNetObjID(obj);

            if (objName == "PortalItemIn")
            {
                PortalItemIn portalItemIn = obj.GetComponent<PortalItemIn>();
                portalItemIn.myPortal = this;

                GameObject othObj = otherPortal.ReturnObj("PortalItemOut");
                if (othObj)
                {
                    var othId = NetworkObjManager.instance.FindNetObjID(othObj);
                    portalItemIn.ConnectObjServerRpc(othId);
                }
            }
            else if (objName == "PortalItemOut")
            {
                PortalItemOut portalItemOut = obj.GetComponent<PortalItemOut>();
                portalItemOut.myPortal = this;

                GameObject othObj = otherPortal.ReturnObj("PortalItemIn");
                if (othObj)
                {
                    othObj.GetComponent<PortalItemIn>().ConnectObjServerRpc(objId);
                }
            }
            else if (objName == "PortalUnitIn")
            {
                PortalUnitIn portalUnitIn = obj.GetComponent<PortalUnitIn>();
                portalUnitIn.myPortal = this;

                GameObject othObj = otherPortal.ReturnObj("PortalUnitOut");
                if (othObj)
                {
                    var othId = NetworkObjManager.instance.FindNetObjID(othObj);
                    portalUnitIn.ConnectObjServerRpc(othId);
                }

                GameObject myObj = ReturnObj("PortalUnitOut");
                if (myObj)
                {
                    var othId = NetworkObjManager.instance.FindNetObjID(myObj);
                    portalUnitIn.ConnectMyObjServerRpc(othId);
                }
            }
            else if (objName == "PortalUnitOut")
            {
                PortalUnitOut portalUnitOut = obj.GetComponent<PortalUnitOut>();
                portalUnitOut.myPortal = this;

                GameObject othObj = otherPortal.ReturnObj("PortalUnitIn");
                if (othObj)
                {
                    othObj.GetComponent<PortalUnitIn>().ConnectObjServerRpc(objId);
                }

                GameObject myObj = ReturnObj("PortalUnitIn");
                if (myObj)
                {
                    myObj.GetComponent<PortalUnitIn>().ConnectMyObjServerRpc(objId);
                }
            }
        }
    }

    public void RemovePortalObj(string objName)
    {
        if (portalObjList.ContainsKey(objName))
        {
            portalObjList.Remove(objName);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetScienceBuildingServerRpc()
    {
        if (IsServer)
        {
            GameObject spawnobj = Instantiate(scienceBuilding, transform.position, Quaternion.identity);
            spawnobj.TryGetComponent(out NetworkObject netObj);
            if (!netObj.IsSpawned) spawnobj.GetComponent<NetworkObject>().Spawn(true);
            spawnobj.transform.parent = transform;
            spawnobj.GetComponent<ScienceBuilding>().SetPortal(isInHostMap);
        }
    }
}
