using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// UTF-8 설정
public class PlayerController : MonoBehaviour
{
    public Inventory inventory;

    List<GameObject> items = new List<GameObject>();
    List<GameObject> beltList = new List<GameObject>();

    public Collider2D circleColl;
    [SerializeField]
    GameObject preBuilding;
    [SerializeField]
    Building tempMiner = null;
    [SerializeField]
    TempMinerUi tempMinerUI;

    int tempFullAmount;
    int tempMinerCount;

    InputManager inputManager;
    bool isLoot;

    void Awake()
    {
        circleColl = GetComponent<CircleCollider2D>();
        tempMinerCount = 5;
        isLoot = false;
    }

    void Start()
    {
        inputManager = InputManager.instance;
        inputManager.controls.Player.Loot.performed += ctx => LootCheck();
        inputManager.controls.Player.Miner.performed += ctx => DeployMiner();
        inputManager.controls.Player.RightClick.performed += ctx => GetStrItem();
        tempFullAmount = 5;
        tempMinerCount = tempFullAmount;
    }

    void Update()
    {
        if (isLoot)
            Loot();
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
            preBuilding.SetActive(true);
            PreBuilding.instance.SetImage(tempMiner, true);
            PreBuilding.instance.isEnough = true;
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

    public void RemoveTempBuild()
    {
        tempMinerCount++;
        TempMinerCountCheck();
    }
}
