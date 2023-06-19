using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class InfoWindow : MonoBehaviour
{
    public Text nameText;
    public Text coreLvText;
    public GameObject[] needItemObj;
    public Image[] icon;
    public Text[] amount;

    GameManager gameManager;
    [SerializeField]
    List<Item> itemsList = new List<Item>();

    List<NeedItem> needItems = new List<NeedItem>();

    public bool totalAmountsEnough = false;
    public Inventory inventory = null;

    ScienceInfoData preSciInfoData;

    #region Singleton
    public static InfoWindow instance;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of InfoWindow found!");
            return;
        }
        instance = this;
    }
    #endregion

    public void SetNeedItem(ScienceInfoData scienceInfoData)
    {
        needItems.Clear();

        if (itemsList.Count == 0)
        {
            gameManager = GameManager.instance;
            itemsList = gameManager.GetComponent<ItemList>().itemList;
        }

        preSciInfoData = scienceInfoData;

        nameText.text = $"{scienceInfoData.name} Lv.{scienceInfoData.level}";
        coreLvText.text = $"Core Lv.{scienceInfoData.coreLv}";

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
    }

    public void SciUpgradeEnd()
    {
        List<NeedItem> itemsCopy = new List<NeedItem>(needItems);

        foreach (var needItem in itemsCopy)
        {
            if (needItem.item != null)
            {
                inventory.Sub(needItem.item, needItem.amount);
                SetNeedItem(preSciInfoData);
            }
        }
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