using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

// UTF-8 설정
public class PlayerController : NetworkBehaviour
{
    public Inventory inventory;

    List<GameObject> items = new List<GameObject>();
    List<GameObject> beltList = new List<GameObject>();

    public Collider2D circleColl;
    GameObject preBuilding;
    Building tempMiner;
    TempMinerUi tempMinerUI;
    int tempFullAmount;
    public int tempMinerCount;

    InputManager inputManager;
    bool isLoot;

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

    void Awake()
    {
        circleColl = GetComponent<CircleCollider2D>();
        tempMinerCount = 5;
        isLoot = false;
    }

    void Start()
    {
        
        tempFullAmount = 5;
        tempMinerCount = tempFullAmount;
        tempMiner = ResourcesManager.instance.tempMiner;
        tempMinerUI = ResourcesManager.instance.tempMinerUI;

        inventory = GameManager.instance.GetComponent<Inventory>();
        

        inputManager = InputManager.instance;
        if (!IsOwner) { return; }
        GameManager.instance.SetPlayer(this.gameObject);
        inputManager.controls.Player.Loot.performed += ctx => LootCheck();
        inputManager.controls.Player.Miner.performed += ctx => DeployMiner();
        inputManager.controls.Player.RightClick.performed += ctx => GetStrItem();
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
        ItemProps itemProps = collision.GetComponent<ItemProps>();
        BeltCtrl belt = collision.GetComponent<BeltCtrl>();

        if (itemProps)
            items.Add(collision.gameObject);
        else if (belt)
            beltList.Add(collision.gameObject);
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        ItemProps itemProps = collision.GetComponent<ItemProps>(); 
        BeltCtrl belt = collision.GetComponent<BeltCtrl>();

        if (itemProps && items.Contains(collision.gameObject))
            items.Remove(collision.gameObject);
        else if (belt && beltList.Contains(collision.gameObject))
            beltList.Remove(collision.gameObject);
    }

    void LootCheck() { isLoot = !isLoot; }

    void Loot()
    {
        foreach (GameObject item in items)
        {
            ItemProps itemProps = item.GetComponent<ItemProps>();
            if (itemProps)
            {
                int containableAmount = inventory.SpaceCheck(itemProps.item);
                if (itemProps.amount <= containableAmount)
                {
                    inventory.Add(itemProps.item, itemProps.amount);
                    items.Remove(item);
                    Destroy(item);

                    break;
                }
                else if (containableAmount != 0)
                {
                    inventory.Add(itemProps.item, containableAmount);
                    itemProps.amount -= containableAmount;
                }
                else
                {
                    Debug.Log("not enough space");
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
                int containableAmount = inventory.SpaceCheck(itemProps.item);
                if (itemProps.amount <= containableAmount)
                {
                    inventory.Add(itemProps.item, itemProps.amount);
                    beltCtrl.PlayerRootFunc(itemProps);
                }
                else if (containableAmount != 0)
                {
                    inventory.Add(itemProps.item, containableAmount);
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

    void DeployMiner()
    {
        if (tempMinerCount > 0)
        {
            preBuilding = GameManager.instance.preBuildingObj;
            preBuilding.SetActive(true);
            PreBuilding pre = preBuilding.GetComponent<PreBuilding>();
            pre.SetImage(tempMiner, true, tempMinerCount);
            pre.isEnough = true;
            TempBuildUI(true);
        }
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
                    inventory.Add(factItemList[i], 1);
                }
            }
            else if (hit.collider != null && hit.collider.TryGetComponent(out Production production))
            {
                var item = production.QuickPullOut();
                if (item.Item1 != null && item.Item2 > 0)
                    inventory.Add(item.Item1, item.Item2);
            }
        }
    }

    public void TempBuildUI(bool isOn)
    {
        tempMinerUI.StartMoveUIElementCoroutine(isOn, tempFullAmount, tempMinerCount);
    }

    public bool TempMinerCountCheck()
    {
        tempMinerUI.AmountTextSet(tempFullAmount, tempMinerCount);

        if (tempMinerCount > 0)
        {
            return true;
        }

        return false;
    }

    public void TempBuildSet()
    {
        if (tempMinerCount > 0) 
        {
            tempMinerCount--;
        }
        TempMinerCountCheck();
    }

    public void TempBuildSet(int amount)
    {
        if (tempMinerCount > 0)
        {
            tempMinerCount -= amount;
        }
        TempMinerCountCheck();
    }

    public void RemoveTempBuild()
    {
        tempMinerCount++;
        TempMinerCountCheck();
    }
}
