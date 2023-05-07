using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class SolidFactoryCtrl : FactoryCtrl
{
    [SerializeField]
    protected SolidFactoryData solidFactoryData;
    protected SolidFactoryData SolidFactoryData { set { solidFactoryData = value; } }

    public List<ItemProps> itemObjList = new List<ItemProps>();
    public List<Item> itemList = new List<Item>();

    public List<GameObject> outSameList = new List<GameObject>();

    [SerializeField]
    GameObject itemPref;
    protected IObjectPool<ItemProps> itemPool;

    protected bool itemGetDelay = false;
    protected bool itemSetDelay = false;

    public BoxCollider2D box2D = null;

    private void Awake()
    {
        box2D = GetComponent<BoxCollider2D>();

        itemPool = new ObjectPool<ItemProps>(CreateItemObj, OnGetItem, OnReleaseItem, OnDestroyItem, maxSize: 20);
    }
    // Start is called before the first frame update

    public void BeltGroupSendItem(ItemProps itemObj)
    {
        itemObjList.Add(itemObj);

        if (itemObjList.Count >= solidFactoryData.FullItemNum)
        {
            isFull = true;
        }
    }

    public void OnBeltItem(ItemProps itemObj)
    {
        itemObjList.Add(itemObj);

        if (GetComponent<BeltCtrl>())        
            GetComponent<BeltCtrl>().beltGroupMgr.GroupItem.Add(itemObj);       

        if (itemObjList.Count >= solidFactoryData.FullItemNum)
        {
            isFull = true;
        }
    }

    public void OnFactoryItem(ItemProps itemProps)
    {
        itemList.Add(itemProps.item);

        OnDestroyItem(itemProps);
        if (itemList.Count >= solidFactoryData.FullItemNum)
        {
            isFull = true;
        }
    }
    public void OnFactoryItem(Item item)
    {
        itemList.Add(item);

        if (itemList.Count >= solidFactoryData.FullItemNum)
        {
            isFull = true;
        }
    }

    private ItemProps CreateItemObj()
    {
        ItemProps item = Instantiate(itemPref).GetComponent<ItemProps>();
        item.SetPool(itemPool);
        return item;
    }

    private void OnGetItem(ItemProps item)
    {
        item.gameObject.SetActive(true);
    }
    private void OnReleaseItem(ItemProps item)
    {
        item.gameObject.SetActive(false);
    }
    private void OnDestroyItem(ItemProps item)
    {
        Destroy(item.gameObject, 0.4f);
    }

    public void ItemNumCheck()
    {
        if (itemList.Count < solidFactoryData.FullItemNum)
        {
            isFull = false;
        }

    }

    public void GetFluid(float getNum)
    {

    }

    public override void DisableColliders()
    {
        box2D.enabled = false;
    }

    public override void EnableColliders()
    {
        box2D.enabled = true;
    }
}
