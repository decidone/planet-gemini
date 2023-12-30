using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Portal : Production
{
    private Tilemap tilemap;
    GameManager gameManager;
    [SerializeField]
    GameObject[] portalTile;

    PortalSciManager portalSci;
    Dictionary<string, GameObject> portalObjList = new Dictionary<string, GameObject>();

    Portal otherPortal;

    protected override void Awake()
    {
        myVision.SetActive(false);
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

    public override void OpenUI()
    {
        sInvenManager.SetInven(inventory, ui);
        sInvenManager.SetProd(this);
        sInvenManager.PortalProductionSet();
        sInvenManager.progressBar.gameObject.SetActive(false);
        sInvenManager.energyBar.gameObject.SetActive(false);
    }

    public override void CloseUI()
    {
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

    public void MapDataSet()
    {
        Vector2 pos = transform.position;
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);

        for (int a = 0; a < 2; a++)
        {
            for (int b = -1; b < 1; b++)
            {
                gameManager.map.mapData[x + b][y + a].structure = this.gameObject;
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
                    gameManager.map.mapData[x + b][y + a].buildable.Clear();
                    gameManager.map.mapData[x + b][y + a].buildable.Add("PortalObj");
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
        if(portalObjList.ContainsKey(objName))
        {
            find = true;
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

            if (objName == "PortalItemIn")
            {
                PortalItemIn portalItemIn = obj.GetComponent<PortalItemIn>();
                portalItemIn.myPortal = this;

                GameObject othObj = otherPortal.ReturnObj("PortalItemOut");
                if (othObj)
                {
                    portalItemIn.ConnectObj(othObj);
                }
            }
            else if (objName == "PortalItemOut")
            {
                PortalItemOut portalItemOut = obj.GetComponent<PortalItemOut>();
                portalItemOut.myPortal = this;

                GameObject othObj = otherPortal.ReturnObj("PortalItemIn");
                if (othObj)
                {
                    othObj.GetComponent<PortalItemIn>().ConnectObj(obj);
                }
            }
            else if (objName == "PortalUnitIn")
            {
                PortalUnitIn portalUnitIn = obj.GetComponent<PortalUnitIn>();
                portalUnitIn.myPortal = this;

                GameObject othObj = otherPortal.ReturnObj("PortalUnitOut");
                if (othObj)
                {
                    portalUnitIn.ConnectObj(othObj);
                }

                GameObject myObj = ReturnObj("PortalUnitOut");
                if (myObj)
                {
                    portalUnitIn.ConnectMyObj(myObj);
                }
            }
            else if (objName == "PortalUnitOut")
            {
                PortalUnitOut portalUnitOut = obj.GetComponent<PortalUnitOut>();
                portalUnitOut.myPortal = this;

                GameObject othObj = otherPortal.ReturnObj("PortalUnitIn");
                if (othObj)
                {
                    othObj.GetComponent<PortalUnitIn>().ConnectObj(obj);
                }

                GameObject myObj = ReturnObj("PortalUnitIn");
                if (myObj)
                {
                    myObj.GetComponent<PortalUnitIn>().ConnectMyObj(obj);
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
}
