using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// UTF-8 설정
public class SplitterFilterRecipe : InventoryManager
{
    List<Item> itemsList;
    public SplitterFilterManager splitter;
    int slotIndex = -1;
    [SerializeField]
    private GameObject itemTagsPanel;
    private Button[] itemTagsBtn;
    private List<List<Item>> itemsTierList;

    protected override void Start()
    {
        gameManager = GameManager.instance;
        itemsList = ItemList.instance.itemList;
        itemsTierList = new List<List<Item>>();
        SortItemTier();

        itemTagsBtn = itemTagsPanel.GetComponentsInChildren<Button>();
        for (int i = 0; i < itemTagsBtn.Length; i++)
        {
            int buttonIndex = i;
            itemTagsBtn[i].onClick.AddListener(() => ButtonClicked(buttonIndex));
        }
    }

    protected override void InputCheck()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (focusedSlot != null)
            {
                if (focusedSlot.item != null)
                {
                    splitter.SetItem(focusedSlot.item, slotIndex);
                    focusedSlot = null;
                    CloseUI();
                }
            }
        }
    }

    void SortItemTier()
    {
        for (int i = 0; i < 6; i++) //6은 -1을 제외한 아이템 Tier분류 수
        {
            List<Item> list = new List<Item>();
            for (int j = 0; j < itemsList.Count; j++)
            {
                if (itemsList[j].tier == i)
                {
                    list.Add(itemsList[j]);
                }
            }
            itemsTierList.Add(list);
        }
    }

    void SetItemList(int tier)
    {
        inventory.ResetInven();
        for (int i = 0; i < itemsTierList[tier].Count; i++)
        {
            inventory.Add(itemsTierList[tier][i], 1);
        }
        SetInven(inventory, inventoryUI);
    }

    public void GetFillterNum(int buttonIndex)
    {
        slotIndex = buttonIndex;
    }

    private void ButtonClicked(int buttonIndex)
    {
        SetItemList(buttonIndex);
    }

    public override void OpenUI()
    {
        SetItemList(0);

        inventoryUI.SetActive(true);
        gameManager.onUIChangedCallback?.Invoke(inventoryUI);
    }

    public override void CloseUI()
    {
        inventoryUI.SetActive(false);
        gameManager.onUIChangedCallback?.Invoke(inventoryUI);
        slotIndex = -1;
    }
}
