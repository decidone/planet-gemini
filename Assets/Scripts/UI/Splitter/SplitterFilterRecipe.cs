using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// UTF-8 설정
public class SplitterFilterRecipe : InventoryManager
{
    List<Item> itemsList;
    [SerializeField]
    Sprite removeUI;
    public SplitterFilterManager splitter;
    public UnloaderManager unloader;
    string selectManager;
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

        itemInfoWindow = gameManager.inventoryUiCanvas.GetComponent<ItemInfoWindow>();
        soundManager = SoundManager.instance;
    }
    void OnEnable()
    {
        inputManager = InputManager.instance;
        inputManager.controls.Inventory.SplitterFilter.performed += FilterItemClick;
    }
    void OnDisable()
    {
        inputManager.controls.Inventory.SplitterFilter.performed -= FilterItemClick;
    }
    void FilterItemClick(InputAction.CallbackContext ctx)
    {
        if (focusedSlot != null)
        {
            if (focusedSlot.item != null)
            {
                if (selectManager == "SplitterFilterManager")
                    splitter.SetItem(focusedSlot.item, slotIndex);
                else if (selectManager == "UnloaderManager")
                    unloader.SetItem(focusedSlot.item);

                focusedSlot = null;
                CloseUI();
            }
        }
    }

    void SortItemTier()
    {
        for (int i = 0; i < 5; i++) //5는 -1을 제외한 아이템 Tier분류 수
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
        SetInven(inventory, inventoryUI);
        int[] slotNums = new int[itemsTierList[tier].Count];
        Item[] itemIndexs = new Item[itemsTierList[tier].Count];
        int[] itemAmounts = new int[itemsTierList[tier].Count];

        for (int i = 0; i < itemsTierList[tier].Count; i++)
        {
            slotNums[i] = i;
            itemAmounts[i] = 1;
        }

        itemIndexs = itemsTierList[tier].ToArray();
        inventory.NonNetSlotsAdd(slotNums, itemIndexs, itemAmounts, itemsTierList[tier].Count);
    }

    public void GetFillterNum(int buttonIndex, string manager)
    {
        slotIndex = buttonIndex;
        selectManager = manager;
    }

    private void ButtonClicked(int buttonIndex)
    {
        SetItemList(buttonIndex);
        soundManager.PlayUISFX("SidebarClick");
    }

    public override void OpenUI()
    {
        SetItemList(0);

        inventoryUI.SetActive(true);
        gameManager.onUIChangedCallback?.Invoke(inventoryUI);
    }

    public override void CloseUI()
    {
        focusedSlot = null;
        inventoryUI.SetActive(false);
        itemInfoWindow.CloseWindow();
        soundManager.PlayUISFX("CloseUI");
        gameManager.onUIChangedCallback?.Invoke(inventoryUI);
        slotIndex = -1;
    }
}
