using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Pool;
using UnityEngine.Rendering;
using Unity.Netcode;
using System;
using Pathfinding;

// UTF-8 설정
public class Structure : NetworkBehaviour
{
    // 건물 공용 스크립트
    // Update처럼 함수 호출하는 부분은 다 하위 클래스에 넣을 것
    // 연결된 구조물 확인 방법 1. 콜라이더, 2. 맵에서 인접 타일 체크
    [SerializeField]
    public StructureData structureData;
    protected StructureData StructureData { set { structureData = value; } }

    [HideInInspector]
    public bool isFull = false;

    [HideInInspector]
    public int dirNum = 0;
    [HideInInspector]
    public int dirCount = 0;
    public int level = 0;
    public string buildName;

    [HideInInspector]
    public int height;
    [HideInInspector]
    public int width;
    [HideInInspector]
    public bool sizeOneByOne;

    [HideInInspector]
    public bool isPreBuilding = true;
    [HideInInspector]
    public bool isSetBuildingOk = false;

    protected bool removeState = false;

    [SerializeField]
    protected GameObject unitCanvas;

    public int maxLevel;

    [SerializeField]
    protected Image hpBar;
    public float maxHp;
    public float hp;
    protected RepairEffectFunc repairEffect;
    protected bool dieCheck = false;
    //[HideInInspector]
    //public bool isRuin = false;

    [HideInInspector]
    public bool isRepair = false;
    [SerializeField]
    protected Image repairBar;
    protected float repairGauge = 0.0f;

    [HideInInspector]
    public List<Item> itemList = new List<Item>();
    [HideInInspector]
    public List<ItemProps> itemObjList = new List<ItemProps>();

    [HideInInspector]
    public bool canBuilding = true;
    protected List<GameObject> buildingPosUnit = new List<GameObject>();

    public GameObject[] nearObj = new GameObject[4];
    public Vector2[] checkPos = new Vector2[4];
    public bool checkObj = true;

    protected Vector2[] startTransform;
    protected Vector3[] directions;
    protected int[] indices;

    protected Inventory playerInven = null;

    protected bool itemGetDelay = false;
    protected bool itemSetDelay = false;

    [HideInInspector]
    public List<GameObject> inObj = new List<GameObject>();
    [HideInInspector]
    public List<GameObject> outObj = new List<GameObject>();
    [HideInInspector]
    public List<GameObject> outSameList = new List<GameObject>();

    protected int getItemIndex = 0;
    protected int sendItemIndex = 0;

    protected Coroutine setFacDelayCoroutine; // 실행 중인 코루틴을 저장하는 변수

    [SerializeField]
    protected Sprite[] modelNum;
    protected SpriteRenderer setModel;

    public ItemProps spawnItem;

    [HideInInspector]
    public List<GameObject> monsterList = new List<GameObject>();

    [HideInInspector]
    public Collider2D col;
    //[HideInInspector]
    public RepairTower repairTower;
    [HideInInspector]
    public Overclock overclockTower;
    protected float effiOverclock;
    public float overclockAmount;

    public bool isStorageBuilding;
    public bool isMainSource;
    public bool isUIOpened;
    public bool isPortalBuild = false;

    public GameObject myVision;

    [HideInInspector]
    public int maxAmount;
    //[HideInInspector]
    public float cooldown;

    public List<EnergyGroupConnector> connectors;
    public EnergyGroupConnector conn;
    [HideInInspector]
    public bool energyUse;
    [HideInInspector]
    public bool isEnergyStr;
    [HideInInspector]
    public float energyProduction;
    [HideInInspector]
    public float energyConsumption;

    public bool isOperate;
    public float efficiency;
    public float effiCooldown;

    public bool isMainEnergyColony;
    public SoundManager soundManager;

    public bool isInHostMap;
    public Vector3 tileSetPos;

    public bool settingEndCheck = false;
    public List<(int, int)> DelaySendList = new List<(int, int)>();
    public List<int> DelayGetList = new List<int>();
    protected int buildingIndex;
    public List<Vector3> connectedPosList = new List<Vector3>();

    public delegate void OnHpChanged();
    public OnHpChanged onHpChangedCallback;

    protected virtual void Awake()
    {
        GameManager gameManager = GameManager.instance;
        playerInven = gameManager.inventory;
        buildName = structureData.FactoryName;
        col = GetComponent<BoxCollider2D>();
        maxLevel = structureData.MaxLevel;
        maxHp = structureData.MaxHp[level];
        hp = structureData.MaxHp[level];
        hpBar.fillAmount = hp / maxHp;
        repairBar.fillAmount = 0;
        isStorageBuilding = false;
        isMainSource = false;
        isUIOpened = false;
        myVision.SetActive(false);
        maxAmount = structureData.MaxItemStorageLimit;
        cooldown = structureData.Cooldown;
        connectors = new List<EnergyGroupConnector>();
        conn = null;
        efficiency = 0;
        effiCooldown = 0;
        energyUse = structureData.EnergyUse[level];
        isEnergyStr = structureData.IsEnergyStr;
        energyProduction = structureData.Production;
        energyConsumption = structureData.Consumption[level];
        soundManager = SoundManager.Instance;
        repairEffect = GetComponentInChildren<RepairEffectFunc>();
    }

    protected virtual void Update()
    {
        if (!removeState)
        {
            if (isRepair)
            {
                RepairFunc(false);
            }
            else if (isPreBuilding && isSetBuildingOk)
            {
                RepairFunc(true);
            }
        }
    }

    protected virtual void OnClientConnectedCallback(ulong clientId)
    {
        ClientConnectSyncServerRpc();
        ItemSyncServerRpc();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        NetworkObjManager.instance.NetObjAdd(gameObject);
        if (IsServer && !GetComponent<Portal>())
        {
            NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (IsServer && !GetComponent<Portal>())
        {
            NetworkManager.OnClientConnectedCallback -= OnClientConnectedCallback;
        }
    }

    public ulong ObjFindId() 
    {
        return GetComponent<NetworkObject>().NetworkObjectId;
    }

    [ServerRpc(RequireOwnership = false)]
    public virtual void ClientConnectSyncServerRpc()
    {
        ClientConnectSyncClientRpc(level, dirNum, height, width, isInHostMap, isSetBuildingOk, isPreBuilding);
        for (int i = 0; i < nearObj.Length; i++)
        {
            if (nearObj[i] == null)
                continue;

            ulong objID = nearObj[i].GetComponent<Structure>().ObjFindId();
            NearObjSyncClientRpc(objID, i);
        }

        for (int i = 0; i < outObj.Count; i++)
        {
            ulong objID = outObj[i].GetComponent<Structure>().ObjFindId();
            InOutObjSyncClientRpc(objID, false);
        }

        for (int i = 0; i < inObj.Count; i++)
        {
            ulong objID = inObj[i].GetComponent<Structure>().ObjFindId();
            InOutObjSyncClientRpc(objID, true);
        }
        MapDataSaveClientRpc(tileSetPos);
        ConnectCheckClientRpc(true);
    }

    [ServerRpc(RequireOwnership = false)]
    public virtual void ItemSyncServerRpc()
    {
        ItemListClearClientRpc();
        for (int i = 0; i < itemList.Count; i++)
        {
            int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(itemList[i]);
            ItemSyncClientRpc(itemIndex);
        }
    }

    [ClientRpc]
    public void ConnectCheckClientRpc(bool isEnd)
    {
        settingEndCheck = isEnd;
    }
     
    [ClientRpc]
    public virtual void ClientConnectSyncClientRpc(int syncLevel, int syncDir, int syncHeight, int syncWidth, bool syncMap, bool syncSetBuilding, bool syncPreBuilding)
    {
        if (IsServer)
            return;

        level = syncLevel;
        dirNum = syncDir;
        height = syncHeight;
        width = syncWidth;
        isInHostMap = syncMap;
        isSetBuildingOk = syncSetBuilding;
        isPreBuilding = syncPreBuilding;
        ColliderTriggerOnOff(false);
        gameObject.AddComponent<DynamicGridObstacle>();
        myVision.SetActive(true);
        CheckPos();
    }

    [ClientRpc]
    void NearObjSyncClientRpc(ulong ObjID, int index, ClientRpcParams rpcParams = default)
    {
        if (IsServer)
            return;

        NetworkObject obj = NetworkObjManager.instance.FindNetworkObj(ObjID);

        nearObj[index] = obj.gameObject;
    }

    [ClientRpc]
    void InOutObjSyncClientRpc(ulong ObjID, bool isIn, ClientRpcParams rpcParams = default)
    {
        if (IsServer)
            return;

        NetworkObject obj = NetworkObjManager.instance.FindNetworkObj(ObjID);

        if(isIn)
            inObj.Add(obj.gameObject);
        else
            outObj.Add(obj.gameObject);
    }

    [ClientRpc]
    protected virtual void ItemListClearClientRpc()
    {
        if (!IsServer)
            itemList.Clear();
    }

    [ClientRpc]
    protected virtual void ItemSyncClientRpc(int itemIndex, ClientRpcParams rpcParams = default)
    {
        if (IsServer)
            return;

        Item item = GeminiNetworkManager.instance.GetItemSOFromIndex(itemIndex);
        itemList.Add(item);
    }

    protected virtual void DataSet()
    {
        hp = structureData.MaxHp[level];
        hpBar.fillAmount = hp / structureData.MaxHp[level];
        energyUse = structureData.EnergyUse[level];
        energyConsumption = structureData.Consumption[level];
    }

    public virtual void Focused() { }

    public virtual void DisableFocused() { }

    public virtual bool CheckOutItemNum() { return new bool(); }

    public void BuildingSetting(int _level, int _height, int _width, int _dirCount)
    {
        isPreBuilding = true;
        ColliderTriggerOnOff(true);
        level = _level - 1;
        height = _height;
        width = _width;
        dirCount = _dirCount;
    }

    protected virtual void SetDirNum()
    {
        setModel.sprite = modelNum[dirNum];
        CheckPos();
    }

    // 건물의 방향 설정
    protected virtual void CheckPos()
    {
        if (width == 1 && height == 1)
        {
            sizeOneByOne = true;
            Vector2[] dirs = { Vector2.up, Vector2.right, Vector2.down, Vector2.left };

            for (int i = 0; i < 4; i++)
            {
                checkPos[i] = dirs[(dirNum + i) % 4];
            }
        }
        else if (width == 2 && height == 1)
        {
            sizeOneByOne = false;
            nearObj = new GameObject[6];
            indices = new int[] { 1, 0, 0, 0, 1, 1 };
            startTransform = new Vector2[] { new Vector2(0.5f, -1f), new Vector2(-0.5f, -1f) };
            directions = new Vector3[] { transform.up, transform.right, -transform.up, -transform.right };

        }
        else if (width == 2 && height == 2)
        {
            sizeOneByOne = false;
            nearObj = new GameObject[8];
            indices = new int[] { 3, 0, 0, 1, 1, 2, 2, 3 };
            startTransform = new Vector2[] { new Vector2(0.5f, 0.5f), new Vector2(0.5f, -0.5f), new Vector2(-0.5f, -0.5f), new Vector2(-0.5f, 0.5f) };
            directions = new Vector3[] { transform.up, transform.right, -transform.up, -transform.right };
        }
    }

    // 1x1 사이즈 근처 오브젝트 찻는 위치(상하좌우) 설정
    protected virtual void CheckNearObj(Vector2 direction, int index, Action<GameObject> callback)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, 1f);

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hitCollider = hits[i].collider;

            if (GetComponent<LogisticsCtrl>() || (GetComponent<Production>() && !GetComponent<FluidFactoryCtrl>()))
            {
                if (hitCollider.GetComponent<FluidFactoryCtrl>() && !hitCollider.GetComponent<Refinery>() && !hitCollider.GetComponent<SteamGenerator>())
                {
                    continue;
                }
            }
            else if (GetComponent<FluidFactoryCtrl>() && !GetComponent<Refinery>() && !GetComponent<SteamGenerator>())
            {
                if (hitCollider.GetComponent<LogisticsCtrl>())
                {
                    continue;
                }
            }

            if (hitCollider.CompareTag("Factory") && hitCollider.GetComponent<Structure>().isSetBuildingOk &&
                hits[i].collider.gameObject != this.gameObject)
            {
                nearObj[index] = hits[i].collider.gameObject;
                callback(hitCollider.gameObject);
                break;
            }
        }
    }

    // 2x2 사이즈 근처 오브젝트 찻는 위치(상하좌우) 설정
    protected virtual void CheckNearObj(Vector3 startVec, Vector3 endVec, int index, Action<GameObject> callback)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(this.transform.position + startVec, endVec, 1f);

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hitCollider = hits[i].collider;
            if (hitCollider.CompareTag("Factory") && hitCollider.GetComponent<Structure>().isSetBuildingOk &&
                hits[i].collider.gameObject != this.gameObject)
            {
                nearObj[index] = hits[i].collider.gameObject;
                callback(hitCollider.gameObject);
                break;
            }
        }
    }

    public virtual void SetBuild()
    {
        isPreBuilding = true;
        unitCanvas.SetActive(true);
        hpBar.enabled = false;
        repairBar.enabled = true;
        repairGauge = 0;
        repairBar.fillAmount = repairGauge / structureData.MaxBuildingGauge;
        isSetBuildingOk = true;
    }
    // 건물 설치 기능

    [ClientRpc]
    public virtual void SettingClientRpc(int _level, int _beltDir, int objHeight, int objWidth, bool isHostMap, int index)
    {
        level = _level;
        dirNum = _beltDir;
        height = objHeight;
        width = objWidth;
        buildingIndex = index;
        isInHostMap = isHostMap;
        settingEndCheck = true;
        SetBuild();
        ColliderTriggerOnOff(false);
        gameObject.AddComponent<DynamicGridObstacle>();
        myVision.SetActive(true);
        DataSet();
    }

    public virtual void GameStartSpawnSet(int _level, int _beltDir, int objHeight, int objWidth, bool isHostMap, int index)
    {
        level = _level;
        dirNum = _beltDir;
        height = objHeight;
        width = objWidth;
        buildingIndex = index;
        isInHostMap = isHostMap;
        settingEndCheck = true;
        isPreBuilding = true;
        isSetBuildingOk = true;
        ColliderTriggerOnOff(false);
        gameObject.AddComponent<DynamicGridObstacle>();
        myVision.SetActive(true);
        DataSet();
    }

    public void ConnectedPosListPosSet(Vector3 pos)
    {
        connectedPosList.Add(pos);
    }

    public virtual void GameStartRecipeSet(int recipeId) { }

    public void GameStartItemSet(int itemIndex)
    {
        Item item = GeminiNetworkManager.instance.GetItemSOFromIndex(itemIndex);
        itemList.Add(item);
        if (itemList.Count >= structureData.MaxItemStorageLimit)
        {
            isFull = true;
        }
    }

    //[ClientRpc]
    //public void TempMinerSetClientRpc()
    //{
    //    isTempBuild = true;
    //}

    public virtual void OnFactoryItem(ItemProps itemProps)
    {
        itemProps.itemPool.Release(itemProps.gameObject);
    }

    public virtual void OnFactoryItem(Item item) { }

    [ServerRpc]
    protected void OnFactoryItemServerRpc(int itemIndex)
    {
        OnFactoryItemClientRpc(itemIndex);
    }

    [ClientRpc]
    protected void OnFactoryItemClientRpc(int itemIndex)
    {
        Item item = GeminiNetworkManager.instance.GetItemSOFromIndex(itemIndex);
        itemList.Add(item);

        if (itemList.Count >= structureData.MaxItemStorageLimit)
        {
            isFull = true;
        }
    }

    [ClientRpc]
    protected void itemListRemoveClientRpc()
    {
        itemList.RemoveAt(0);
    }

    public virtual void ItemNumCheck() { }

    protected virtual IEnumerator UnderBeltConnectCheck(GameObject game)
    {
        yield return new WaitForSeconds(0.1f);

        if (game.TryGetComponent(out GetUnderBeltCtrl getUnder))
        {
            if (!getUnder.outObj.Contains(this.gameObject))
            {
                inObj.Remove(game);
            }
            if (!getUnder.inObj.Contains(this.gameObject))
            {
                outObj.Remove(game);
                outSameList.Remove(game);
            }
        }
        else if (game.TryGetComponent(out SendUnderBeltCtrl sendUnder))
        {
            if (!sendUnder.inObj.Contains(this.gameObject))
            {
                outObj.Remove(game);
                outSameList.Remove(game);
            }
            if (!sendUnder.outObj.Contains(this.gameObject))
            {
                inObj.Remove(game);
            }
        }
        checkObj = true;
    }

    [ClientRpc]
    protected virtual void InOutObjIndexResetClientRpc(bool isGetObj)
    {
        if (!IsServer)
            return;

        if (isGetObj)
            getItemIndex = 0;
        else
            sendItemIndex = 0;
    }

    protected virtual void GetItemIndexSet()
    {
        getItemIndex++;
        if (getItemIndex >= inObj.Count)
            getItemIndex = 0;
    }

    protected virtual void GetItem()
    {
        itemGetDelay = true;

        if (inObj[getItemIndex].TryGetComponent(out BeltCtrl belt))
        {
            if (!belt.isItemStop)
            {
                GetItemIndexSet();
                Invoke(nameof(DelayGetItem), structureData.SendDelay);
                return;
            }
            else if (TryGetComponent(out Production production) && belt.itemObjList.Count > 0 && !production.CanTakeItem(belt.itemObjList[0].item))
            {
                GetItemIndexSet();
                Invoke(nameof(DelayGetItem), structureData.SendDelay);
                return;
            }
        }
        else if (TryGetComponent(out Unloader unloader) && inObj[getItemIndex].TryGetComponent(out Production inObjScript) && !inObjScript.UnloadItemCheck(unloader.selectItem))
        {
            GetItemIndexSet();
            Invoke(nameof(DelayGetItem), structureData.SendDelay);
            return;
        }
        else if (!GetComponent<Unloader>() && inObj[getItemIndex].GetComponent<Structure>())
        {
            GetItemIndexSet();
            Invoke(nameof(DelayGetItem), structureData.SendDelay);
            return;
        }

        GetItemServerRpc(getItemIndex);
        GetItemIndexSet();
    }


    [ServerRpc]
    protected virtual void GetItemServerRpc(int inObjIndex)
    {
        GetItemClientRpc(inObjIndex);
    }

    [ClientRpc]
    protected virtual void GetItemClientRpc(int inObjIndex)
    {
        if (IsServer)
        {
            GetItemFunc(inObjIndex);
        }
        else if (settingEndCheck)
        {
            GetDelaySet(inObjIndex);
        }
    }

    protected virtual void GetItemFunc(int inObjIndex)
    {
        if (inObj[inObjIndex].TryGetComponent(out BeltCtrl belt))
        {
            OnFactoryItem(belt.itemObjList[0]);
            belt.itemObjList[0].transform.position = this.transform.position;
            belt.isItemStop = false;
            belt.itemObjList.RemoveAt(0);
            if (IsServer)
                belt.beltGroupMgr.groupItem.RemoveAt(0);
            belt.ItemNumCheck();
            DelayGetItem();
            //Invoke(nameof(DelayGetItem), structureData.SendDelay);
        }
    }

    protected void GetDelaySet(int inObjIndex)
    {
        DelayGetList.Add(inObjIndex);
    }

    protected void GetDelayFunc(int inObjIndex, int listIndex)
    {
        GetItemFunc(inObjIndex);
        DelayGetList.RemoveAt(listIndex);
    }

    protected virtual void SendItem(int itemIndex)
    {
        itemSetDelay = true;

        Structure outFactory = outObj[sendItemIndex].GetComponent<Structure>();

        if (outFactory.isFull)
        {
            SendItemIndexSet();
            itemSetDelay = false;
            return;
        }
        else if (outFactory.TryGetComponent(out Production production))
        {
            Item item = GeminiNetworkManager.instance.GetItemSOFromIndex(itemIndex);
            if (!production.CanTakeItem(item))
            {
                SendItemIndexSet();
                itemSetDelay = false;
                return;
            }
        }
        else if (outFactory.isMainSource)
        {
            SendItemIndexSet();
            itemSetDelay = false;
            return;
        }

        SendItemServerRpc(itemIndex, sendItemIndex);

        SendItemIndexSet();
    }

    protected virtual void SendItemIndexSet()
    {
        sendItemIndex++;
        if (sendItemIndex >= outObj.Count)
            sendItemIndex = 0;
    }

    [ServerRpc]
    protected virtual void SendItemServerRpc(int itemIndex, int outObjIndex)
    {
        SendItemClientRpc(itemIndex, outObjIndex);
    }

    [ClientRpc]
    protected virtual void SendItemClientRpc(int itemIndex, int outObjIndex)
    {
        if (IsServer)
        {
            SendItemFunc(itemIndex, outObjIndex);
        }
        else if (settingEndCheck)
        {
            SendDelaySet(itemIndex, outObjIndex);
        }
    }

    protected virtual void SendItemFunc(int itemIndex, int outObjIndex)
    {
        Item item = GeminiNetworkManager.instance.GetItemSOFromIndex(itemIndex);

        Structure outFactory = outObj[outObjIndex].GetComponent<Structure>();

        if (outObj[outObjIndex].TryGetComponent(out BeltCtrl beltCtrl))
        {
            var itemPool = ItemPoolManager.instance.Pool.Get();
            ItemProps spawnItem = itemPool.GetComponent<ItemProps>();
            if (beltCtrl.OnBeltItem(spawnItem))
            {
                SpriteRenderer sprite = spawnItem.GetComponent<SpriteRenderer>();
                spawnItem.col.enabled = false;
                sprite.sprite = item.icon;
                sprite.sortingOrder = 2;
                spawnItem.item = item;
                spawnItem.amount = 1;
                spawnItem.transform.position = transform.position;
                spawnItem.isOnBelt = true;
                spawnItem.setOnBelt = beltCtrl.GetComponent<BeltCtrl>();

                if (GetComponent<Production>())
                {
                    SubFromInventory();
                }
                else if (GetComponent<LogisticsCtrl>() && !GetComponent<ItemSpawner>() && !GetComponent<Unloader>())
                {
                    itemList.RemoveAt(0);
                    ItemNumCheck();
                }
            }
        }
        else if (!outFactory.isMainSource)
        {
            if (outObj[outObjIndex].GetComponent<LogisticsCtrl>())
                setFacDelayCoroutine = StartCoroutine(SendFacDelayArguments(outObj[outObjIndex], item));
            else if (outObj[outObjIndex].TryGetComponent(out Production production) && production.CanTakeItem(item))
                setFacDelayCoroutine = StartCoroutine(SendFacDelayArguments(outObj[outObjIndex], item));
        }

        Invoke(nameof(DelaySetItem), structureData.SendDelay);
    }
    
    protected void SendDelaySet(int itemIndex, int outObjIndex)
    {
        DelaySendList.Add((itemIndex, outObjIndex));
    }

    protected void SendDelayFunc(int itemIndex, int outObjIndex, int listIndex)
    {
        SendItemFunc(itemIndex, outObjIndex);
        DelaySendList.RemoveAt(listIndex);
    }

    protected IEnumerator SendFacDelayArguments(GameObject game, Item item)
    {
        yield return StartCoroutine(SendFacDelay(game, item));
    }

    protected virtual IEnumerator SendFacDelay(GameObject outFac, Item item)
    {
        var itemPool = ItemPoolManager.instance.Pool.Get();
        ItemProps spawnItem = itemPool.GetComponent<ItemProps>();
        spawnItem.col.enabled = false;
        SpriteRenderer sprite = spawnItem.GetComponent<SpriteRenderer>();
        sprite.color = new Color(1f, 1f, 1f, 0f);
        CircleCollider2D coll = spawnItem.GetComponent<CircleCollider2D>();
        coll.enabled = false;

        spawnItem.transform.position = this.transform.position;

        var targetPos = outFac.transform.position;
        var startTime = Time.time;
        var distance = Vector3.Distance(spawnItem.transform.position, targetPos);

        while (spawnItem != null && spawnItem.transform.position != targetPos)
        {
            var elapsed = Time.time - startTime;
            var t = Mathf.Clamp01(elapsed / (distance / structureData.SendSpeed[0]));

            spawnItem.transform.position = Vector3.Lerp(spawnItem.transform.position, targetPos, t);

            yield return null;
        }

        if (spawnItem != null && spawnItem.transform.position == targetPos)
        {
            if (CanSendItemCheck())
            {
                if (checkObj && outObj.Count > 0 && outFac != null)
                {
                    if (outFac.TryGetComponent(out Structure outFactory))
                    {
                        outFactory.OnFactoryItem(item);
                    }
                }
                else
                {
                    sprite.color = new Color(1f, 1f, 1f, 1f);
                    coll.enabled = true;
                    spawnItem.itemPool.Release(itemPool);
                    spawnItem = null;
                }
                if (GetComponent<LogisticsCtrl>() && !GetComponent<ItemSpawner>())
                {
                    itemListRemoveClientRpc();
                    ItemNumCheck();
                }
                else if (GetComponent<Production>())
                {
                    SubFromInventory();
                }
            }
        }

        if (spawnItem != null)
        {
            sprite.color = new Color(1f, 1f, 1f, 1f);
            coll.enabled = true;
            setFacDelayCoroutine = null;
            spawnItem.itemPool.Release(itemPool);
        }
        else
        {
            setFacDelayCoroutine = null;
        }
    }

    bool CanSendItemCheck()
    {
        if (GetComponent<LogisticsCtrl>())
        {
            if (GetComponent<ItemSpawner>())
                return true;
            else if (itemList.Count > 0)
                return true;
        }
        else if (GetComponent<Production>() && CheckOutItemNum())
        {
            return true;
        }

        return false;
    }

    protected virtual void SubFromInventory() { }

    public void TakeDamage(float damage)
    {
        if(!dieCheck)
            TakeDamageServerRpc(damage);
    }

    [ServerRpc]
    public void TakeDamageServerRpc(float damage)
    {
        TakeDamageClientRpc(damage);
    }

    [ClientRpc]
    public void TakeDamageClientRpc(float damage)
    {
        if (!isPreBuilding)
        {
            if (!unitCanvas.activeSelf)
            {
                unitCanvas.SetActive(true);
                hpBar.enabled = true;
            }
        }

        if (hp <= 0f)
            return;

        hp -= damage;
        if (hp < 0f)
            hp = 0f;
        onHpChangedCallback?.Invoke();
        hpBar.fillAmount = hp / structureData.MaxHp[level];

        if (IsServer && hp <= 0f)
        {
            hp = 0f;
            DieFuncClientRpc();
        }
    }

    [ClientRpc]
    protected virtual void DieFuncClientRpc()
    {
        //hp = structureData.MaxHp[level];
        //repairBar.enabled = true;
        hpBar.enabled = false;
        dieCheck = true;
        //repairGauge = 0;
        //repairBar.fillAmount = repairGauge / structureData.MaxBuildingGauge;
        ColliderTriggerOnOff(true);

        soundManager.PlaySFX(gameObject, "structureSFX", "Destory");

        if (!IsServer)
            return;

        if (!isPreBuilding)
        {
            foreach (GameObject monster in monsterList)
            {
                if (monster.TryGetComponent(out MonsterAi monsterAi))
                {
                    monsterAi.RemoveTarget(this.gameObject);
                }
            }
        }
        else
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(this.transform.position, structureData.ColliderRadius);

            foreach (Collider2D collider in colliders)
            {
                GameObject monster = collider.gameObject;
                if (monster.CompareTag("Monster") || monster.CompareTag("Spawner"))
                {
                    if (!monsterList.Contains(monster))
                    {
                        monsterList.Add(monster);
                    }
                }
            }
            foreach (GameObject monsterObj in monsterList)
            {
                if (monsterObj.TryGetComponent(out MonsterAi monsterAi))
                {
                    monsterAi.RemoveTarget(this.gameObject);
                }
            }
        }
        monsterList.Clear();

        ItemDrop();
        RemoveObjServerRpc();
    }

    protected void ItemToItemProps(Item item, int itemAmount)
    {
        var itemPool = ItemPoolManager.instance.Pool.Get();
        ItemProps itemProps = itemPool.GetComponent<ItemProps>();

        SpriteRenderer sprite = itemProps.GetComponent<SpriteRenderer>();
        sprite.sprite = item.icon;
        sprite.sortingOrder = 2;
        itemProps.item = item;
        itemProps.amount = itemAmount;
        itemProps.transform.position = transform.position;
        itemProps.ResetItemProps();

        NetworkObject itemNetworkObject = itemProps.GetComponent<NetworkObject>();
        itemNetworkObject.Spawn(true);        
    }

    protected virtual void ItemDrop() { }

    public void RepairFunc(float heal)
    {
        if (hp == structureData.MaxHp[level])
        {
            return;
        }
        else if (hp + heal > structureData.MaxHp[level])
        {
            hp = structureData.MaxHp[level];
            if (!isRepair)
                unitCanvas.SetActive(false);
        }
        else
        {
            hp += heal;
            RepairServerRpc(hp);
        }

        onHpChangedCallback?.Invoke();
        hpBar.fillAmount = hp / structureData.MaxHp[level];
    }

    [ServerRpc]
    void RepairServerRpc(float currentHp)
    {
        RepairClientRpc(currentHp);
    }

    [ClientRpc]
    void RepairClientRpc(float currentHp)
    {
        hp = currentHp;
        repairEffect.EffectStart();
    }

    public void RepairSet(bool repair)
    {
        hp = structureData.MaxHp[level];
        isRepair = repair;
    }

    protected void RepairFunc(bool isBuilding)
    {
        repairGauge += 10.0f * Time.deltaTime;

        if (isBuilding)
        {
            repairBar.fillAmount = repairGauge / structureData.MaxBuildingGauge;
            if (repairGauge >= structureData.MaxBuildingGauge)
            {
                isPreBuilding = false;
                repairGauge = 0.0f;
                repairBar.enabled = false;
                if (hp < structureData.MaxHp[level])
                {
                    unitCanvas.SetActive(true);
                    hpBar.enabled = true;
                }
                else
                {
                    unitCanvas.SetActive(false);
                }

                ColliderTriggerOnOff(false);
            }
        }
        else
        {
            repairBar.fillAmount = repairGauge / structureData.MaxRepairGauge;
            if (repairGauge >= structureData.MaxRepairGauge)
            {
                RepairEnd();
            }
        }
    }

    protected virtual void RepairEnd()
    {
        hpBar.enabled = true;
        hp = structureData.MaxHp[level];
        Debug.Log("DataSet : " + hp);
        unitCanvas.SetActive(false);
        hpBar.fillAmount = hp / structureData.MaxHp[level];
        repairBar.enabled = false;
        repairGauge = 0.0f;
        isPreBuilding = false;
        ColliderTriggerOnOff(false);
    }

    public virtual void ColliderTriggerOnOff(bool isOn)
    {
        if (isOn)
            col.isTrigger = true;
        else
            col.isTrigger = false;
    }

    protected void RemoveSameOutList()
    {
        outSameList.Clear();
    }

    protected virtual IEnumerator SetInObjCoroutine(GameObject obj)
    {
        checkObj = false;
        yield return new WaitForSeconds(0.1f);

        if (obj.GetComponent<Structure>() != null)
        {
            if (obj.TryGetComponent(out BeltCtrl belt))
            {
                if (belt.GetComponentInParent<BeltGroupMgr>().nextObj != this.gameObject)
                {
                    checkObj = true;
                    yield break;
                }
                belt.FactoryPosCheck(GetComponentInParent<Structure>());
            }
            inObj.Add(obj);
            StartCoroutine(UnderBeltConnectCheck(obj));
        }
    }

    protected virtual IEnumerator SetOutObjCoroutine(GameObject obj)
    {
        checkObj = false;
        yield return new WaitForSeconds(0.1f);

        if (obj.GetComponent<WallCtrl>())
        {
            Debug.Log("wall");
            yield break;

        }

        if (obj.GetComponent<Structure>() != null)
        {
            if ((obj.GetComponent<ItemSpawner>() && GetComponent<ItemSpawner>())
                || obj.GetComponent<Unloader>())
            {
                checkObj = true;
                yield break;
            }

            if (obj.TryGetComponent(out BeltCtrl belt))
            {
                if (obj.GetComponentInParent<BeltGroupMgr>().nextObj == this.gameObject)
                {
                    checkObj = true;
                    yield break;
                }
                belt.FactoryPosCheck(GetComponentInParent<Structure>());
            }
            else
            {
                outSameList.Add(obj);
                StartCoroutine(OutCheck(obj));
            }
            outObj.Add(obj);
            StartCoroutine(UnderBeltConnectCheck(obj));
        }
    }

    protected virtual IEnumerator OutCheck(GameObject otherObj)
    {
        yield return new WaitForSeconds(0.1f);

        if (otherObj.TryGetComponent(out Structure otherFacCtrl))
        {
            if (otherObj.GetComponent<Production>())
                yield break;

            if (otherFacCtrl.outSameList.Contains(this.gameObject) && outSameList.Contains(otherObj))
            {
                StopCoroutine("SendFacDelay");
                outObj.Remove(otherObj);
                Invoke(nameof(RemoveSameOutList), 0.1f);
                InOutObjIndexResetClientRpc(false);
            }
        }
    }

    public virtual void ResetCheckObj(GameObject game)
    {
        checkObj = false;

        if (inObj.Contains(game))
        {
            inObj.Remove(game);
            InOutObjIndexResetClientRpc(true);
        }
        if (outObj.Contains(game))
        {
            outObj.Remove(game);
            InOutObjIndexResetClientRpc(false);
        }

        for (int i = 0; i < nearObj.Length; i++)
        {
            if (nearObj[i] != null && nearObj[i] == game)
            {
                nearObj[i] = null;
            }
        }

        checkObj = true;
    }

    [ServerRpc(RequireOwnership = false)]
    public virtual void RemoveObjServerRpc()
    {
        RemoveObjClientRpc();
    }


    [ClientRpc]
    void RemoveObjClientRpc()
    {
        removeState = true;
        ColliderTriggerOnOff(true);
        StopAllCoroutines();

        if (InfoUI.instance.str == this)
            InfoUI.instance.SetDefault();

        for (int i = 0; i < nearObj.Length; i++)
        {
            if (nearObj[i] != null && nearObj[i].TryGetComponent(out Structure structure))
            {
                structure.ResetCheckObj(gameObject);
                if (structure.GetComponentInParent<BeltGroupMgr>())
                {
                    BeltGroupMgr beltGroup = structure.GetComponentInParent<BeltGroupMgr>();
                    beltGroup.nextCheck = true;
                    beltGroup.preCheck = true;
                }
            }
        }
        if (repairTower != null)
            repairTower.RemoveObjectsOutOfRange(gameObject);
        if(overclockTower != null)
            overclockTower.RemoveObjectsOutOfRange(this);

        CloseUI();

        if (IsServer && GetComponent<BeltCtrl>() && GetComponentInParent<BeltManager>() && GetComponentInParent<BeltGroupMgr>())
        {
            BeltManager beltManager = GetComponentInParent<BeltManager>();
            BeltGroupMgr beltGroup = GetComponentInParent<BeltGroupMgr>();
            beltManager.BeltDivide(beltGroup, gameObject);
        }
        else if (TryGetComponent(out FluidFactoryCtrl fluid))
        {
            fluid.RemoveMainSource(true);
        }
        else if (TryGetComponent(out Transporter trBuild))
        {
            trBuild.RemoveFunc();
        }
        else if (TryGetComponent(out PortalObj portalObj))
        {
            portalObj.RemovePortalData();
        }

        GameManager gameManager = GameManager.instance;
        int x = Mathf.FloorToInt(transform.position.x);
        int y = Mathf.FloorToInt(transform.position.y);
        Cell cell = gameManager.map.GetCellDataFromPos(x, y);
        bool isOnMap = gameManager.map.IsOnMap(x, y);

        if (sizeOneByOne)
        {
            if (isOnMap && cell.structure == gameObject)
            {
                cell.structure = null;
            }
        }
        else
        {
            if (isOnMap && cell.structure == gameObject)
            {
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        gameManager.map.GetCellDataFromPos(x + j, y + i).structure = null;
                    }
                }
            }
        }

        NetworkObjManager.instance.NetObjRemove(gameObject);

        if (IsServer && NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn();
        }
        //Destroy(this.gameObject);
    }

    void CloseUI()
    {
        if (TryGetComponent(out LogisticsClickEvent solidFacClickEvent))
        {
            if (solidFacClickEvent.LogisticsUI != null)
            {
                if (solidFacClickEvent.LogisticsUI.activeSelf)
                {
                    if (solidFacClickEvent.sFilterManager != null)
                        solidFacClickEvent.sFilterManager.CloseUI();
                    else if (solidFacClickEvent.itemSpManager != null)
                        solidFacClickEvent.itemSpManager.CloseUI();
                }
            }
        }
        else if (TryGetComponent(out StructureClickEvent structureClickEvent))
        {
            if (structureClickEvent.structureInfoUI != null)
            {
                if (structureClickEvent.structureInfoUI.activeSelf)
                {
                    if (structureClickEvent.sInvenManager != null)
                    {
                        structureClickEvent.sInvenManager.ClearInvenOption();
                        structureClickEvent.sInvenManager.CloseUI();
                    }
                }
            }
            if (TryGetComponent(out Transporter trBuild))
            {
                if (trBuild.lineRenderer != null)
                    Destroy(trBuild.lineRenderer);
            }
            else if (TryGetComponent(out UnitFactory unitFactory))
            {
                if (unitFactory.lineRenderer != null)
                    Destroy(unitFactory.lineRenderer);
            }
        }
    }

    public virtual void AddInvenItem() 
    {
        if (isInHostMap)
            playerInven = GameManager.instance.hostMapInven;
        else
            playerInven = GameManager.instance.clientMapInven;
    }

    protected void DelaySetItem()
    {
        itemSetDelay = false;
    }

    protected void DelayGetItem()
    {
        itemGetDelay = false;
    }

    public virtual Dictionary<Item, int> PopUpItemCheck() { return null; }

    public virtual (bool, bool , EnergyGroup) PopUpEnergyCheck()
    {
        if (energyUse && conn != null && conn.group != null)
        {
            return (energyUse, isEnergyStr, conn.group);
        }

        return (false, false, null);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<UnitCommonAi>() || collision.GetComponent<PlayerController>())
        {
            if (isPreBuilding)
            {
                buildingPosUnit.Add(collision.gameObject);

                if (buildingPosUnit.Count > 0)
                {
                    if (!collision.GetComponentInParent<PreBuilding>())
                    {
                        canBuilding = false;
                    }

                    PreBuilding preBuilding = GetComponentInParent<PreBuilding>();
                    if (preBuilding != null)
                    {
                        preBuilding.isBuildingOk = false;
                    }
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<UnitCommonAi>() || collision.GetComponent<PlayerController>())
        {
            if (isPreBuilding)
            {
                buildingPosUnit.Remove(collision.gameObject);
                if (buildingPosUnit.Count > 0)
                    canBuilding = false;
                else
                {
                    canBuilding = true;

                    PreBuilding preBuilding = GetComponentInParent<PreBuilding>();
                    if (preBuilding != null)
                        preBuilding.isBuildingOk = true;
                }
            }
        }
    }

    public virtual void AddConnector(EnergyGroupConnector connector)
    {
        if (!connectors.Contains(connector))
        {
            connectors.Add(connector);
            if (conn == null)
            {
                conn = connector;
                conn.AddConsumption(this);
            }
        }
    }

    public virtual void RemoveConnector(EnergyGroupConnector connector)
    {
        if (connectors.Contains(connector))
        {
            connectors.Remove(connector);
            if (conn == connector)
            {
                conn.RemoveConsumption(this);
                conn = null;
                if (connectors.Count > 0)
                {
                    conn = connectors[0];
                    conn.AddConsumption(this);
                }
            }
        }
    }

    public void OverclockSet(bool isOn)
    {
        if (isOn)
        {
            effiOverclock = effiCooldown * overclockAmount / 100;
        }
        else
        {
            effiOverclock = 0;
        }
    }

    public virtual void EfficiencyCheck() { }

    [ClientRpc]
    public void MapDataSaveClientRpc(Vector3 pos)
    {
        tileSetPos = pos;

        int x = Mathf.FloorToInt(tileSetPos.x);
        int y = Mathf.FloorToInt(tileSetPos.y);
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                if (isInHostMap)
                    GameManager.instance.hostMap.GetCellDataFromPos(x + j, y + i).structure = this.gameObject;
                else
                    GameManager.instance.clientMap.GetCellDataFromPos(x + j, y + i).structure = this.gameObject;
            }
        }
    }

    public virtual StructureSaveData SaveData()
    {
        StructureSaveData data = new StructureSaveData();
        data.index = buildingIndex;

        data.pos = Vector3Extensions.FromVector3(transform.position);
        data.tileSetPos = Vector3Extensions.FromVector3(tileSetPos);
        data.hp = hp;
        data.planet = isInHostMap;
        data.level = level;
        data.direction = dirNum;
        foreach (Item items in itemList)
        {
            int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(items);
            data.itemIndex.Add(itemIndex);
        }

        return data;
    }
}
