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
    [SerializeField]
    Text titleText;
    GameManager gameManager;
    List<Item> itemsList = new List<Item>();
    public List<ItemInputField> itemInputFields = new List<ItemInputField>();
    public List<GameObject> itemObjList = new List<GameObject>();
    ScienceBtn scienceBtn;
    ScienceInfoData scienceInfoData;
    SoundManager soundManager;
    List<int> useAmountList = new List<int>();
    ItemInfoWindow itemInfoWindow;

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
        soundManager = SoundManager.instance;
        gameManager = GameManager.instance;
        itemsList = ItemList.instance.itemList;
        itemInfoWindow = GameManager.instance.inventoryUiCanvas.GetComponent<ItemInfoWindow>();

        CloseUI();
    }

    public void SetUI(ScienceBtn sciBtn)
    {
        InputFieldReset();
        scienceBtn = sciBtn;
        scienceInfoData = scienceBtn.scienceInfoData;
        titleText.text = sciBtn.gameName;

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
                        itemUi.DataSet(item.icon, item.name);
                        itemUi.AmountSet(scienceBtn.itemAmountList[index].Item1, scienceInfoData.amounts[index]);
                        int maxAmount = scienceBtn.itemAmountList[index].Item2 - scienceBtn.itemAmountList[index].Item1;
                        bool hasItem = gameManager.inventory.totalItems.TryGetValue(ItemList.instance.itemDic[itemName], out int value);
                        bool isEnough = hasItem && value >= scienceInfoData.amounts[index];

                        if (scienceBtn.ItemFullCheck())
                        {
                            itemUi.amount.color = Color.green;
                            itemUi.InputFieldSet(false);
                        }
                        else
                        {
                            itemUi.InputFieldSet(true);
                            if (scienceBtn.itemAmountList[index].Item1 >= scienceInfoData.amounts[index])
                            {
                                itemUi.amount.color = Color.green;
                                itemUi.InputFieldSet(false);
                            }
                            else if (value == 0)
                                itemUi.amount.color = Color.red;                            
                            else
                                itemUi.amount.color = isEnough ? Color.white : Color.yellow;

                        }

                        itemInputFields[index].InputFieldFGetData(maxAmount, value, hasItem);
                    }
                }
            }
            itemObjList[index].SetActive(isActive);
        }
    }

    public void OkBtnFunc()
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

                int maxInputItemAmount = scienceBtn.itemAmountList[i].Item2 - scienceBtn.itemAmountList[i].Item1;

                if (maxInputItemAmount == 0)
                {
                    continue;
                }

                int itemSetAount = 0;

                if (maxInputItemAmount >= textInt)
                    itemSetAount = textInt;
                else
                    itemSetAount = maxInputItemAmount;

                ScienceDb.instance.ScienceWindowItemAdd(item, i, itemSetAount, scienceBtn.btnIndex, gameManager.isPlayerInHostMap);
            }
        }

        ScienceDb.instance.InfoWindowRefreshServerRpc(scienceBtn.btnIndex);
        CloseUI();
        soundManager.PlayUISFX("ButtonClick");
    }

    void CancelBtnFunc()
    {
        CloseUI();
        soundManager.PlayUISFX("ButtonClick");
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
        useAmountList.Clear();
    }

    public void InfoWindowRefresh(int btnIndex)
    {
        if (scienceBtn && btnIndex == scienceBtn.btnIndex)
        {
            SetUI(scienceBtn);
        }
    }

    public void CloseUI()
    {
        InputFieldReset();
        this.gameObject.SetActive(false);
        itemInfoWindow.CloseWindow();

        gameManager.onUIChangedCallback?.Invoke(this.gameObject);
        gameManager.PopUpUISetting(false);
    }
}
