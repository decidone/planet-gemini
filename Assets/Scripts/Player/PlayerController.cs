using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

// UTF-8 설정
public class PlayerController : NetworkBehaviour
{
    GameManager gameManager;
    public List<GameObject> items = new List<GameObject>();
    List<GameObject> beltList = new List<GameObject>();

    public Collider2D circleColl;
    //PreBuilding preBuilding;
    //Building tempMiner;
    //TempMinerUi tempMinerUI;
    //int tempFullAmount;
    //public int tempMinerCount;
    //int tempMinerMaxCount;

    InputManager inputManager;
    ShopInteract nearShop;
    TeleportUI teleportUI;
    bool isLoot;
    bool isTeleportable;
    bool isMarketTeleportable;

    [Space]
    [Header ("Movement")]
    [SerializeField]
    float moveSpeed;
    [SerializeField]
    Rigidbody2D rb;
    [SerializeField]
    Animator animator;
    public Vector2 movement;
    float animTimer;

    public delegate void OnTeleported(int type);
    public OnTeleported onTeleportedCallback;

    void Awake()
    {
        gameManager = GameManager.instance;
        circleColl = GetComponent<CircleCollider2D>();
        nearShop = null;
        isLoot = false;
        isTeleportable = false;
        isMarketTeleportable = false;
    }

    void Start()
    {
        inputManager = InputManager.instance;
        teleportUI = TeleportUI.instance;

        if (!IsOwner) { return; }

        GameManager.instance.SetPlayer(this.gameObject);
        inputManager.controls.Player.Loot.performed += ctx => LootCheck();
        inputManager.controls.Player.RightClick.performed += ctx => GetStrItem();
        inputManager.controls.Player.Interaction.performed += ctx => Interact();
        //inputManager.controls.Player.Market.performed += ctx => TeleportMarket();

        GeminiNetworkManager.instance.onItemDestroyedCallback += ItemDestroyed;
    }

    void Update()
    {
        if (!IsOwner) { return; }

        if (isLoot)
            Loot();

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

    void FixedUpdate()
    {
        if (!IsOwner) { return; }

        rb.MovePosition(rb.position + moveSpeed * Time.fixedDeltaTime * movement.normalized);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsOwner) { return; }

        ItemProps itemProps = collision.GetComponent<ItemProps>();
        BeltCtrl belt = collision.GetComponent<BeltCtrl>();
        Portal portal = collision.GetComponent<Portal>();
        MarketPortal marketPortal = collision.GetComponent<MarketPortal>();
        Interactable interactable = collision.GetComponent<Interactable>();
        ShopInteract shop = collision.GetComponent<ShopInteract>();

        if (interactable)
            interactable.SpawnIcon();

        if (itemProps && !items.Contains(collision.gameObject))
            items.Add(collision.gameObject);
        else if (belt && !beltList.Contains(collision.gameObject))
            beltList.Add(collision.gameObject);

        if (portal)
        {
            isTeleportable = true;
            isMarketTeleportable = true;
        }

        if (marketPortal)
            isMarketTeleportable = true;

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
        ShopInteract shop = collision.GetComponent<ShopInteract>();

        if (interactable)
            interactable.DespawnIcon();

        if (itemProps && items.Contains(collision.gameObject))
            items.Remove(collision.gameObject);
        else if (belt && beltList.Contains(collision.gameObject))
            beltList.Remove(collision.gameObject);

        if (portal)
        {
            isTeleportable = false;
            isMarketTeleportable = false;
        }

        if (marketPortal)
            isMarketTeleportable = false;

        if (shop && GameManager.instance.isShopOpened)
        {
            nearShop.CloseUI();
            nearShop = null;
            GameManager.instance.isShopOpened = false;
        }
    }

    void Interact()
    {
        if (!gameManager.isPlayerInMarket)
        {
            if (isTeleportable && GameManager.instance.isMultiPlay)
            {
                teleportUI.SetBtnDefault();
                teleportUI.leftBtn.onClick.AddListener(TeleportWorld);
                teleportUI.rightBtn.onClick.AddListener(TeleportMarket);

                teleportUI.OpenUI();
            }
        }
        else
        {
            TeleportMarket();
        }
    }

    void TeleportWorld()
    {
        if (isTeleportable && GameManager.instance.isMultiPlay)
        {
            if (PreBuilding.instance.isBuildingOn)
                PreBuilding.instance.CancelBuild();
            Vector3 pos = GameManager.instance.Teleport();
            this.transform.position = pos;
            SoundManager.Instance.PlayBgmMapCheck();
            onTeleportedCallback?.Invoke(0);

            teleportUI.CloseUI();
            teleportUI.DisplayWorldName();
        }
    }

    void TeleportMarket()
    {
        if (isMarketTeleportable)
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

    void LootCheck() { isLoot = !isLoot; }

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
                    beltCtrl.PlayerRootFunc(itemProps);
                }
                else if (containableAmount != 0)
                {
                    gameManager.inventory.Add(itemProps.item, containableAmount);
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

    void GetStrItem()
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
    }
}
