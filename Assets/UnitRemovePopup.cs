using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitRemovePopup : MonoBehaviour
{
    [SerializeField]
    GameObject popupPanel;
    [SerializeField]
    Button okBtn;
    [SerializeField]
    Button canselBtn;
    [SerializeField]
    Slot[] slots;
    [SerializeField]
    Text sellPriceAmount;
    UnitDrag unitDrag;
    Dictionary<string, Item> itemDic;

    GameManager gameManager;
    public bool isOpen
    {
        get { return popupPanel.activeSelf; }
    }

    #region Singleton
    public static UnitRemovePopup instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameManager.instance;
        unitDrag = gameManager.GetComponent<UnitDrag>();
        okBtn.onClick.AddListener(() => OkBtnFunc());
        canselBtn.onClick.AddListener(() => CanselBtnFunc());
        itemDic = ItemList.instance.itemDic;
    }

    public void OkBtnFunc()
    {
        unitDrag.UnitRemoveFunc();
        ClosePopup();
    }

    void CanselBtnFunc()
    {
        ClosePopup();
    }

    public void OpenPopup(Dictionary<(string, int), int> datas, int amount)
    {
        popupPanel.SetActive(true);

        for (int i = 0; i < slots.Length; i++)
        {
            if (i < datas.Count)
            {
                slots[i].gameObject.SetActive(true);
            }
            else
            {
                slots[i].gameObject.SetActive(false);
            }
        }

        int slotIndex = 0;

        foreach (var data in datas)
        {
            slots[slotIndex].icon.sprite = ItemList.instance.FindDataGetLevel(data.Key.Item1, data.Key.Item2 + 1).icon;
            slots[slotIndex].icon.enabled = true;
            slots[slotIndex].amountText.text = data.Value.ToString();
            slots[slotIndex].amountText.enabled = true;
            slotIndex++;
        }

        sellPriceAmount.text = amount.ToString();

        gameManager.onUIChangedCallback?.Invoke(popupPanel);
        gameManager.PopUpUISetting(true);
    }

    public void ClosePopup()
    {
        popupPanel.SetActive(false);
        GameManager.instance.onUIChangedCallback?.Invoke(popupPanel);
        gameManager.PopUpUISetting(false);
    }
}
