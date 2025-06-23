using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// UTF-8 설정
public class ItemSpManager : InventoryManager
{
    List<Item> itemsList;
    ItemSpawner itemSpawner = null;

    [SerializeField]
    private GameObject itemTagsPanel;
    private Button[] itemTagsBtn;
    private List<List<Item>> itemsTierList;

    protected override void Start()
    {
        gameManager = GameManager.instance;
        itemsList = gameManager.GetComponent<ItemList>().itemList;
        itemInfoWindow = gameManager.inventoryUiCanvas.GetComponent<ItemInfoWindow>();

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].amountText.gameObject.SetActive(false);
        }

        itemsTierList = new List<List<Item>>();
        SortItemTier();

        itemTagsBtn = itemTagsPanel.GetComponentsInChildren<Button>();
        for (int i = 0; i < itemTagsBtn.Length; i++)
        {
            int buttonIndex = i;
            itemTagsBtn[i].onClick.AddListener(() => ButtonClicked(buttonIndex));
        }
        soundManager = SoundManager.instance;
    }
    void OnEnable()
    {
        inputManager = InputManager.instance;
        inputManager.controls.Inventory.ItemSpawner.performed += InvenClick;
    }
    void OnDisable()
    {
        inputManager.controls.Inventory.ItemSpawner.performed -= InvenClick;
    }
    void InvenClick(InputAction.CallbackContext ctx)
    {
        if (focusedSlot != null)
        {
            if (focusedSlot.item != null)
            {
                SetItem(focusedSlot.item);
                focusedSlot = null;
                CloseUI();
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

    public void SetItemSp(ItemSpawner _itemSp)
    {
        itemSpawner = _itemSp;
    }

    private void ButtonClicked(int buttonIndex)
    {
        SetItemList(buttonIndex);
        soundManager.PlayUISFX("SidebarClick");
    }

    public void ReleaseInven()
    {
        ResetInvenOption();
        itemSpawner = null;
    }

    public void ResetInvenOption()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            Slot slot = slots[i];
            slot.ResetOption();
        }
    }

    public void InvenInit()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            Slot slot = slots[i];
            slot.outputSlot = true;
        }
    }

    public void SetItem(Item _item)
    {
        int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(_item);
        itemSpawner.ItemSetServerRpc(itemIndex);
    }

    public override void OpenUI()
    {
        if(itemSpawner)
            itemSpawner.isUIOpened = true;
        SetItemList(0);
        inventoryUI.SetActive(true);
        gameManager.onUIChangedCallback?.Invoke(inventoryUI);
    }

    public override void CloseUI()
    {
        if (itemSpawner)
            itemSpawner.isUIOpened = false;
        focusedSlot = null;
        inventoryUI.SetActive(false);
        itemInfoWindow.CloseWindow();
        soundManager.PlayUISFX("CloseUI");
        gameManager.onUIChangedCallback?.Invoke(inventoryUI);
    }
}
