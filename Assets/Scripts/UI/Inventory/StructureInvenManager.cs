using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// UTF-8 설정
public class StructureInvenManager : InventoryManager
{
    [SerializeField]
    Inventory playerInven;
    [SerializeField]
    GameObject structureInfoUI;
    public ProgressBar progressBar;
    public ProgressBar energyBar;
    public bool isOpened;
    [HideInInspector]
    public Production prod;

    //TransportBuild UI 전용
    public Toggle toggle;
    public InputField inputField;
    public Button subBtn;

    protected override void Start()
    {
        base.Start();
        inputManager.controls.Inventory.SlotLeftClick.performed += ctx => SlotShiftClick();
    }

    protected override void Update()
    {
        base.Update();
        if (isOpened)
        {
            progressBar.SetProgress(prod.GetProgress());
            if (prod.GetType().Name == "Furnace")
            {
                energyBar.SetProgress(prod.GetFuel());
            }
        }
    }

    void SlotShiftClick()
    {
        if (inventory == null) return;
        if (!inputManager.shift) return;

        if (focusedSlot != null)
        {
            if (focusedSlot.item != null)
            {
                int containableAmount = playerInven.SpaceCheck(focusedSlot.item);
                if (focusedSlot.amount <= containableAmount)
                {
                    playerInven.Add(focusedSlot.item, focusedSlot.amount);
                    inventory.Remove(focusedSlot);
                }
                else if (containableAmount != 0)
                {
                    playerInven.Add(focusedSlot.item, containableAmount);
                    inventory.Sub(focusedSlot.slotNum, containableAmount);
                }
                else
                {
                    Debug.Log("not enough space");
                }
            }
        }
    }

    public void ReleaseInven()
    {
        ResetInvenOption();
        prod = null;
        progressBar.SetMaxProgress(1);
        energyBar.SetMaxProgress(1);
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

    public void SetProd(Production _prod)
    {
        prod = _prod;
    }

    public int InsertItem(Item item, int amount)
    {
        // input 슬롯으로 지정된 칸에 아이템을 넣을 때 사용
        int containable = 0;
        bool canInsertItem = false;

        if (prod != null)
            canInsertItem = prod.isStorageBuild;

        for (int i = 0; i < slots.Length; i++)
        {
            Slot slot = slots[i];
            if ((slot.inputItem.Contains(item) || canInsertItem) && (slot.item == item || slot.item == null)) 
            {
                if (amount + slot.amount > inventory.maxAmount)
                {
                    containable = inventory.maxAmount - slot.amount;
                }
                else
                {
                    containable = amount;
                }
                inventory.SlotAdd(slot.slotNum, item, containable);
                break;
            }
        }

        return containable;
    }

    public void EmptySlot()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            Slot slot = slots[i];
            if (slot.item != null)
            {
                playerInven.Add(slot.item, slot.amount);
                inventory.Remove(slot);
            }
        }
    }

    //TransportBuild UI 전용
    public void ToggleControl()
    {
        if (prod != null && prod.TryGetComponent(out TransportBuild trBuild))
        {
            int parsedValue;

            if (int.TryParse(inputField.text, out parsedValue))
            {
                trBuild.SendFuncSet(toggle.isOn, parsedValue);
            }
            else if(inputField.text == "")
            {
                trBuild.SendFuncSet(toggle.isOn, 0);
            }
        }
    }

    public void TransportBuildSetting(bool toggleOn, int amount)
    {
        if (toggleOn)
        {
            toggle.SetIsOnWithoutNotify(true);
        }
        else
        {
            toggle.SetIsOnWithoutNotify(false);
        }

        if (amount > 0)
            inputField.text = amount.ToString();
        else
            inputField.text = "";
    }
    //TransportBuild UI 전용

    public override void OpenUI()
    {
        structureInfoUI.SetActive(true);
        inventoryUI.SetActive(true);
        gameManager.onUIChangedCallback?.Invoke(structureInfoUI);
        isOpened = true;
    }

    public override void CloseUI()
    {
        structureInfoUI.SetActive(false);
        inventoryUI.SetActive(false);
        gameManager.onUIChangedCallback?.Invoke(structureInfoUI);
        isOpened = false;
    }
}
