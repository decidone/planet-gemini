using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SciItemSetWindow : MonoBehaviour
{
    [SerializeField]
    GameObject itemInput;
    [SerializeField]
    Button okBtn;
    [SerializeField]
    Button cancelBtn;
    [SerializeField]
    GameObject itemsRoot;

    GameManager gameManager;
    List<Item> itemsList = new List<Item>();
    public List<ItemInputField> itemInputFields = new List<ItemInputField>();
    public List<GameObject> itemObjList = new List<GameObject>();
    ScienceBtn scienceBtn;
    ScienceInfoData scienceInfoData;

    List<(string, ItemInputField)> financeInputField = new List<(string, ItemInputField)>();
    List<int> useAmountList = new List<int>();

    private void Awake()
    {
        for (int i = 0; i < 6; i++)
        {
            GameObject uI = Instantiate(itemInput);
            uI.transform.SetParent(itemsRoot.transform, false);
            itemObjList.Add(uI);
        }
        foreach (GameObject obj in itemObjList)
        {
            ItemInputField inputField = obj.GetComponentInChildren<ItemInputField>();
            itemInputFields.Add(inputField);
        }
        okBtn.onClick.AddListener(() => OkBtnFunc());
        cancelBtn.onClick.AddListener(() => CancelBtnFunc());
    }

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameManager.instance;
        itemsList = gameManager.GetComponent<ItemList>().itemList;

        CloseUI();
    }

    public void SetUI(ScienceBtn sciBtn)
    {
        InputFieldReset();
        scienceBtn = sciBtn;
        scienceInfoData = scienceBtn.scienceInfoData;
        int financeIndex = 0;

        for (int index = 0; index < itemObjList.Count; index++)
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
                        if (itemObjList[index].TryGetComponent(out InfoNeedItemUi itemUi))
                        {
                            itemUi.icon.sprite = item.icon;
                            itemUi.AmountSet(scienceBtn.itemAmountList[index].Item1, scienceInfoData.amounts[index]);
                            int maxAmount = scienceBtn.itemAmountList[index].Item2 - scienceBtn.itemAmountList[index].Item1;
                            bool hasItem = gameManager.inventory.totalItems.TryGetValue(ItemList.instance.itemDic[itemName], out int value);
                            bool isEnough = hasItem && value >= scienceInfoData.amounts[index];

                            if (value == 0)
                                itemUi.amount.color = Color.red;
                            else
                                itemUi.amount.color = isEnough ? Color.white : Color.yellow;

                            itemInputFields[index].InputFieldFGetData(maxAmount, value, hasItem);
                        }
                    }
                    else
                    {
                        if (itemObjList[index].TryGetComponent(out InfoNeedItemUi itemUi))
                        {
                            int useAmount = 0;
                            float value = 0;
                            if (itemName == "Diamond")
                            {
                                useAmount = 10000 * scienceInfoData.amounts[index];
                                value = gameManager.finance.finance / 10000;
                            }
                            else if (itemName == "Ruby")
                            {
                                useAmount = 100 * scienceInfoData.amounts[index];
                                value = gameManager.finance.finance / 100;
                            }
                            else if (itemName == "Amethyst")
                            {
                                useAmount = 1 * scienceInfoData.amounts[index];
                                value = gameManager.finance.finance;
                            }

                            itemUi.icon.sprite = item.icon;
                            itemUi.AmountSet(scienceBtn.itemAmountList[index].Item1, scienceInfoData.amounts[index]);
                            int maxAmount = scienceBtn.itemAmountList[index].Item2 - scienceBtn.itemAmountList[index].Item1;
                            
                            bool isEnough = gameManager.finance.finance >= useAmount;

                            if (!isEnough)
                                itemUi.amount.color = Color.red;
                            else
                                itemUi.amount.color = isEnough ? Color.white : Color.yellow;

                            financeInputField.Add((itemName, itemInputFields[index]));
                            itemInputFields[index].FinanceInputFieldFGetData(maxAmount, this, financeIndex, (int)value, (int)value > 0);
                            useAmountList.Add(0);
                            financeIndex++;
                        }
                    }
                }
            }
            itemObjList[index].SetActive(isActive);
        }
    }

    public void FinanceInputItemCheck(int index, int useAmount)
    {
        int amount = 0;
        if (financeInputField[index].Item1 == "Diamond")
        {
            amount = 10000 * useAmount;
        }
        else if (financeInputField[index].Item1 == "Ruby")
        {
            amount = 100 * useAmount;
        }
        else if (financeInputField[index].Item1 == "Amethyst")
        {
            amount = 1 * useAmount;
        }

        useAmountList[index] = amount;

        int totalAmount = 0;

        for (int i = 0; i < useAmountList.Count; i++)
        {
            totalAmount += useAmountList[i];
        }

        int tempFinanceAmount = gameManager.finance.finance - totalAmount;

        for (int i = 0; i < financeInputField.Count; i++)
        {
            if (financeInputField[i].Item1 == "Diamond")
            {
                financeInputField[i].Item2.invenItemAmount = tempFinanceAmount / 10000;
            }
            else if (financeInputField[i].Item1 == "Ruby")
            {
                financeInputField[i].Item2.invenItemAmount = tempFinanceAmount / 100;

            }
            else if (financeInputField[i].Item1 == "Amethyst")
            {
                financeInputField[i].Item2.invenItemAmount = tempFinanceAmount;
            }
        }
    }

    void OkBtnFunc()
    {
        int financeIndex = 0;
        for (int i = 0; i < scienceInfoData.items.Count; i++) 
        {
            string itemName = scienceInfoData.items[i];
            Item item = itemsList.FirstOrDefault(x => x.name == itemName);


            if (item != null)
            {
                InputField inputField = itemInputFields[i].inputField;

                if (!int.TryParse(inputField.text, out int textInt))
                {
                    if(item.tier == -1)
                        financeIndex++;
                    continue;
                }

                if(item.tier != -1)
                    gameManager.inventory.Sub(ItemList.instance.itemDic[itemName], textInt);
                else
                {
                    Debug.Log(financeIndex);
                    GameManager.instance.SubFinanceServerRpc(useAmountList[financeIndex]);
                    financeIndex++;
                }
                scienceBtn.ItemAddAmount(i, textInt);
                gameManager.inventory.Sub(ItemList.instance.itemDic[itemName], textInt);
                Overall.instance.OverallConsumption(ItemList.instance.itemDic[itemName], textInt);
                itemObjList[i].GetComponent<InfoNeedItemUi>().AmountSet(scienceBtn.itemAmountList[i].Item1, scienceBtn.itemAmountList[i].Item2);
            }
        }
        CloseUI();
    }

    void CancelBtnFunc()
    {
        CloseUI();
    }

    void InputFieldReset()
    {
        foreach (ItemInputField obj in itemInputFields)
        {
            if (obj.inputField)
            {
                obj.inputField.text = "";
            }
        }
        financeInputField.Clear();
        useAmountList.Clear();
    }

    public void CloseUI()
    {
        InputFieldReset();
        this.gameObject.SetActive(false);
    }
}
