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
    public Inventory inventory;

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
        inventory = gameManager.GetComponent<Inventory>();

        CloseUI();
    }

    public void SetUI(ScienceBtn sciBtn)
    {
        InputFieldReset();

        scienceBtn = sciBtn;
        scienceInfoData = scienceBtn.scienceInfoData;
        for (int index = 0; index < itemObjList.Count; index++)
        {
            bool isActive = index < scienceInfoData.items.Count;

            if (isActive)
            {
                string itemName = scienceInfoData.items[index];
                Item item = itemsList.FirstOrDefault(x => x.name == itemName);

                if (item != null)
                {
                    if (itemObjList[index].TryGetComponent(out InfoNeedItemUi itemUi))
                    {
                        itemUi.icon.sprite = item.icon;
                        itemUi.AmountSet(scienceBtn.itemAmountList[index].Item1, scienceInfoData.amounts[index]);
                        int maxAmount = scienceBtn.itemAmountList[index].Item2 - scienceBtn.itemAmountList[index].Item1;
                        bool hasItem = inventory.totalItems.TryGetValue(ItemList.instance.itemDic[itemName], out int value);
                        bool isEnough = hasItem && value >= scienceInfoData.amounts[index];

                        if (value == 0)
                            itemUi.amount.color = Color.red;
                        else
                            itemUi.amount.color = isEnough ? Color.white : Color.yellow;

                        itemInputFields[index].InputFieldFGetData(maxAmount, value, hasItem);
                    }
                }
            }
            itemObjList[index].SetActive(isActive);
        }
    }

    void OkBtnFunc()
    {
        for (int i = 0; i < scienceInfoData.items.Count; i++) 
        {
            string itemName = scienceInfoData.items[i];
            Item item = itemsList.FirstOrDefault(x => x.name == itemName);

            if (item != null)
            {
                InputField inputField = itemInputFields[i].inputField;

                if (!int.TryParse(inputField.text, out int textInt))
                {
                    continue;
                }

                scienceBtn.ItemAddAmount(i, textInt);
                inventory.Sub(ItemList.instance.itemDic[itemName], textInt);
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
    }

    public void CloseUI()
    {
        InputFieldReset();
        this.gameObject.SetActive(false);
    }
}
