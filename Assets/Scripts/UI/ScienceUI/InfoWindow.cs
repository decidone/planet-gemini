using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

// UTF-8 설정
public class InfoWindow : MonoBehaviour
{
    public Text nameText;
    public Text infoText;
    [SerializeField]
    RectTransform infoRT;
    float[] infoPosYSize = new float[2] { -303, -240 };
    [SerializeField]
    RectTransform menuRT;
    float[] menuHeightSize = new float[2] { 400, 337 };
    float[] menuCoreHeightSize = new float[2] { 280, 200 };

    public List<GameObject> needItemObj = new List<GameObject>();
    public GameObject needItemUI;
    public GameObject needItemRoot;
    ScienceBtn scienceBtn;
    GameManager gameManager;
    List<Item> itemsList = new List<Item>();

    List<NeedItem> needItems = new List<NeedItem>();

    public bool totalAmountsEnough = false;

    ScienceInfoData preSciInfoData;
    ScienceDb scienceDb;

    public static InfoWindow instance;
    BuildingInven buildingInven;

    bool isCoreSel = false;
    int preSciLevel = 0;
    string preSciName = null;

    private void Awake()
    {
        if (needItemRoot != null)
        {
            for (int i = 0; i < 6; i++) 
            {
                GameObject uI = Instantiate(needItemUI);
                uI.transform.SetParent(needItemRoot.transform, false);
                needItemObj.Add(uI);
            }
        }
    }

    private void Start()
    {
        gameManager = GameManager.instance;
        buildingInven = gameManager.GetComponent<BuildingInven>();
        this.gameObject.SetActive(false);
    }

    public void SetNeedItem(ScienceInfoData scienceInfoData, string name, bool isCore)
    {
        instance = this;

        needItems.Clear();
        nameText.text = $"{name}";

        if (!isCore)
        {
            infoText.text = scienceInfoData.info;
        }
    }

    public void SetNeedItem(ScienceInfoData scienceInfoData, string name, int level ,bool isCore , ScienceBtn sciBtn)
    {
        instance = this;

        needItems.Clear();
        isCoreSel = isCore;
        preSciLevel = level;
        preSciName = name;
        scienceBtn = sciBtn;
        if (itemsList.Count == 0)
        {
            itemsList = new List<Item>();
            itemsList = gameManager.GetComponent<ItemList>().itemList;
        }
        if(scienceDb == null)
            scienceDb = gameManager.GetComponent<ScienceDb>();

        preSciInfoData = scienceInfoData;

        if (!isCore)
        {
            nameText.text = $"{name}";
            infoText.text = scienceInfoData.info;
            Vector2 anchoredPosition = infoRT.anchoredPosition;
            Vector2 sizeDelta = menuRT.sizeDelta;

            if (scienceInfoData.items.Count > 3)
            {
                anchoredPosition.y = infoPosYSize[0];
                sizeDelta.y = menuHeightSize[0];
            }
            else
            {
                anchoredPosition.y = infoPosYSize[1];
                sizeDelta.y = menuHeightSize[1];
            }
            infoRT.anchoredPosition = anchoredPosition;
            menuRT.sizeDelta = sizeDelta;
        }
        else
        {
            nameText.text = $"{name} Lv." + level;
            Vector2 sizeDelta = menuRT.sizeDelta;

            if (scienceInfoData.items.Count > 3)
            {
                sizeDelta.y = menuCoreHeightSize[0];
            }
            else
            {
                sizeDelta.y = menuCoreHeightSize[1];
            }
            menuRT.sizeDelta = sizeDelta;
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
                    if (item.tier != -1)
                    {
                        int value;
                        bool hasItem = gameManager.inventory.totalItems.TryGetValue(ItemList.instance.itemDic[itemName], out value);
                        bool isEnough = hasItem && value >= scienceInfoData.amounts[index];

                        if (isEnough && totalAmountsEnough)
                            totalAmountsEnough = true;
                        else
                            totalAmountsEnough = false;

                        if (needItemObj[index].TryGetComponent(out InfoNeedItemUi itemUi))
                        {
                            itemUi.icon.sprite = item.icon;
                            itemUi.AmountSet(scienceBtn.itemAmountList[index].Item1, scienceInfoData.amounts[index]);
                            if (scienceBtn.ItemFullCheck())
                            {
                                itemUi.amount.color = Color.white;
                            }
                            else
                            {
                                if (value == 0)
                                    itemUi.amount.color = Color.red;
                                else
                                    itemUi.amount.color = isEnough ? Color.white : Color.yellow;
                            }

                            needItems.Add(new NeedItem(item, scienceInfoData.amounts[index]));
                        }
                    }
                    else
                    {
                        int useAmount = 0;

                        if (itemName == "Diamond")
                        {
                            useAmount = 10000 * scienceInfoData.amounts[index];
                        }
                        else if (itemName == "Ruby")
                        {
                            useAmount = 100 * scienceInfoData.amounts[index];
                        }
                        else if (itemName == "Amethyst")
                        {
                            useAmount = 1 * scienceInfoData.amounts[index];
                        }

                        bool isEnough = gameManager.finance.finance >= useAmount;  // 앞에서 사용하고 남은 금액 보다 많은지

                        if (isEnough && totalAmountsEnough)
                            totalAmountsEnough = true;
                        else
                            totalAmountsEnough = false;

                        if (needItemObj[index].TryGetComponent(out InfoNeedItemUi itemUi))
                        {
                            itemUi.icon.sprite = item.icon;
                            itemUi.AmountSet(scienceBtn.itemAmountList[index].Item1, scienceInfoData.amounts[index]);
                            if (scienceBtn.ItemFullCheck())
                            {
                                itemUi.amount.color = Color.white;
                            }
                            else
                            {
                                if (!isEnough)
                                    itemUi.amount.color = Color.red;
                                else
                                    itemUi.amount.color = isEnough ? Color.white : Color.yellow;
                            }

                            needItems.Add(new NeedItem(item, scienceInfoData.amounts[index]));
                        }
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
        SetNeedItem(preSciInfoData, preSciName, preSciLevel, isCoreSel, scienceBtn);
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
