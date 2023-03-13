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

    [SerializeField]
    GameObject itemPref;
    protected IObjectPool<ItemProps> itemPool;

    protected bool itemGetDelay = false;
    protected bool itemSetDelay = false;

    private void Awake()
    {
        itemPool = new ObjectPool<ItemProps>(CreateItemObj, OnGetItem, OnReleaseItem, OnDestroyItem, maxSize: 20);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnBeltItem(ItemProps itemObj)
    {
        itemObjList.Add(itemObj);

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
        if (itemObjList.Count < solidFactoryData.FullItemNum)
            isFull = false;
    }

    public void GetFluid(float getNum)
    {

    }
}
