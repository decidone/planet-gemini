using Pathfinding;
using Pathfinding.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

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
    protected bool removeState = false;

    [SerializeField]
    protected GameObject unitCanvas;

    public int maxLevel;

    [SerializeField]
    protected Image hpBar;
    public float maxHp;
    public float hp;
    protected float defense;
    protected RepairEffectFunc repairEffect;
    protected bool dieCheck = false;
    public GameObject RuinExplo;
    bool damageEffectOn;
    protected SpriteRenderer unitSprite;

    //[HideInInspector]
    //public bool isRuin = false;

    [HideInInspector]
    public bool isRepair = false;
    [SerializeField]
    protected Image repairBar;
    protected float repairGauge = 0.0f;

    //[HideInInspector]
    public List<Item> itemList = new List<Item>();
    //[HideInInspector]
    public List<ItemProps> itemObjList = new List<ItemProps>();

    [HideInInspector]
    public bool canBuilding = true;
    protected List<GameObject> buildingPosUnit = new List<GameObject>();

    public GameObject[] nearObj = new GameObject[4];
    public Vector2[] checkPos = new Vector2[4];

    protected Vector2[] startTransform;
    protected Vector3[] directions;
    protected int[] indices;

    protected Inventory playerInven = null;

    protected bool itemGetDelay = false;
    protected bool itemSetDelay = false;

    protected float getDelay;
    protected float sendDelay;
    [HideInInspector]
    public bool takeItemDelay = false;

    //[HideInInspector]
    public List<GameObject> inObj = new List<GameObject>();
    //[HideInInspector]
    public List<GameObject> outObj = new List<GameObject>();
    [HideInInspector]
    public List<GameObject> outSameList = new List<GameObject>();

    protected int getItemIndex = 0;
    protected int sendItemIndex = 0;

    //protected Coroutine setFacDelayCoroutine; // 실행 중인 코루틴을 저장하는 변수

    [SerializeField]
    protected Sprite[] modelNum;
    protected SpriteRenderer setModel;

    public ItemProps spawnItem;

    public List<GameObject> monsterList = new List<GameObject>();

    [HideInInspector]
    public Collider2D col;
    [HideInInspector]
    public RepairTower repairTower;
    [HideInInspector]
    public Overclock overclockTower;

    [SerializeField]
    protected bool overclockOn;
    [SerializeField]
    protected float overclockPer;
    protected float effiCooldownUpgradeAmount;
    [SerializeField]
    protected float effiCooldownUpgradePer;

    public bool isStorageBuilding;
    public bool isMainSource;
    public bool isUIOpened;
    public bool isPortalBuild = false;

    public GameObject myVision;
    [HideInInspector] public Vector3 visionPos;
    public float visionRadius;

    [HideInInspector]
    public int maxAmount;
    [HideInInspector]
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
    float upgradeConsumptionPer = 10;
    public bool isEnergyConnected = false;
    public bool isOperate;
    [HideInInspector]
    public float efficiency;
    [HideInInspector]
    public float effiCooldown;

    public string portalName = "";

    //public bool isMainEnergyColony;
    public SoundManager soundManager;

    public bool isInHostMap;
    public bool isManualDestroy;    //사용자가 철거 명령을 하는 경우 true. 철거로 인해 파괴되는지 몬스터에 의해 파괴되는지 구분하기 위해 사용
    public bool destroyStart;       //철거 시작하고 나서 계속 돌아감
    public bool isDestroying;       //철거 시작하는 순간 한 번만 돌아감
    public bool isRunning;          //isOperate쓰기 애매한 건물에 작동 체크용(드론운송 관련 건물들)
    protected float destroyInterval;
    protected float destroyTimer;

    public bool settingEndCheck = false;
    public List<(int, int)> DelaySendList = new List<(int, int)>();
    public List<int> DelayGetList = new List<int>();
    protected int buildingIndex;
    public List<Vector3> connectedPosList = new List<Vector3>();

    public bool canUpgrade;
    public SpriteRenderer warningIcon;
    //public Sprite warningRed;
    //public Sprite warningYellow;
    public bool warningIconCheck;
    public IEnumerator warning;

    public delegate void OnHpChanged();
    public OnHpChanged onHpChangedCallback;

    public delegate void OnEffectUpgradeCheck();
    public OnEffectUpgradeCheck onEffectUpgradeCheck;

    protected bool[] increasedStructure;
    // 0 생산속도, 1 Hp, 2 인풋아웃풋 속도, 3 소비량 감소, 4 방어력

    [SerializeField]
    protected bool getAnim;
    protected Animator animator;
    [SerializeField]
    protected Sprite[] strImg;  // 0번은 멈춤, 1번은 작동(애니메이션이 있는 오브젝트는 멈춤만 등록)

    public bool isAuto;    // 분쇄기 자동화 체크

    public float selectPointSetPos;

    protected int[,] oneDirections = new int[,] { { 0, 1 }, { 1, 0 }, { 0, -1 }, { -1, 0 } }; // 1x1, 2x2 건물의 주변 좌표
    protected int[,] twoDirections = new int[,] { { -1, 1 }, { 0, 1 }, { 1, 0 }, { 1, -1 }, { 0, -2 }, { -1, -2 }, { -2, -1 }, { -2, 0 } };

    [SerializeField]
    protected FactoryOverlay overlay;
    [SerializeField]
    bool overlayUse;
    protected GameManager gameManager;

    protected virtual void Awake()
    {
        gameManager = GameManager.instance;
        playerInven = gameManager.inventory;
        buildName = structureData.FactoryName;
        col = GetComponent<BoxCollider2D>();
        unitSprite = GetComponent<SpriteRenderer>();

        maxLevel = structureData.MaxLevel;
        maxHp = structureData.MaxHp[level];
        defense = structureData.Defense[level];
        hp = structureData.MaxHp[level];
        getDelay = 0.05f;
        sendDelay = structureData.SendDelay[level];
        hpBar.enabled = false;
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
        destroyInterval = structureData.RemoveGauge;
        soundManager = SoundManager.instance;
        repairEffect = GetComponentInChildren<RepairEffectFunc>();
        destroyTimer = destroyInterval;
        warningIconCheck = false;
        visionPos = transform.position;
        increasedStructure = new bool[5];
        onEffectUpgradeCheck += IncreasedStructureCheck;
        setModel = GetComponent<SpriteRenderer>(); 
        if (TryGetComponent(out Animator anim))
        {
            getAnim = true;
            animator = anim;
        }
        NonOperateStateSet(isOperate);
        WarningStateCheck();
    }

    protected virtual void Update()
    {
        if (Time.timeScale == 0)
        {
            return;
        }

        if (!removeState)
        {
            if (isRepair)
            {
                RepairFunc(false);
            }
            else if (isPreBuilding)
            {
                RepairFunc(true);
            }
        }

        if (destroyStart)
        {
            if (GameManager.instance.debug)
                destroyTimer -= (Time.deltaTime * 10);
            else
                destroyTimer -= Time.deltaTime;
            repairBar.fillAmount = destroyTimer / destroyInterval;
            OperateStateSet(false);
            if (destroyTimer <= 0)
            {
                ObjRemoveFunc();
                //destroyStart = false;
            }
        }
    }

    void OnEnable()
    {
        if (overlayUse)
        {
            GameManager.OnFactoryOverlayToggle += FactoryOverlay;
        }
    }

    void OnDisable()
    {
        if (!IsServer && GameManager.instance && soundManager)
        {
            ClientItemDrop();
            soundManager.PlayUISFX("BuildingRemove");
            GameManager.instance.BuildAndSciUiReset();
        }

        if (overlayUse)
            GameManager.OnFactoryOverlayToggle -= FactoryOverlay;
    }

    public virtual void CheckSlotState(int slotindex)
    {
        // update에서 검사해야 하는 특정 슬롯들 상태를 인벤토리 콜백이 있을 때 미리 저장
    }

    public void WarningStateCheck()
    {
        if (warningIcon != null)
            StartCoroutine(CheckWarning());
    }

    protected virtual IEnumerator CheckWarning()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(1f);

            if (!isPreBuilding && !removeState && energyUse)
            {
                if (conn != null && conn.group != null)
                {
                    if (conn.group.efficiency < 1f)
                    {
                        if (!warningIconCheck)
                        {
                            //low energy
                            if (warning != null)
                                StopCoroutine(warning);
                            warningIcon.sprite = Resources.Load<Sprite>("warning_yellow");
                            warning = FlickeringIcon();
                            StartCoroutine(warning);
                            warningIconCheck = true;
                        }
                    }
                    else
                    {
                        if (warningIconCheck)
                        {
                            if (warning != null)
                                StopCoroutine(warning);
                            warningIconCheck = false;
                            warningIcon.enabled = false;
                        }
                    }
                }
                else
                {
                    if (!warningIconCheck)
                    {
                        //disconnected
                        if (warning != null)
                            StopCoroutine(warning);
                        warningIcon.sprite = Resources.Load<Sprite>("warning_red");
                        warning = FlickeringIcon();
                        StartCoroutine(warning);
                        warningIconCheck = true;
                    }
                }
            }
        }
    }

    public IEnumerator FlickeringIcon()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f);
            warningIcon.enabled = !warningIcon.enabled;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyServerRpc()
    {
        DestroyClientRpc();
    }

    [ClientRpc]
    void DestroyClientRpc()
    {
        Debug.Log("DestroyClientRpc : " + destroyStart + " : " + !isPreBuilding);
        if (!destroyStart && !isPreBuilding && destroyTimer > 0)
        {
            isManualDestroy = true;
            destroyStart = true;
            isDestroying = true;
            isPreBuilding = true;
            removeState = true;
            unitCanvas.SetActive(true);
            repairBar.enabled = true;

            CloseUI();
        }
    }

    //public void DestroyStart()
    //{
    //    if (!destroyStart && !isPreBuilding && destroyTimer > 0)
    //    {
    //        destroyStart = true;
    //        isPreBuilding = true;
    //        isSetBuildingOk = false;
    //        unitCanvas.SetActive(true);
    //        repairBar.enabled = true;
    //        RemoveObjServerRpc();
    //    }
    //}

    protected void ObjRemoveFunc()
    {
        if (IsServer)
        {
            ItemDrop();
            RefundCost();
        }
        //else
        //    ClientItemDrop();
        //AddInvenItem();
        RemoveObjServerRpc();
        //DestroyFuncServerRpc();
        soundManager.PlayUISFX("BuildingRemove");
        GameManager.instance.BuildAndSciUiReset();
    }

    void RefundCost()
    {
        if (isPortalBuild)
        {
            return;
        }
        else
        {
            BuildingData buildingData = new BuildingData();
            buildingData = BuildingDataGet.instance.GetBuildingName(buildName, level + 1);
            //Inventory inventory;
            //GameManager gameManager = GameManager.instance;

            //if (isInHostMap)
            //    inventory = gameManager.hostMapInven;
            //else
            //    inventory = gameManager.clientMapInven;

            for (int i = 0; i < buildingData.GetItemCount(); i++)
            {
                //inventory.Add(ItemList.instance.itemDic[buildingData.items[i]], buildingData.amounts[i]);
                ItemToItemProps(ItemList.instance.itemDic[buildingData.items[i]], buildingData.amounts[i]);
                Overall.instance.OverallConsumptionCancel(ItemList.instance.itemDic[buildingData.items[i]], buildingData.amounts[i]);
            }
        }
    }

    protected virtual void OnClientConnectedCallback(ulong clientId)
    {        
        ClientConnectSyncServerRpc();
        RepairGaugeServerRpc();
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
        //ClientConnectSyncClientRpc(level, dirNum, height, width, isInHostMap, isSetBuildingOk, isPreBuilding);
        ClientConnectSyncClientRpc(level, dirNum, height, width, isInHostMap, hp);
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
        MapDataSaveClientRpc(transform.position);
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

    [ServerRpc(RequireOwnership = false)]
    protected void RepairGaugeServerRpc()
    {
        RepairGaugeClientRpc(isPreBuilding, destroyStart, hp, repairGauge, destroyTimer);
    }

    [ClientRpc]
    protected virtual void RepairGaugeClientRpc(bool preBuilding, bool destroy, float hpSet, float repairGaugeSet, float destroyTimerSet)
    {
        StructureStateSet(preBuilding, destroy, hpSet, repairGaugeSet, destroyTimerSet);
    }

    [ClientRpc]
    public void ConnectCheckClientRpc(bool isEnd)
    {
        settingEndCheck = isEnd;
    }

    [ClientRpc]
    public virtual void ClientConnectSyncClientRpc(int syncLevel, int syncDir, int syncHeight, int syncWidth, bool syncMap, float syncHp)
    {
        if (IsServer)
            return;

        level = syncLevel;
        DataSet();
        maxHp = structureData.MaxHp[level];
        dirNum = syncDir;
        height = syncHeight;
        width = syncWidth;
        isInHostMap = syncMap;
        hp = syncHp;
        ColliderTriggerOnOff(false);
        gameObject.AddComponent<DynamicGridObstacle>();
        myVision.SetActive(true);
        onEffectUpgradeCheck.Invoke();
        StrBuilt();
    }

    [ClientRpc]
    void NearObjSyncClientRpc(ulong ObjID, int index, ClientRpcParams rpcParams = default)
    {
        if (IsServer)
            return;

        CheckPos();

        NetworkObject obj = NetworkObjManager.instance.FindNetworkObj(ObjID);
        nearObj[index] = obj.gameObject;
    }

    [ClientRpc]
    void InOutObjSyncClientRpc(ulong ObjID, bool isIn, ClientRpcParams rpcParams = default)
    {
        if (IsServer)
            return;

        NetworkObject obj = NetworkObjManager.instance.FindNetworkObj(ObjID);

        if (isIn)
            inObj.Add(obj.gameObject);
        else
        {
            if (!outObj.Contains(obj.gameObject))
                outObj.Add(obj.gameObject);
        }
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
        maxHp = structureData.MaxHp[level];
        defense = structureData.Defense[level];
        hp = maxHp;
        hpBar.fillAmount = hp / structureData.MaxHp[level];
        energyUse = structureData.EnergyUse[level];
        energyConsumption = structureData.Consumption[level];
        sendDelay = structureData.SendDelay[level];
        onEffectUpgradeCheck.Invoke();
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
        onEffectUpgradeCheck.Invoke();
    }

    public virtual void StrBuilt()
    {
        // 건설 시 근처 건물들이 NearStrBuilt()를 실행하도록 알림을 보냄

        NearStrBuilt();

        int posX = (int)transform.position.x;
        int posY = (int)transform.position.y;
        Cell cell;
        if (width == 1 && height == 1)
        {
            for (int i = 0; i < 4; i++)
            {
                int nearX = posX + oneDirections[i, 0];
                int nearY = posY + oneDirections[i, 1];
                cell = GameManager.instance.GetCellDataFromPosWithoutMap(nearX, nearY);
                if (cell.structure != null)
                {
                    cell.structure.GetComponent<Structure>().NearStrBuilt();
                }
            }
        }
        else if (width == 2 && height == 2)
        {
            for (int i = 0; i < 8; i++)
            {
                int nearX = posX + twoDirections[i, 0];
                int nearY = posY + twoDirections[i, 1];
                cell = GameManager.instance.GetCellDataFromPosWithoutMap(nearX, nearY);

                if (cell.structure != null)
                {
                    cell.structure.GetComponent<Structure>().NearStrBuilt();
                }
            }
        }

        CheckSlotState(0);
    }

    public virtual void NearStrBuilt()
    {
        // 건물을 지었을 때나 근처에 새로운 건물이 지어졌을 때 동작
        // 변경사항이 생기면 DelayNearStrBuiltCoroutine()에도 반영해야 함
        if (IsServer)
        {
            CheckPos();
        }
        else
        {
            DelayNearStrBuilt();
        }
    }

    public virtual void DelayNearStrBuilt()
    {
        // 동시 건설, 클라이언트 동기화 등의 이유로 딜레이를 주고 NearStrBuilt()를 실행할 때 사용
        StartCoroutine(DelayNearStrBuiltCoroutine());
    }

    protected virtual IEnumerator DelayNearStrBuiltCoroutine()
    {
        // 동시 건설이나 그룹핑을 따로 예외처리 하는 경우가 아니면 NearStrBuilt()를 그대로 사용
        yield return new WaitForEndOfFrame();

        CheckPos();
    }

    //protected virtual void SetDirNum()
    //{
    //    CheckPos();
    //    setModel.sprite = modelNum[dirNum];
    //}

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
        else if (width == 2 && height == 2)
        {
            sizeOneByOne = false;
            if(nearObj.Length != 8)
                nearObj = new GameObject[8];
            //indices = new int[] { 3, 0, 0, 1, 1, 2, 2, 3 };
            //startTransform = new Vector2[] { new Vector2(0.5f, 0.5f), new Vector2(0.5f, -0.5f), new Vector2(-0.5f, -0.5f), new Vector2(-0.5f, 0.5f) };
            //directions = new Vector3[] { transform.up, transform.right, -transform.up, -transform.right };
        }
    }

    // 1x1 사이즈 근처 오브젝트 찾는 위치(상하좌우) 설정
    protected virtual void CheckNearObj(Vector2 direction, int index, Action<GameObject> callback)
    {
        int nearX = (int)(transform.position.x + direction.x);
        int nearY = (int)(transform.position.y + direction.y);
        Cell cell = GameManager.instance.GetCellDataFromPosWithoutMap(nearX, nearY);
        if (cell == null)
            return;

        GameObject obj = cell.structure;
        if (obj != null)
        {
            if (GetComponent<LogisticsCtrl>() || (GetComponent<Production>() && !GetComponent<FluidFactoryCtrl>()))
            {
                if (obj.GetComponent<FluidFactoryCtrl>() && !obj.GetComponent<Refinery>() && !obj.GetComponent<SteamGenerator>())
                {
                    return;
                }
            }
            else if (GetComponent<FluidFactoryCtrl>() && !GetComponent<Refinery>() && !GetComponent<SteamGenerator>())
            {
                if (obj.GetComponent<LogisticsCtrl>())
                {
                    return;
                }
            }

            if (obj.CompareTag("Factory") || obj.CompareTag("Tower"))
            {
                nearObj[index] = obj;
                callback(obj);
            }
        }
    }

    // 2x2 사이즈 근처 오브젝트 찾는 위치(상하좌우) 설정
    protected virtual void CheckNearObj(int index, Action<GameObject> callback)
    {
        int nearX = (int)transform.position.x + twoDirections[index, 0];
        int nearY = (int)transform.position.y + twoDirections[index, 1];
        Cell cell = GameManager.instance.GetCellDataFromPosWithoutMap(nearX, nearY);
        if (cell == null)
            return;

        GameObject obj = cell.structure;

        if (obj != null)
        {
            if (obj.CompareTag("Factory"))
            {
                nearObj[index] = obj;
                callback(obj);
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
        MapGenerator.instance.RemoveFogTile(visionPos, visionRadius);
    }

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

        if (energyUse)
        {
            GameObject TriggerObj = new GameObject("Trigger");
            CircleCollider2D coll = TriggerObj.AddComponent<CircleCollider2D>();
            coll.isTrigger = true;
            TriggerObj.transform.position = Vector3.zero;
            StartCoroutine(Move(TriggerObj));
        }
        soundManager.PlaySFX(gameObject, "structureSFX", "BuildingSound");
    }

    protected IEnumerator Move(GameObject obj)
    {
        yield return new WaitForSeconds(0.1f);
        obj.transform.position = transform.position;

        yield return new WaitForSeconds(1f);
        Destroy(obj);
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
        ColliderTriggerOnOff(false);
        gameObject.AddComponent<DynamicGridObstacle>();
        myVision.SetActive(true);
        DataSet();
    }

    public virtual void StructureStateSet(bool preBuilding, bool destroy, float hpSet, float repairGaugeSet, float destroyTimerSet)
    {
        hp = hpSet;
        isPreBuilding = preBuilding;
        destroyStart = destroy;

        if (destroyStart)
        {
            hpBar.enabled = false;
            destroyTimer = destroyTimerSet;
            repairBar.fillAmount = destroyTimer / destroyInterval;
            unitCanvas.SetActive(true);
        }
        else if (isPreBuilding)
        {
            hpBar.enabled = false;
            repairGauge = repairGaugeSet;
            repairBar.fillAmount = repairGauge / structureData.MaxRepairGauge;
            unitCanvas.SetActive(true);
        }
        else if (hp < maxHp)
        {
            repairBar.enabled = false;
            hpBar.fillAmount = hp / maxHp;
            hpBar.enabled = true;
            unitCanvas.SetActive(true);
        }
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

    protected void ItemListRemove()
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

        if (inObj.Count < getItemIndex)
        {
            GetItemIndexSet();
            Invoke(nameof(DelayGetItem), getDelay);
            return;
        }
        else if (inObj[getItemIndex].TryGetComponent(out BeltCtrl belt))
        {
            if (!belt.isItemStop)
            {
                GetItemIndexSet();
                Invoke(nameof(DelayGetItem), getDelay);
                return;
            }
            else if (TryGetComponent(out Production production) && belt.itemObjList.Count > 0 && !production.CanTakeItem(belt.itemObjList[0].item))
            {
                GetItemIndexSet();
                Invoke(nameof(DelayGetItem), getDelay);
                return;
            }
        }
        else if (TryGetComponent(out Unloader unloader) && inObj[getItemIndex].TryGetComponent(out Production inObjScript) && !inObjScript.UnloadItemCheck(unloader.selectItem))
        {
            GetItemIndexSet();
            Invoke(nameof(DelayGetItem), getDelay);
            return;
        }
        else if (!GetComponent<Unloader>() && inObj[getItemIndex].GetComponent<Structure>())
        {
            GetItemIndexSet();
            Invoke(nameof(DelayGetItem), getDelay);
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
        GetItemFunc(inObjIndex);
        //if (IsServer)
        //{
        //    GetItemFunc(inObjIndex);
        //}
        //else if (settingEndCheck)
        //{
        //    GetDelaySet(inObjIndex);
        //}
    }

    protected virtual void GetItemFunc(int inObjIndex)
    {
        if (inObj[inObjIndex].TryGetComponent(out BeltCtrl belt))
        {
            OnFactoryItem(belt.itemObjList[0]);
            belt.isItemStop = false;
            belt.itemObjList.RemoveAt(0);
            if(belt.beltGroupMgr.groupItem.Count != 0)
                belt.beltGroupMgr.groupItem.RemoveAt(0);
            belt.ItemNumCheck();
        }
        DelayGetItem();
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
        if (itemIndex < 0) return;

        itemSetDelay = true;

        if (outObj.Count <= sendItemIndex || !outObj[sendItemIndex])
        {
            SendItemIndexSet();
            //itemSetDelay = false;
            Invoke(nameof(ItemSetDelayReset), 0.05f);
            return;
        }
        else
        {
            Structure outFactory = outObj[sendItemIndex].GetComponent<Structure>();

            if (outFactory.isFull || outFactory.takeItemDelay || outFactory.destroyStart || outFactory.isPreBuilding)
            {
                SendItemIndexSet();
                //itemSetDelay = false;
                Invoke(nameof(ItemSetDelayReset), 0.05f);
                return;
            }
            else if (outFactory.TryGetComponent(out Production production))
            {
                Item item = GeminiNetworkManager.instance.GetItemSOFromIndex(itemIndex);
                if (!production.CanTakeItem(item))
                {
                    SendItemIndexSet();
                    //itemSetDelay = false;
                    Invoke(nameof(ItemSetDelayReset), 0.05f);
                    return;
                }
            }
            else if (outFactory.isMainSource)
            {
                SendItemIndexSet();
                //itemSetDelay = false;
                Invoke(nameof(ItemSetDelayReset), 0.05f);
                return;
            }
            outFactory.takeItemDelay = true;
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

    protected void ItemSetDelayReset()
    {
        itemSetDelay = false;
    }

    [ServerRpc]
    protected virtual void SendItemServerRpc(int itemIndex, int outObjIndex)
    {
        if (outObj[outObjIndex].TryGetComponent(out BeltCtrl beltCtrl))
        {
            int beltGroupIndex = 0;
            int groupItemCount = beltCtrl.beltGroupMgr.groupItem.Count;

            if (groupItemCount > 0)
            {
                beltGroupIndex = beltCtrl.beltGroupMgr.groupItem[groupItemCount - 1].beltGroupIndex;

                if (beltGroupIndex == int.MaxValue)
                {
                    beltGroupIndex = 0;
                }
                else
                {
                    beltGroupIndex++;
                }
            }
            SendItemClientRpc(itemIndex, outObjIndex, beltGroupIndex);
        }
        else
            SendItemClientRpc(itemIndex, outObjIndex);
    }

    [ClientRpc]
    protected virtual void SendItemClientRpc(int itemIndex, int outObjIndex)
    {
        SendItemFunc(itemIndex, outObjIndex);
        //if (IsServer)
        //{
        //    SendItemFunc(itemIndex, outObjIndex);
        //}
        //else if (settingEndCheck)
        //{
        //    SendDelaySet(itemIndex, outObjIndex);
        //}
    }

    [ClientRpc]
    protected virtual void SendItemClientRpc(int itemIndex, int outObjIndex, int beltGroupIndex)
    {
        SendItemFunc(itemIndex, outObjIndex, beltGroupIndex);
    }

    protected virtual void SendItemFunc(int itemIndex, int outObjIndex, int beltGroupIndex)
    {
        Item item = GeminiNetworkManager.instance.GetItemSOFromIndex(itemIndex);

        Structure outFactory = outObj[outObjIndex].GetComponent<Structure>();
        if (outObj[outObjIndex].TryGetComponent(out BeltCtrl beltCtrl))
        {
            var itemPool = ItemPoolManager.instance.Pool.Get();
            ItemProps spawnItem = itemPool.GetComponent<ItemProps>();
            spawnItem.beltGroupIndex = beltGroupIndex;
            if (beltCtrl.OnBeltItem(spawnItem))
            {
                SpriteRenderer sprite = spawnItem.GetComponent<SpriteRenderer>();
                sprite.sprite = item.icon;
                sprite.sortingOrder = 2;
                spawnItem.item = item;
                spawnItem.amount = 1;
                Vector3 spawnPos = Vector3.Lerp(transform.position, beltCtrl.nextPos[2], 0.8f);
                spawnItem.transform.position = spawnPos;
                spawnItem.isOnBelt = true;
                spawnItem.setOnBelt = beltCtrl.GetComponent<BeltCtrl>();

                if (GetComponent<Production>())
                {
                    SubFromInventory();
                }
                else if (GetComponent<LogisticsCtrl>() && !GetComponent<ItemSpawner>())
                {
                    ItemListRemove();
                    ItemNumCheck();
                }
            }
        }
        else if (!outFactory.isMainSource)
        {
            if (outObj[outObjIndex].GetComponent<LogisticsCtrl>())
                SendFacDelay(outObj[outObjIndex], item);
            else if (outObj[outObjIndex].TryGetComponent(out Production production) && production.CanTakeItem(item))
                SendFacDelay(outObj[outObjIndex], item);
        }

        outFactory.takeItemDelay = false;
        Invoke(nameof(DelaySetItem), sendDelay);
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
                sprite.sprite = item.icon;
                sprite.sortingOrder = 2;
                spawnItem.item = item;
                spawnItem.amount = 1;
                Vector3 spawnPos = Vector3.Lerp(transform.position, beltCtrl.nextPos[2], 0.8f);
                spawnItem.transform.position = spawnPos;
                spawnItem.isOnBelt = true;  
                spawnItem.setOnBelt = beltCtrl.GetComponent<BeltCtrl>();

                if (GetComponent<Production>())
                {
                    SubFromInventory();
                }
                else if (GetComponent<LogisticsCtrl>() && !GetComponent<ItemSpawner>() && !GetComponent<Unloader>())
                {
                    ItemListRemove();
                    ItemNumCheck();
                }
            }
        }
        else if (!outFactory.isMainSource)
        {
            if (outObj[outObjIndex].GetComponent<LogisticsCtrl>())
                SendFacDelay(outObj[outObjIndex], item);
            else if (outObj[outObjIndex].TryGetComponent(out Production production) && production.CanTakeItem(item))
                SendFacDelay(outObj[outObjIndex], item);
        }

        outFactory.takeItemDelay = false;
        Invoke(nameof(DelaySetItem), sendDelay);
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

    protected void SendFacDelay(GameObject outFac, Item item)
    {
        if (CanSendItemCheck())
        {
            if (outObj.Count > 0 && outFac != null)
            {
                if (outFac.TryGetComponent(out Structure outFactory))
                {
                    outFactory.OnFactoryItem(item);
                }
            }

            if (GetComponent<LogisticsCtrl>() && !GetComponent<ItemSpawner>())
            {
                ItemListRemove();
                ItemNumCheck();
            }
            else if (GetComponent<Production>())
            {
                SubFromInventory();
            }
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
        float reducedDamage = Mathf.Max(damage - defense, 5);

        if (!damageEffectOn)
        {
            StartCoroutine(TakeDamageEffect());
        }

        hp -= reducedDamage;
        if (hp < 0f)
            hp = 0f;
        onHpChangedCallback?.Invoke();
        hpBar.fillAmount = hp / maxHp;

        if (IsServer && hp <= 0f)
        {
            hp = 0f;
            DieFuncServerRpc();
        }
    }

    protected IEnumerator TakeDamageEffect()
    {
        damageEffectOn = true;

        unitSprite.color = new Color32(255, 100, 100, 255);

        yield return new WaitForSeconds(0.3f);

        unitSprite.color = new Color32(255, 255, 255, 255);

        damageEffectOn = false;
    }

    [ServerRpc]
    protected virtual void DieFuncServerRpc()
    {
        DieFuncClientRpc();
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
        Instantiate(RuinExplo, transform.position, Quaternion.identity);
        soundManager.PlaySFX(gameObject, "structureSFX", "Destory");

        if (!IsServer)
        {
            ClientItemDrop();
            return;
        }

        ItemDrop();

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

        RemoveObjServerRpc();
    }

    protected void ItemToItemProps(Item item, int itemAmount)
    {
        int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(item);
        GeminiNetworkManager.instance.ItemSpawnServerRpc(itemIndex, itemAmount, transform.position);

        //var itemPool = ItemPoolManager.instance.Pool.Get();
        //ItemProps itemProps = itemPool.GetComponent<ItemProps>();

        //SpriteRenderer sprite = itemProps.GetComponent<SpriteRenderer>();
        //sprite.sprite = item.icon;
        //sprite.sortingOrder = 2;
        //sprite.material = ResourcesManager.instance.outlintMat;
        //itemProps.item = item;
        //itemProps.amount = itemAmount;
        //itemProps.transform.position = transform.position;
        //itemProps.ResetItemProps();

        //NetworkObject itemNetworkObject = itemProps.GetComponent<NetworkObject>();
        //itemNetworkObject.Spawn(true);
    }

    protected virtual void ItemDrop() { }

    protected virtual void ClientItemDrop()
    {
        if (itemObjList.Count > 0)
        {
            foreach (ItemProps itemProps in itemObjList)
            {
                itemProps.ClientResetItemProps();
            }
        }
    }

    public void RepairFunc(float heal)
    {
        if (hp == maxHp)
        {
            return;
        }
        else if (hp + heal > maxHp)
        {
            hp = maxHp;
        }
        else
        {
            hp += heal;
        }
        RepairServerRpc(hp);
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
        onHpChangedCallback?.Invoke();
        if (maxHp > hp)
        {
            hpBar.fillAmount = hp / maxHp;
        }
        else
            unitCanvas.SetActive(false);
    }

    public void RepairSet(bool repair)
    {
        hp = maxHp;
        isRepair = repair;
    }

    protected virtual void RepairFunc(bool isBuilding)
    {
        if (isBuilding)
        {
            if (GameManager.instance.debug)
                repairGauge += (Time.deltaTime * 10);
            else
                repairGauge += Time.deltaTime;

            repairBar.fillAmount = repairGauge / structureData.MaxBuildingGauge;
            if (repairGauge >= structureData.MaxBuildingGauge)
            {
                isPreBuilding = false;
                repairGauge = 0.0f;
                repairBar.enabled = false;
                if (hp < maxHp)
                {
                    unitCanvas.SetActive(true);
                    hpBar.enabled = true;
                }
                else
                {
                    unitCanvas.SetActive(false);
                }

                //ColliderTriggerOnOff(false);
            }
        }
        else
        {
            repairGauge += Time.deltaTime;

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
        hp = maxHp;
        Debug.Log("DataSet : " + hp);
        unitCanvas.SetActive(false);
        hpBar.fillAmount = hp / maxHp;
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
        yield return new WaitForSeconds(0.1f);

        if (obj.GetComponent<Structure>() != null)
        {
            if (obj.TryGetComponent(out BeltCtrl belt))
            {
                if (belt.GetComponentInParent<BeltGroupMgr>().nextObj != this.gameObject)
                {
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
        yield return new WaitForSeconds(0.1f);

        if (obj.GetComponent<WallCtrl>())
        {
            yield break;
        }

        if (obj.GetComponent<Structure>() != null)
        {
            if ((obj.GetComponent<ItemSpawner>() && GetComponent<ItemSpawner>())
                || obj.GetComponent<Unloader>())
            {
                yield break;
            }

            if (obj.TryGetComponent(out BeltCtrl belt))
            {
                if (obj.GetComponentInParent<BeltGroupMgr>().nextObj == this.gameObject)
                {
                    yield break;
                }
                belt.FactoryPosCheck(GetComponentInParent<Structure>());
            }
            else
            {
                outSameList.Add(obj);
                StartCoroutine(OutCheck(obj));
            }
            if (!outObj.Contains(obj))
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
                StopCoroutine(nameof(SendFacDelay));
                outObj.Remove(otherObj);
                Invoke(nameof(RemoveSameOutList), 0.1f);
                InOutObjIndexResetClientRpc(false);
            }
        }
    }

    public virtual void ResetNearObj(GameObject game)
    {
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
    }

    [ServerRpc(RequireOwnership = false)]
    public virtual void RemoveObjServerRpc()
    {
        RemoveObjClientRpc();
    }

    [ClientRpc]
    void RemoveObjClientRpc()
    {
        //removeState = true;
        //ColliderTriggerOnOff(true);
        StopAllCoroutines();

        if (InfoUI.instance.str == this)
            InfoUI.instance.SetDefault();

        for (int i = 0; i < nearObj.Length; i++)
        {
            if (nearObj[i] != null && nearObj[i].TryGetComponent(out Structure structure))
            {
                structure.ResetNearObj(gameObject);
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
        if(overclockTower != null && TryGetComponent(out Production prod))
            overclockTower.RemoveObjectsOutOfRange(prod);

        if (IsServer && GetComponent<BeltCtrl>() && GetComponentInParent<BeltManager>() && GetComponentInParent<BeltGroupMgr>())
        {
            BeltManager beltManager = GetComponentInParent<BeltManager>();
            BeltGroupMgr beltGroup = GetComponentInParent<BeltGroupMgr>();
            bool canDivide = beltManager.BeltDivide(beltGroup, gameObject);
            if(canDivide)
                beltGroup.ClientBeltSyncServerRpc();
        }
        else if (TryGetComponent(out Transporter transporter))
        {
            if (!transporter.isManualDestroy)
                transporter.RemoveFunc();
            transporter.TrUnitToHomelessDrone();
        }
        else if (TryGetComponent(out AutoSeller autoSeller))
        {
            if (!autoSeller.isManualDestroy)
                autoSeller.RemoveFunc();
            autoSeller.TrUnitToHomelessDrone();
        }
        else if (TryGetComponent(out AutoBuyer autoBuyer))
        {
            if (!autoBuyer.isManualDestroy)
                autoBuyer.RemoveFunc();
            autoBuyer.TrUnitToHomelessDrone();
        }
        else if (TryGetComponent(out FluidFactoryCtrl fluid))
        {
            fluid.RemoveMainSource();
        }
        else if (TryGetComponent(out PortalObj portalObj))
        {
            portalObj.RemovePortalData();
        }
        else if (TryGetComponent(out Overclock overclock))
        {
            overclock.OverclockRemove();
        }

        if (TryGetComponent(out GetUnderBeltCtrl getUnder))
        {
            getUnder.EndRenderer();
        }
        else if (TryGetComponent(out SendUnderBeltCtrl sendUnder))
        {
            sendUnder.EndRenderer();
        }

        if (GameManager.instance.focusedStructure == this)
        {
            GameManager.instance.focusedStructure = null;
        }

        DestroyFuncServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public virtual void DestroyFuncServerRpc()
    {
        DestroyFuncClientRpc();
    }

    [ClientRpc]
    public virtual void DestroyFuncClientRpc()
    {
        ColliderTriggerOnOff(true);

        Map map;
        if (isInHostMap)
        {
            map = GameManager.instance.hostMap;
        }
        else
        {
            map = GameManager.instance.clientMap;
        }

        if (sizeOneByOne)
        {
            int x = Mathf.FloorToInt(transform.position.x);
            int y = Mathf.FloorToInt(transform.position.y);
            Cell cell = map.GetCellDataFromPos(x, y);
            bool isOnMap = map.IsOnMap(x, y);

            if (isOnMap && cell.structure == gameObject)
            {
                cell.structure = null;
            }
        }
        else
        {
            int x = Mathf.FloorToInt(transform.position.x - 0.5f);
            int y = Mathf.FloorToInt(transform.position.y - 0.5f);
            Cell cell = map.GetCellDataFromPos(x, y);
            bool isOnMap = map.IsOnMap(x, y);

            if (isOnMap && cell.structure == gameObject)
            {
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        map.GetCellDataFromPos(x + j, y + i).structure = null;
                    }
                }
            }
        }

        StrBuilt();

        NetworkObjManager.instance.NetObjRemove(gameObject);
        onEffectUpgradeCheck -= IncreasedStructureCheck;

        if (IsServer && NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn();
        }
    }

    void CloseUI()
    {
        Debug.Log("isUIOpened : " + isUIOpened);

        if (!isUIOpened) return;

        if (TryGetComponent(out LogisticsClickEvent solidFacClickEvent))
        {
            if (solidFacClickEvent.LogisticsUI != null)
            {
                if (solidFacClickEvent.LogisticsUI.activeSelf)
                {
                    if (solidFacClickEvent.sFilterManager != null)
                        solidFacClickEvent.sFilterManager.CloseUI();
                    else if (solidFacClickEvent.unloaderManager != null)
                        solidFacClickEvent.unloaderManager.CloseUI();
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
                else
                {
                    if (structureClickEvent.sInvenManager != null)
                    {
                        structureClickEvent.sInvenManager.CloseRecipeUI();
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

    [ServerRpc(RequireOwnership = false)]
    public void UpgradeFuncServerRpc()
    {
        UpgradeFuncClientRpc();
    }

    [ClientRpc]
    public virtual void UpgradeFuncClientRpc()
    {
        level++;

        if (hp == maxHp)
        {
            hp = structureData.MaxHp[level];
        }

        maxHp = structureData.MaxHp[level];
        defense = structureData.Defense[level];
        sendDelay = structureData.SendDelay[level];
        onEffectUpgradeCheck.Invoke();
    }

    public virtual Dictionary<Item, int> PopUpItemCheck() { return null; }

    public virtual (bool, bool , bool, EnergyGroup, float) PopUpEnergyCheck()
    {
        if (energyUse || isEnergyStr)
        {
            if (conn != null && conn.group != null)
            {
                // 발전기류는 energyConsumption 변수 자리에 생산량을 리턴함
                return (energyUse, isEnergyStr, false, conn.group, energyConsumption);
            }
            else
            {
                return (energyUse, isEnergyStr, false, null, energyConsumption);
            }
        }

        return (false, false, false, null, 0f);
    }

    public virtual (float, float) PopUpStoredCheck() { return (0f, 0f); }

    public virtual (float, float) PopUpStoredEnergyCheck() { return (0f, 0f); }

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

    public virtual IEnumerator EfficiencyCheck() { yield return null; }

    [ClientRpc]
    public void MapDataSaveClientRpc(Vector3 pos)
    {
        Vector2 tileSetPos = pos;

        if (width == 2  && height == 2)
        {
            tileSetPos = new Vector3(pos.x - 0.5f, pos.y - 0.5f);
        }
        else
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
        data.sideObj = false;
        data.pos = Vector3Extensions.FromVector3(transform.position);
        data.hp = hp;
        data.planet = isInHostMap;
        data.level = level;
        data.direction = dirNum;
        data.isPreBuilding = isPreBuilding;
        data.destroyStart = destroyStart;
        data.repairGauge = repairGauge;
        data.destroyTimer = destroyTimer;
        data.portalName = portalName;
        data.isAuto = isAuto;

        foreach (Item items in itemList)
        {
            int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(items);
            data.itemIndex.Add(itemIndex);
        }

        return data;
    }

    public virtual void IncreasedStructureCheck()
    {
        increasedStructure = ScienceDb.instance.IncreasedStructureCheck(0);
        // 0 생산속도, 1 Hp, 2 인풋아웃풋 속도, 3 소비량 감소, 4 방어력

        if (increasedStructure[0])
        {
            effiCooldownUpgradeAmount = effiCooldown * effiCooldownUpgradePer / 100;
        }
        if (increasedStructure[1])
        {
            bool isHpFull = false;
            if (maxHp == hp)
            {
                isHpFull = true;
            }

            maxHp = structureData.UpgradeMaxHp[level];
            if (isHpFull)
                hp = maxHp;
        }
        if (increasedStructure[2])
        {
            sendDelay = structureData.UpgradeSendDelay[level];
        }
        if (increasedStructure[3])
        {
            energyConsumption = structureData.Consumption[level] - (structureData.Consumption[level] * upgradeConsumptionPer / 100);
        }
        if (increasedStructure[4])
        {
            defense = structureData.UpgradeDefense[level];
        }
    }

    public void SetPortalName(string str)
    {
        SetPortalNameServerRpc(str);
    }

    [ServerRpc (RequireOwnership = false)]
    public void SetPortalNameServerRpc(string str)
    {
        SetPortalNameClientRpc(str);
    }

    [ClientRpc]
    public void SetPortalNameClientRpc(string str)
    {
        portalName = str;
    }

    protected void OperateStateSet(bool isOn)
    {
        if(isOperate != isOn)
        {
            isOperate = isOn;
            NonOperateStateSet(isOn);
        }
    }

    protected virtual void NonOperateStateSet(bool isOn) { }

    protected virtual void FactoryOverlay() { }
}
