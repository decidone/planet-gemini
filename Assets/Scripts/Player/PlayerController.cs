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
    GameManager gameManager;
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
    [Header ("Movement")]
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
    bool tankOn;
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

    void Awake()
    {
        gameManager = GameManager.instance;
        circleColl = GetComponent<CircleCollider2D>();
        capsuleColl = GetComponent<CapsuleCollider2D>();
        nearShop = null;
        isLoot = false;
    }

    void Start()
    {
        teleportUI = TeleportUI.instance;

        if (!IsOwner) { return; }

        GameManager.instance.SetPlayer(this.gameObject);
        GeminiNetworkManager.instance.onItemDestroyedCallback += ItemDestroyed;
        if (GameManager.instance.playerDataHp != -1)
        {
            PlayerStatus status = gameObject.GetComponent<PlayerStatus>();
            status.LoadGame();
            gameObject.transform.position = GameManager.instance.playerDataPos;
        }
    }

    void OnEnable()
    {
        inputManager = InputManager.instance;
        inputManager.controls.Player.Loot.performed += LootCheck;
        inputManager.controls.Player.RightClick.performed += RightClick;
        inputManager.controls.Player.LeftClick.performed += LeftClick;
        inputManager.controls.Player.Interaction.performed += Interact;
        inputManager.controls.Player.TankAttack.performed += TankAttack;
    }

    void OnDisable()
    {
        inputManager.controls.Player.Loot.performed -= LootCheck;
        inputManager.controls.Player.RightClick.performed -= RightClick;
        inputManager.controls.Player.LeftClick.performed -= LeftClick;
        inputManager.controls.Player.Interaction.performed -= Interact;
        inputManager.controls.Player.TankAttack.performed -= TankAttack;
    }

    void Update()
    {
        if (Time.timeScale == 0)
        {
            return;
        }

        if (!IsOwner) { return; }

        if (isLoot)
            Loot();

        if (!attackMotion)
        {
            movement = inputManager.controls.Player.Movement.ReadValue<Vector2>();
            animator.SetFloat("Horizontal", movement.x);
            animator.SetFloat("Vertical", movement.y);
            animator.SetFloat("Speed", movement.sqrMagnitude);

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

        if (Input.GetKeyDown(KeyCode.G))
        {
            tankOn = !tankOn;
            if (tankOn)
            {
                capsuleColl.size = new Vector2(2.25f, 2f);
                animator.Play("TankWalk");
            }
            else
            {
                capsuleColl.size = new Vector2(1f, 0.8f);
                if (tankAttackKeyPressed)
                {
                    TankAttackEnd();
                }

                animator.Play("Idle");
            }
        }
    }

    void FixedUpdate()
    {
        if (!IsOwner) { return; }


        if (tankOn)
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
        else
        {
            rb.MovePosition(rb.position + moveSpeed * Time.fixedDeltaTime * movement.normalized);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsOwner) { return; }

        ItemProps itemProps = collision.GetComponent<ItemProps>();
        BeltCtrl belt = collision.GetComponent<BeltCtrl>();
        Interactable interactable = collision.GetComponent<Interactable>();
        NPCInteract shop = collision.GetComponent<NPCInteract>();

        if (interactable)
            interactable.SpawnIcon();

        if (itemProps && !items.Contains(collision.gameObject))
            items.Add(collision.gameObject);
        else if (belt && !beltList.Contains(collision.gameObject))
            beltList.Add(collision.gameObject);

        if (shop && !GameManager.instance.isShopOpened)
        {
            nearShop = shop;
            nearShop.OpenUI();
            GameManager.instance.isShopOpened = true;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (!IsOwner) { return; }

        ItemProps itemProps = collision.GetComponent<ItemProps>();
        BeltCtrl belt = collision.GetComponent<BeltCtrl>();
        Portal portal = collision.GetComponent<Portal>();
        MarketPortal marketPortal = collision.GetComponent<MarketPortal>();
        Interactable interactable = collision.GetComponent<Interactable>();
        NPCInteract shop = collision.GetComponent<NPCInteract>();

        if (interactable)
            interactable.DespawnIcon();

        if (itemProps && items.Contains(collision.gameObject))
            items.Remove(collision.gameObject);
        else if (belt && beltList.Contains(collision.gameObject))
            beltList.Remove(collision.gameObject);

        if (portal || marketPortal)
            teleportUI.CloseUI();

        if (shop && GameManager.instance.isShopOpened)
        {
            nearShop.CloseUI();
            nearShop = null;
            GameManager.instance.isShopOpened = false;
        }
    }

    void Interact(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) { return; }

        if (!gameManager.isPlayerInMarket)
        {
            if (circleColl.IsTouchingLayers(LayerMask.GetMask("Portal")))
            {
                teleportUI.SetBtnDefault();
                teleportUI.leftBtn.onClick.AddListener(TeleportWorld);
                teleportUI.rightBtn.onClick.AddListener(TeleportMarket);

                teleportUI.OpenUI();
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

    public void TeleportLocal(Vector3 pos)
    {
        this.transform.position = pos;
        if (circleColl.IsTouchingLayers(LayerMask.GetMask("Portal")))
        {
            if (PreBuilding.instance.isBuildingOn)
                PreBuilding.instance.CancelBuild();

            this.transform.position = pos;
        }
    }

    void TeleportWorld()
    {
        if (circleColl.IsTouchingLayers(LayerMask.GetMask("Portal")))
        {
            if (PreBuilding.instance.isBuildingOn)
                PreBuilding.instance.CancelBuild();
            Vector3 pos = GameManager.instance.Teleport();
            this.transform.position = pos;
            SoundManager.instance.PlayerBgmMapCheck();
            onTeleportedCallback?.Invoke(0);

            teleportUI.CloseUI();
            teleportUI.DisplayWorldName();
        }
    }

    void TeleportMarket()
    {
        if (circleColl.IsTouchingLayers(LayerMask.GetMask("Portal")))
        {
            if (PreBuilding.instance.isBuildingOn)
                PreBuilding.instance.CancelBuild();
            Vector3 pos = GameManager.instance.TeleportMarket();
            this.transform.position = pos;
            onTeleportedCallback?.Invoke(1);

            teleportUI.CloseUI();
            teleportUI.DisplayWorldName();
        }
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
                    LootListManager.instance.DisplayLootInfo(itemProps.item, itemProps.amount);
                    beltCtrl.PlayerRootFunc(itemProps);
                }
                else if (containableAmount != 0)
                {
                    gameManager.inventory.Add(itemProps.item, containableAmount);
                    LootListManager.instance.DisplayLootInfo(itemProps.item, containableAmount);
                    itemProps.amount -= containableAmount;
                }
                else
                {
                    Debug.Log("not enough space");
                }
            }
        }
        GameManager.instance.BuildAndSciUiReset();
    }

    public void ItemDestroyed()
    {
        // items에서 null 제거
        // items.RemoveAll( x => !x);
        // 빈 콜백으로 둬도 클라이언트 아이템 복사버그가 해결 됨. 아마 컴포넌트를 리프레시 해주는 기능이 있는 듯
    }

    void RightClick(InputAction.CallbackContext ctx)
    {
        if (inputManager.ctrl)
        {
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero);
            if (hit.collider != null && hit.collider.TryGetComponent(out LogisticsCtrl factoryCtrl))
            {
                List<Item> factItemList = factoryCtrl.PlayerGetItemList();
                for (int i = 0; i < factItemList.Count; i++)
                {
                    gameManager.inventory.Add(factItemList[i], 1);
                }
            }
            else if (hit.collider != null && hit.collider.TryGetComponent(out Production production))
            {
                var item = production.QuickPullOut();
                if (item.Item1 != null && item.Item2 > 0)
                    gameManager.inventory.Add(item.Item1, item.Item2);
            }
        }

        if (tankAttackKeyPressed)
        {
            TankAttackEnd();
        }
    }

    void LeftClick(InputAction.CallbackContext ctx)
    {
        if (tankAttackKeyPressed)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            Vector3 dir = mousePos - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            var rot = Quaternion.identity;
            if (Quaternion.AngleAxis(angle + 180, Vector3.forward).z < 0)
                rot = Quaternion.AngleAxis(angle + 180, Vector3.forward);
            else
                rot = Quaternion.AngleAxis(angle, Vector3.forward);

            animator.SetFloat("Horizontal", dir.x);
            animator.SetFloat("Vertical", dir.y);
            animator.SetFloat("lastMoveX", dir.x);
            animator.SetFloat("lastMoveY", dir.y);

            dir.z = 0;
            Vector2 spawnPos = transform.position + dir.normalized * 1;

            NetworkObject bulletPool = NetworkObjectPool.Singleton.GetNetworkObject(attackFX, new Vector2(spawnPos.x, spawnPos.y), rot);
            if (!bulletPool.IsSpawned) bulletPool.Spawn(true); 

            bulletPool.TryGetComponent(out TowerSingleAttackFx fx);
            fx.GetTarget(mousePos, 10, gameObject, false);

            TankAttackEnd();

            StartCoroutine(StopMotion());
            reloading = true;
            ReloadingUISet(true);
            reloadTimer = 0;
        }
    }

    IEnumerator StopMotion()
    {            attackMotion = true;

        yield return new WaitForSeconds(stopTime);
        attackMotion = false;
    }


    void TankAttack(InputAction.CallbackContext ctx)
    {
        if (tankOn && !reloading)
        {
            tankAttackKeyPressed = true;
            if (!UpgradeRemoveBtn.instance.clickBtn)
                MouseSkin.instance.UnitCursorCursorSet(true);
        }
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
}
