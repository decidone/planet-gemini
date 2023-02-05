using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactoryCtrl : MonoBehaviour
{
    [SerializeField]
    public FactoryData factoryData;
    public FactoryData FactoryData { set { factoryData = value; } }

    public List<ItemProps> itemList = new List<ItemProps>();
    public bool isFull = false;

    public int dirNum = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //ItemNumCheck();
    }

    public void AddItem(ItemProps item)
    {
        itemList.Add(item);

        if (itemList.Count >= factoryData.FullItemNum)
            isFull = true;
    }

    public void ItemNumCheck()
    {
        if (itemList.Count < factoryData.FullItemNum)
            isFull = false;
    }
}
