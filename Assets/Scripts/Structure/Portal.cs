using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Portal : Production
{
    private Tilemap tilemap;
    [SerializeField]
    GameObject[] portalTile;

    public Dictionary<string, GameObject> portalObjList = new Dictionary<string, GameObject>();
    public Portal otherPortal;
    public GameObject scienceBuilding;

    bool bloodMoonEvent;
    Recipe selectRecipe;

    protected override void Awake()
    {
        //myVision.SetActive(false);
        buildName = "Portal";   // 포탈 건물은 따로 데이터를 두지 않아서 직접 이름을 잡아줌
        inventory = GetComponent<Inventory>();
        inventory.PortalInvenSet();
        visionPos = new Vector3(transform.position.x, transform.position.y + 1, 0);
        onEffectUpgradeCheck += IncreasedStructureCheck;
        onEffectUpgradeCheck.Invoke();
    }

    protected override void Start()
    {
        gameManager = GameManager.instance;
        itemDic = ItemList.instance.itemDic;
        canvas = gameManager.GetComponent<GameManager>().inventoryUiCanvas;
        sInvenManager = canvas.GetComponent<StructureInvenManager>();
        GetUIFunc();
        tilemap = GameObject.Find("Tilemap").GetComponent<Tilemap>();
        if(IsServer && MainGameSetting.instance.isNewGame)
            SetScienceBuildingServerRpc();
        selectRecipe = RecipeList.instance.GetRecipeIndex("Portal", 0);
        settingEndCheck = true;
    }

    protected override void Update() 
    {
        if(isUIOpened && bloodMoonEvent)
        {
            prodTimer += Time.deltaTime;
            if (prodTimer > cooldown)
            {
                var timeData = gameManager.BloodMoonProgressSet();
                sInvenManager.progressBar.SetMaxProgress(timeData.Item1);
                prodTimer = timeData.Item2;
            }
        }
    }

    protected override void OnClientConnectedCallback(ulong clientId)
    {
        if (inventory != null)
            ItemSyncServerRpc();
    }


    public override void OpenUI()
    {
        base.OpenUI();
        PortalSciManager.instance.UISet();
        sInvenManager.SetInven(inventory, ui);
        sInvenManager.SetProd(this);
        sInvenManager.PortalProductionSet();
        sInvenManager.InvenInit();
        SetRecipe(selectRecipe, 0);
    }

    public override void SetRecipe(Recipe _recipe, int index)
    {
        recipe = _recipe;
        recipeIndex = index;
        sInvenManager.ResetInvenOption();
        BloodMoonProgressSet();        
        sInvenManager.slots[0].SetInputItem(itemDic[recipe.items[0]]);
        sInvenManager.slots[0].SetNeedAmount(recipe.amounts[0]);
    }

    public void BloodMoonProgressSet()
    {
        if (gameManager.bloodMoonEventState)
        {
            var timeData = gameManager.BloodMoonProgressSet();
            sInvenManager.progressBar.SetMaxProgress(timeData.Item1);
            cooldown = timeData.Item1;
            prodTimer = timeData.Item2;
        }
        sInvenManager.WaveDiffLevelTextSet();
    }

    public void BloodMoonEventStart()
    {
        if (isUIOpened)
        {
            var timeData = gameManager.BloodMoonProgressSet();
            sInvenManager.progressBar.SetMaxProgress(timeData.Item1);
            cooldown = timeData.Item1;
            prodTimer = timeData.Item2;
        }
        bloodMoonEvent = true;
    }

    public override void CloseUI()
    {
        base.CloseUI();
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

            if (objName == "PortalItemIn")
            {
                PortalItemIn portalItemIn = obj.GetComponent<PortalItemIn>();
                portalItemIn.myPortal = this;

                GameObject othObj = otherPortal.ReturnObj("PortalItemOut");
                if (othObj)
                {
                    portalItemIn.ConnectObjServerRpc(othObj.GetComponent<NetworkObject>());
                }
            }
            else if (objName == "PortalItemOut")
            {
                PortalItemOut portalItemOut = obj.GetComponent<PortalItemOut>();
                portalItemOut.myPortal = this;

                GameObject othObj = otherPortal.ReturnObj("PortalItemIn");
                if (othObj)
                {
                    othObj.GetComponent<PortalItemIn>().ConnectObjServerRpc(obj.GetComponent<NetworkObject>());
                }
            }
            else if (objName == "PortalUnitIn")
            {
                PortalUnitIn portalUnitIn = obj.GetComponent<PortalUnitIn>();
                portalUnitIn.myPortal = this;

                GameObject othObj = otherPortal.ReturnObj("PortalUnitOut");
                if (othObj)
                {
                    portalUnitIn.ConnectObjServerRpc(othObj.GetComponent<NetworkObject>());
                }

                GameObject myObj = ReturnObj("PortalUnitOut");
                if (myObj)
                {
                    portalUnitIn.ConnectMyObjServerRpc(myObj.GetComponent<NetworkObject>());
                }
            }
            else if (objName == "PortalUnitOut")
            {
                PortalUnitOut portalUnitOut = obj.GetComponent<PortalUnitOut>();
                portalUnitOut.myPortal = this;

                GameObject othObj = otherPortal.ReturnObj("PortalUnitIn");
                if (othObj)
                {
                    othObj.GetComponent<PortalUnitIn>().ConnectObjServerRpc(obj.GetComponent<NetworkObject>());
                }

                GameObject myObj = ReturnObj("PortalUnitIn");
                if (myObj)
                {
                    myObj.GetComponent<PortalUnitIn>().ConnectMyObjServerRpc(obj.GetComponent<NetworkObject>());
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

    [ServerRpc]
    public void SetScienceBuildingServerRpc()
    {
        GameObject spawnobj = Instantiate(scienceBuilding, transform.position, Quaternion.identity);
        spawnobj.TryGetComponent(out NetworkObject netObj);
        if (!netObj.IsSpawned) spawnobj.GetComponent<NetworkObject>().Spawn(true);
        spawnobj.transform.parent = transform;
        spawnobj.GetComponent<ScienceBuilding>().SetPortal(isInHostMap);
    }

    public override void IncreasedStructureCheck() { }
}
