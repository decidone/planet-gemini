using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

// UTF-8 설정
public class InfoWindow : MonoBehaviour
{
    public Text nameText;
    public Text coreLvText;
    public Text infoText;
    public List<GameObject> needItemObj = new List<GameObject>();
    public GameObject needItemUI;
    public GameObject needItemRoot;

    GameManager gameManager;
    List<Item> itemsList = new List<Item>();

    List<NeedItem> needItems = new List<NeedItem>();

    public bool totalAmountsEnough = false;
    public Inventory inventory;

    ScienceInfoData preSciInfoData;
    TempScienceDb scienceDb;

    public static InfoWindow instance;
    BuildingInven buildingInven;

    bool isCoreSel = false;
    int preSciLevel = 0;
    string preSciName = null;

    private void Awake()
    {
        for (int i = 0; i < 6; i++) 
        {
            GameObject uI = Instantiate(needItemUI);
            uI.transform.SetParent(needItemRoot.transform, false);
            needItemObj.Add(uI);
        }
        this.gameObject.SetActive(false);
    }

    private void Start()
    {
        buildingInven = gameManager.GetComponent<BuildingInven>();
        inventory = gameManager.GetComponent<Inventory>();
    }

    public void SetNeedItem(ScienceInfoData scienceInfoData, string name, int level ,bool isCore)
    {
        instance = this;

        needItems.Clear();
        isCoreSel = isCore;
        preSciLevel = level;
        preSciName = name;
        if (itemsList.Count == 0)
        {
            gameManager = GameManager.instance;
            itemsList = gameManager.GetComponent<ItemList>().itemList;
        }
        if(scienceDb == null)
            scienceDb = gameManager.GetComponent<TempScienceDb>();

        preSciInfoData = scienceInfoData;

        nameText.text = $"{name} Lv.{level}";
        if(coreLvText != null)
        {
            coreLvText.text = $"Core Lv.{scienceInfoData.coreLv}";

            if (scienceInfoData.coreLv <= scienceDb.coreLevel)
                coreLvText.color = Color.white;
            else
                coreLvText.color = Color.red;
        }

        if(!isCore)
        {
            infoText.text = scienceInfoData.info;
        }

        totalAmountsEnough = true;

        for (int index = 0; index < needItemObj.Count; index++)
        {
            bool isActive = index < scienceInfoData.items.Count;

            if (isActive)
            {
                string itemName = scienceInfoData.items[index];
                Item item = itemsList.FirstOrDefault(x => x.name == itemName);

                if (item != null)
                {
                    int value;
                    bool hasItem = inventory.totalItems.TryGetValue(ItemList.instance.itemDic[itemName], out value);
                    bool isEnough = hasItem && value >= scienceInfoData.amounts[index];

                    if (isEnough && totalAmountsEnough)
                        totalAmountsEnough = true;
                    else
                        totalAmountsEnough = false;

                    if(needItemObj[index].TryGetComponent(out InfoNeedItemUi itemUi))
                    {
                        itemUi.icon.sprite = item.icon;
                        itemUi.amount.text = scienceInfoData.amounts[index].ToString();
                        itemUi.amount.color = isEnough ? Color.white : Color.red;
                        needItems.Add(new NeedItem(item, scienceInfoData.amounts[index]));
                    }
                }
            }
            needItemObj[index].SetActive(isActive);
        }
        if (totalAmountsEnough && scienceInfoData.coreLv <= scienceDb.coreLevel)
            totalAmountsEnough = true;
        else
            totalAmountsEnough = false;
    }

    public void SetNeedItem()
    {
        SetNeedItem(preSciInfoData, preSciName, preSciLevel, isCoreSel);
    }

    public void SciUpgradeStart()
    {
        List<NeedItem> itemsCopy = new List<NeedItem>(needItems);

        foreach (var needItem in itemsCopy)
        {
            if (needItem.item != null)
            {
                inventory.Sub(needItem.item, needItem.amount);
            }
        }
        SetNeedItem();
        totalAmountsEnough = false;
    }
}

public class NeedItem
{
    public Item item;
    public int amount;

    public NeedItem(Item item, int amount)
    {
        this.item = item;
        this.amount = amount;
    }
}
