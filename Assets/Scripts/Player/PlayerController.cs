using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// UTF-8 설정
public class PlayerController : NetworkBehaviour
{
    public NetworkVariable<bool> isTeleporting = new NetworkVariable<bool>();

    GameManager gameManager;
    public PlayerStatus status;
    public List<GameObject> items = new List<GameObject>();
    List<GameObject> beltList = new List<GameObject>();

    public Collider2D circleColl;
    CapsuleCollider2D capsuleColl;
    //PreBuilding preBuilding;
    //Building tempMiner;
    //TempMinerUi tempMinerUI;
    //int tempFullAmount;
    //public int tempMinerCount;
    //int tempMinerMaxCount;

    InputManager inputManager;
    NPCInteract nearShop;
    TeleportUI teleportUI;
    bool isLoot;

    [Space]
    [Header("Movement")]
    [SerializeField]
    float moveSpeed;
    [SerializeField]
    float moveTankSpeed;
    [SerializeField]
    Rigidbody2D rb;
    [SerializeField]
    Animator animator;
    public Vector2 movement;
    float animTimer;

    public delegate void OnTeleported(int type);
    public OnTeleported onTeleportedCallback;

    // 탱크 테스트
    public bool tankOn;
    bool tankAttackKeyPressed;
    [SerializeField]
    GameObject attackFX;
    [SerializeField]
    Image reloadingBar;
    [SerializeField]
    Image reloadingBackBar;
    bool reloading;
    float reloadTimer;
    [SerializeField]
    float reloadInterval;
    [SerializeField]
    float slowdown;
    bool attackMotion;
    [SerializeField]
    float stopTime;
    [SerializeField]
    TankCtrl nearTank;
    public TankCtrl onTankData;
    public float visionRadius;
    float fogTimer;
    [SerializeField]
    PlayerTankTurret playerTankTurret;
    [SerializeField]
    GameObject tankTurret;


    void Awake()
    {
        gameManager = GameManager.instance;
        circleColl = GetComponent<CircleCollider2D>();
        capsuleColl = GetComponent<CapsuleCollider2D>();
        status = GetComponent<PlayerStatus>();
        nearShop = null;
        isLoot = false;
        PlayerTPSetServerRpc(true);
        ReloadingUISet(false);
    }

    void Start()
    {
        teleportUI = TeleportUI.instance;

        if (!IsOwner) { return; }

        GameManager.instance.SetPlayer(this.gameObject);
        GeminiNetworkManager.instance.onItemDestroyedCallback += ItemDestroyed;

        if (GameManager.instance.playerDataHp != -1)
        {
            status.LoadGame();
            gameObject.transform.position = GameManager.instance.playerDataPos;
        }

        MainGameSetting.instance.StopStopwatch();
        StartCoroutine(PlayerSet());
    }

    void OnEnable()
    {
        inputManager = InputManager.instance;
        inputManager.controls.Player.Loot.performed += LootCheck;
        inputManager.controls.Player.RightClick.performed += RightClick;
        inputManager.controls.Player.LeftClick.performed += LeftClick;
        inputManager.controls.Player.Interaction.performed += Interact;
        inputManager.controls.Player.TankAttack.performed += TankAttack;
        inputManager.controls.Player.TankInven.performed += TankInven;
    }

    void OnDisable()
    {
        inputManager.controls.Player.Loot.performed -= LootCheck;
        inputManager.controls.Player.RightClick.performed -= RightClick;
        inputManager.controls.Player.LeftClick.performed -= LeftClick;
        inputManager.controls.Player.Interaction.performed -= Interact;
        inputManager.controls.Player.TankAttack.performed -= TankAttack;
        inputManager.controls.Player.TankInven.performed -= TankInven;
    }

    void Update()
    {
        if (Time.timeScale == 0)
        {
            return;
        }

        fogTimer += Time.deltaTime;
        if (fogTimer > MapGenerator.instance.fogCheckCooldown)
        {
            if (isTeleporting.Value == false)
            {
                MapGenerator.instance.RemoveFogTile(new Vector3(transform.position.x, transform.position.y + 1, 0), visionRadius);
                fogTimer = 0;
            }
        }

        if (reloading)
        {
            reloadTimer += Time.deltaTime;
            reloadingBar.fillAmount = reloadTimer / reloadInterval;

            if (reloadTimer >= reloadInterval)
            {
                reloading = false;
                ReloadingUISet(false);
            }
        }

        if (!IsOwner) { return; }

        if (isLoot)
            Loot();

        if (!attackMotion)
        {
            movement = inputManager.controls.Player.Movement.ReadValue<Vector2>();
            float speed = movement.sqrMagnitude;
            animator.SetFloat("Horizontal", movement.x);
            animator.SetFloat("Vertical", movement.y);
            animator.SetFloat("Speed", speed);
            if (onTankData)
            {
                onTankData.FillFuel();
                if (speed > 0)
                {
                    onTankData.TankMove();
                }
            }

            // idle 모션 방향을 위해 마지막 움직인 방향을 저장
            animTimer += Time.deltaTime;
            if (Mathf.Abs(movement.x) == 1 || Mathf.Abs(movement.y) == 1)
            {
                // 0.1초마다 입력 상태를 저장
                if (animTimer > 0.1)
                {
                    animator.SetFloat("lastMoveX", movement.x);
                    animator.SetFloat("lastMoveY", movement.y);
                    animTimer = 0;
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (!IsOwner) { return; }

        if (tankOn && onTankData.fuel > 0)
        {
            if (!attackMotion)
            {
                if (!reloading)
                {
                    rb.MovePosition(rb.position + moveTankSpeed * Time.fixedDeltaTime * movement.normalized);
                }
                else
                {
                    rb.MovePosition(rb.position + moveTankSpeed / slowdown * Time.fixedDeltaTime * movement.normalized);
                }
            }
        }
        else if(!tankOn)
        {
            rb.MovePosition(rb.position + moveSpeed * Time.fixedDeltaTime * movement.normalized);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        BeltCtrl belt = collision.GetComponent<BeltCtrl>();
        if (belt && !beltList.Contains(collision.gameObject))
            beltList.Add(collision.gameObject);

        if (!IsOwner) { return; }
        ItemProps itemProps = collision.GetComponent<ItemProps>();
        Interactable interactable = collision.GetComponent<Interactable>();
        NPCInteract shop = collision.GetComponent<NPCInteract>();

        if (interactable)
        {
            interactable.SpawnIcon();
            if (collision.GetComponent<TankCtrl>() && nearTank == null)
            {
                TankSetServerRpc(collision.GetComponent<NetworkObject>());
            }
        }

        if (itemProps && !items.Contains(collision.gameObject))
            items.Add(collision.gameObject);

        if (shop && !GameManager.instance.isShopOpened)
        {
            nearShop = shop;
            nearShop.OpenUI();
            GameManager.instance.isShopOpened = true;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        BeltCtrl belt = collision.GetComponent<BeltCtrl>();
        if (belt && beltList.Contains(collision.gameObject))
            beltList.Remove(collision.gameObject);

        if (!IsOwner) { return; }
        ItemProps itemProps = collision.GetComponent<ItemProps>();
        Portal portal = collision.GetComponent<Portal>();
        MarketPortal marketPortal = collision.GetComponent<MarketPortal>();
        Interactable interactable = collision.GetComponent<Interactable>();
        NPCInteract shop = collision.GetComponent<NPCInteract>();

        if (interactable)
        {
            interactable.DespawnIcon();
            if (nearTank == collision.GetComponent<TankCtrl>())
            {
                if(NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
                    TankResetServerRpc();
            }
        }

        if (itemProps && items.Contains(collision.gameObject))
            items.Remove(collision.gameObject);

        if (portal || marketPortal)
            teleportUI.CloseUI();

        if (shop && GameManager.instance.isShopOpened)
        {
            nearShop.CloseUI();
            nearShop = null;
            GameManager.instance.isShopOpened = false;
        }
    }

    IEnumerator PlayerSet()
    {
        yield return new WaitForSeconds(1f);

        PlayerTPSetServerRpc(false);
        NetworkObjManager.instance.InitConnectors();

        if (!IsHost)
        {
            MonsterSpawnerManager.instance.SetCorruption();
        }
    }

    [ServerRpc (RequireOwnership = false)]
    public void PlayerTPSetServerRpc(bool isTP)
    {
        isTeleporting.Value = isTP;
    }

    [ServerRpc(RequireOwnership = false)]
    public void TankSetServerRpc(NetworkObjectReference networkObjectReference)
    {
        TankSetClientRpc(networkObjectReference);
    }

    [ClientRpc]
    public void TankSetClientRpc(NetworkObjectReference networkObjectReference)
    {
        networkObjectReference.TryGet(out NetworkObject networkObject);
        nearTank = networkObject.GetComponent<TankCtrl>();
    }

    [ServerRpc(RequireOwnership = false)]
    public void TankResetServerRpc()
    {
        TankResetClientRpc();
    }

    [ClientRpc]
    public void TankResetClientRpc()
    {
        nearTank = null;
    }

    void Interact(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) { return; }

        if (!gameManager.isPlayerInMarket)
        {
            if (circleColl.IsTouchingLayers(LayerMask.GetMask("Portal")))
            {
                teleportUI.SetBtnDefault();
                teleportUI.firstBtn.onClick.AddListener(TeleportWorld);
                teleportUI.secondBtn.onClick.AddListener(TeleportMarket);
                teleportUI.thirdBtn.onClick.AddListener(OpenMap);

                teleportUI.OpenUI();
            }
            else if (circleColl.IsTouchingLayers(LayerMask.GetMask("LocalPortal")))
            {
                OpenMap();
            }
            else if (!tankOn && nearTank != null)
            {
                nearTank.OpenUI();
                BasicUIBtns.instance.SwapFunc(false);
                TankOnFuncServerRpc();
            }
            else if (tankOn)
            {
                onTankData.ClientUISet();
                onTankData.CloseUI();
                TankOffFuncServerRpc();
            }
        }
        else
        {
            if (circleColl.IsTouchingLayers(LayerMask.GetMask("Portal")))
            {
                TeleportMarket();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void TankOnFuncServerRpc()
    {
        TankOnFuncClientRpc();
    }

    [ClientRpc]
    void TankOnFuncClientRpc()
    {
        if (!nearTank.playerOnTank)
        {
            transform.position = nearTank.transform.position;

            status.TankOnServerRpc(nearTank.GetComponent<NetworkObject>());
            onTankData = nearTank;

            onTankData.PlayerTankOnClientRpc();
            TankOnFunc();
        }
    }

    public void TankTurretSet(bool on)
    {
        tankTurret.SetActive(on);
        playerTankTurret.onTank = on;
    }

    void TankOnFunc()
    {
        tankOn = true;
        TankTurretSet(true);
        capsuleColl.size = new Vector2(2.25f, 2f);
        animator.Play("TankWalk");

        if (onTankData.reloading)
        {
            reloading = onTankData.reloading;
            reloadTimer = onTankData.reloadTimer;
            reloadInterval = onTankData.reloadInterval;
            ReloadingUISet(true);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void TankOffFuncServerRpc()
    {
        TankOffFuncClientRpc();
    }

    [ClientRpc]
    void TankOffFuncClientRpc()
    {
        var landData = TankLandingPos();
        if (landData.Item1)
        {
            tankOn = false;
            TankTurretSet(false);
            onTankData.PlayerTankOff(transform.position, status.tankHp, reloading, reloadTimer, reloadInterval);
            ReloadingUISet(false);
            status.TankOffServerRpc();
            onTankData = null;
            transform.position = landData.Item2;
            capsuleColl.size = new Vector2(1f, 0.8f);
            if (tankAttackKeyPressed)
            {
                TankAttackEnd();
            }
            animator.Play("Idle");
        }
    }

    public void ClientDisConn()
    {
        if (tankOn)
        {
            onTankData.PlayerTankOff(transform.position, status.tankHp, reloading, reloadTimer, reloadInterval);
        }
        if (IsServer && !GameManager.instance.isGameOver)
        {
            DataManager.instance.Save(0);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TankDestoryServerRpc()
    {
        TankDestoryClientRpc();
    }

    [ClientRpc]
    public void TankDestoryClientRpc()
    {
        tankOn = false;
        TankTurretSet(false);
        ReloadingUISet(false);
        onTankData.TankDestory();
        onTankData = null;
        capsuleColl.size = new Vector2(1f, 0.8f);
        if (tankAttackKeyPressed)
        {
            TankAttackEnd();
        }
        animator.Play("Idle");
    }

    (bool, Vector2) TankLandingPos()
    {
        Vector2[] landingPos = GetSurroundingPositions(transform.position);
        foreach (Vector2 newPos in landingPos)
        {
            int x = Mathf.FloorToInt(newPos.x);
            int y = Mathf.FloorToInt(newPos.y);
            Map map;
            if (gameManager.isPlayerInHostMap)
                map = GameManager.instance.hostMap;
            else
                map = GameManager.instance.clientMap;

            Cell cell = map.GetCellDataFromPos(x, y);

            if (cell.obj == null && cell.structure == null && cell.biome.biome != "lake" && cell.biome.biome != "cliff")
            {
                return (true, newPos);
            }
        }

        return (false, new Vector2());
    }

    Vector2[] GetSurroundingPositions(Vector2 playerPos)
    {
        float distance = 1.5f;
        Vector2[] positions = new Vector2[4];

        positions[0] = playerPos + new Vector2(distance, 0);  // 오른쪽
        positions[1] = playerPos + new Vector2(-distance, 0); // 왼쪽
        positions[2] = playerPos + new Vector2(0, distance);  // 위쪽
        positions[3] = playerPos + new Vector2(0, -distance); // 아래쪽

        return positions;
    }

    public void OpenMap()
    {
        MapCameraController.instance.ToggleMap();
        teleportUI.CloseUI();
    }

    public bool IsTeleportable()
    {
        if (circleColl.IsTouchingLayers(LayerMask.GetMask("Portal"))
            || circleColl.IsTouchingLayers(LayerMask.GetMask("LocalPortal")))
            return true;

        return false;
    }

    public bool TeleportLocal(Vector3 pos)
    {
        if (isTeleporting.Value == true)
            return false;

        if (circleColl.IsTouchingLayers(LayerMask.GetMask("Portal"))
            || circleColl.IsTouchingLayers(LayerMask.GetMask("LocalPortal")))
        {
            TeleportServerRpc(pos, gameManager.isPlayerInHostMap);
            //StartCoroutine(Teleport(pos));
            //this.transform.position = pos;
            return true;
        }

        return false;
    }

    void TeleportWorld()
    {
        if (isTeleporting.Value == true)
            return;

        if (circleColl.IsTouchingLayers(LayerMask.GetMask("Portal")))
        {
            if (PreBuilding.instance.isBuildingOn)
                PreBuilding.instance.CancelBuild();
            Vector3 pos = GameManager.instance.Teleport();
            TeleportServerRpc(pos, gameManager.isPlayerInHostMap);
            //StartCoroutine(Teleport(pos));
            //this.transform.position = pos;
            SoundManager.instance.PlaySFX(gameObject, "structureSFX", "PortalSound");
            SoundManager.instance.PlayerBgmMapCheck();
            onTeleportedCallback?.Invoke(0);

            teleportUI.CloseUI();
            teleportUI.DisplayWorldName();
        }
    }

    void TeleportMarket()
    {
        if (isTeleporting.Value == true)
            return;

        if (circleColl.IsTouchingLayers(LayerMask.GetMask("Portal")))
        {
            if (PreBuilding.instance.isBuildingOn)
                PreBuilding.instance.CancelBuild();
            Vector3 pos = GameManager.instance.TeleportMarket();
            TeleportServerRpc(pos, gameManager.isPlayerInHostMap);
            //StartCoroutine(Teleport(pos));
            //this.transform.position = pos;
            onTeleportedCallback?.Invoke(1);
            SoundManager.instance.PlaySFX(gameObject, "structureSFX", "PortalSound");

            teleportUI.CloseUI();
            teleportUI.DisplayWorldName();
        }
    }

    [ServerRpc (RequireOwnership = false)]
    public void TeleportServerRpc(Vector3 pos, bool isInHostMap)
    {
        StartCoroutine(Teleport(pos, isInHostMap));
    }

    [ClientRpc]
    public void TeleportClientRpc(Vector3 pos, bool isInHostMap)
    {
        if (IsOwner)
            this.transform.position = pos;

        if(tankOn)
            onTankData.isInHostMap = isInHostMap;
    }

    IEnumerator Teleport(Vector3 pos, bool isInHostMap)
    {
        isTeleporting.Value = true;

        yield return new WaitForSeconds(0.3f);
        TeleportClientRpc(pos, isInHostMap);

        yield return new WaitForSeconds(0.5f);
        isTeleporting.Value = false;
    }

    void LootCheck(InputAction.CallbackContext ctx) { isLoot = !isLoot; }

    void Loot()
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] != null)
            {
                ItemProps itemProps = items[i].GetComponent<ItemProps>();
                if (itemProps)
                {
                    gameManager.inventory.LootItem(items[i]);
                }
            }
        }

        if (beltList.Count > 0)
        {
            BeltLootServerRpc(IsServer);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void BeltLootServerRpc(bool isServer)
    {        
        foreach (GameObject belt in beltList)
        {
            List<ItemProps> beltItems = new List<ItemProps>();

            if (belt.TryGetComponent(out BeltCtrl beltCtrl))
            {
                beltItems = beltCtrl.PlayerRootItemCheck();
            }

            foreach (ItemProps itemProps in beltItems)
            {
                int containableAmount = gameManager.inventory.SpaceCheck(itemProps.item);
                if (itemProps.amount <= containableAmount)
                {
                    gameManager.inventory.Add(itemProps.item, itemProps.amount);
                    //LootListManager.instance.DisplayLootInfo(itemProps.item, itemProps.amount);
                    beltCtrl.beltGroupMgr.GroupItemLoot(beltCtrl, itemProps.beltGroupIndex, isServer);
                    //beltCtrl.PlayerRootFunc(itemProps);
                }
                else
                {
                    Debug.Log("not enough space");
                }
            }
        }
    }

    public void ItemDestroyed()
    {
        // items에서 null 제거
        // items.RemoveAll( x => !x);
        // 빈 콜백으로 둬도 클라이언트 아이템 복사버그가 해결 됨. 아마 컴포넌트를 리프레시 해주는 기능이 있는 듯
    }

    void RightClick(InputAction.CallbackContext ctx)
    {
        if (tankAttackKeyPressed && IsOwner)
        {
            TankAttackEnd();
        }
    }

    void LeftClick(InputAction.CallbackContext ctx)
    {
        if (!IsOwner)
            return;
        if (RaycastUtility.IsPointerOverUI(Input.mousePosition))
            return;

        if (tankAttackKeyPressed)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (onTankData.TankAttackCheck())
            {                
                var bulletData = onTankData.inventory.SlotCheck(0);
                onTankData.inventory.SlotSubServerRpc(0, 1);

                BulletSpawnServerRpc(mousePos, bulletData.item.name);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void BulletSpawnServerRpc(Vector3 mousePos, string bulletName)
    {
        Vector2 spawnPos = playerTankTurret.TurretAttackPos();
        Vector3 dir = mousePos - (Vector3)spawnPos;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        var rot = Quaternion.identity;
        if (Quaternion.AngleAxis(angle + 180, Vector3.forward).z < 0)
            rot = Quaternion.AngleAxis(angle + 180, Vector3.forward);
        else
            rot = Quaternion.AngleAxis(angle, Vector3.forward);

        dir.z = 0;
        BulletData bulletData = TwBulletDataManager.instance.bulletDic[bulletName];

        NetworkObject bulletPool = NetworkObjectPool.Singleton.GetNetworkObject(attackFX, new Vector2(spawnPos.x, spawnPos.y), rot);
        if (!bulletPool.IsSpawned) bulletPool.Spawn(true);

        bulletPool.TryGetComponent(out TowerSingleAttackFx fx);
        fx.GetTarget2(dir.normalized * 3, bulletData.damage, gameObject, bulletData.explosion);
        BulletSpawnClientRpc(dir);
    }

    [ClientRpc]
    void BulletSpawnClientRpc(Vector3 dir)
    {
        animator.SetFloat("Horizontal", dir.x);
        animator.SetFloat("Vertical", dir.y);
        animator.SetFloat("lastMoveX", dir.x);
        animator.SetFloat("lastMoveY", dir.y);

        TankAttackEnd();

        StartCoroutine(StopMotion());
        reloading = true;
        ReloadingUISet(true);
        reloadTimer = 0;
    }

    IEnumerator StopMotion()
    {            
        attackMotion = true;
        yield return new WaitForSeconds(stopTime);
        attackMotion = false;
    }


    public void TankAttack()
    {
        if (IsOwner && tankOn && !reloading)
        {
            tankAttackKeyPressed = true;
            if (!UpgradeRemoveBtn.instance.clickBtn)
                MouseSkin.instance.UnitCursorCursorSet(true);
        }
    }

    void TankAttack(InputAction.CallbackContext ctx)
    {
        TankAttack();
    }

    void TankAttackEnd()
    {
        tankAttackKeyPressed = false;
        MouseSkin.instance.ResetCursor();
    }

    void ReloadingUISet(bool isOn)
    {
        if (isOn)
        {
            reloadingBar.enabled = true;
            reloadingBackBar.enabled = true;
        }
        else
        {
            reloadingBar.enabled = false;
            reloadingBackBar.enabled = false;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void LoadDataSetTankServerRpc(float tankHp, float tankMaxHp)
    {
        GameObject spawnobj = Instantiate(GeminiNetworkManager.instance.unitListSO.userUnitList[3],transform.position, Quaternion.identity);
        spawnobj.TryGetComponent(out NetworkObject netObj);
        if (!netObj.IsSpawned) spawnobj.GetComponent<NetworkObject>().Spawn(true);

        LoadDataSetTankClientRpc(netObj, tankHp, tankMaxHp);
    }

    [ClientRpc]
    void LoadDataSetTankClientRpc(NetworkObjectReference networkObjectReference, float tankHp, float tankMaxHp)
    {
        networkObjectReference.TryGet(out NetworkObject networkObject);
        networkObject.gameObject.SetActive(false);
        networkObject.TryGetComponent(out TankCtrl tank);
        tank.PlayerOnTankLoad(tankHp, tankMaxHp);

        status.TankOnServerRpc(tank.GetComponent<NetworkObject>());
        onTankData = tank;
        TankOnFunc();
    }

    public void TankSaveFunc()
    {
        if (onTankData)
        {
            onTankData.hp = status.tankHp;
            onTankData.maxHp = status.tankMaxHp;
            onTankData.transform.position = transform.position;
        }
    }

    public void TankInven()
    {
        if (onTankData)
        {
            if (onTankData.tankUIOpen)
                onTankData.CloseUI();
            else
                onTankData.OpenUI();
        }
    }

    void TankInven(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) { return; }

        TankInven();
    }
}
