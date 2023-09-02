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
    public GameObject[] needItemObj;
    public Image[] icon;
    public Text[] amount;

    GameManager gameManager;
    List<Item> itemsList = new List<Item>();

    List<NeedItem> needItems = new List<NeedItem>();

    public bool totalAmountsEnough = false;
    public Inventory inventory = null;

    ScienceInfoData preSciInfoData;
    TempScienceDb scienceDb;

    #region Singleton
    public static InfoWindow instance;
    BuildingInven buildingInven;

    bool isCoreSel = false;
    int preSciLevel = 0;
    string preSciName = null;

    #endregion

    private void Start()
    {
        buildingInven = gameManager.GetComponent<BuildingInven>();
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

        totalAmountsEnough = true;

        for (int index = 0; index < needItemObj.Length; index++)
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

                    icon[index].sprite = item.icon;
                    amount[index].text = scienceInfoData.amounts[index].ToString();
                    amount[index].color = isEnough ? Color.white : Color.red;
                    needItems.Add(new NeedItem(item, scienceInfoData.amounts[index]));
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

    public void SciUpgradeEnd()
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

        if(preSciName == "Core")
        {
            scienceDb.coreLevel = preSciLevel + 1;
        }
        scienceDb.SaveSciDb(preSciName, preSciLevel);
        buildingInven.Refresh();
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
