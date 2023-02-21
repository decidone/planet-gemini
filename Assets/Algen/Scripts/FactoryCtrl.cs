using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class FactoryCtrl : MonoBehaviour
{
    [SerializeField]
    public FactoryData factoryData;
    public FactoryData FactoryData { set { factoryData = value; } }

    public List<ItemProps> itemObjList = new List<ItemProps>();
    public List<Item> itemList = new List<Item>();

    public bool isFull = false;

    public int dirNum = 0;

    public GameObject itemPref;
    public IObjectPool<ItemProps> itemPool;

    public bool itemGetDelay = false;
    public bool itemSetDelay = false;

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

        if (itemObjList.Count >= factoryData.FullItemNum)
        {
            isFull = true;
        }
    }
    public void OnFactoryItem(ItemProps itemProps)
    {
        itemList.Add(itemProps.item);

        OnDestroyItem(itemProps);
        if (itemList.Count >= factoryData.FullItemNum)
        {
            isFull = true;
        }
    }
    public void OnFactoryItem(Item item)
    {
        itemList.Add(item);

        if (itemList.Count >= factoryData.FullItemNum)
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
        if (itemObjList.Count < factoryData.FullItemNum)
            isFull = false;
    }
}
