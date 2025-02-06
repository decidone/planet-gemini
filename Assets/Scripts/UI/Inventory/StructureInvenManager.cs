using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
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
    public Text cooldownText;
    public bool isOpened;
    [HideInInspector]
    public Production prod;
    [HideInInspector]
    public TankCtrl tank;

    //Transporter UI 전용
    public Toggle toggle;
    public InputField inputField;
    public Button subBtn;

    void OnEnable()
    {
        inputManager = InputManager.instance;
        inputManager.controls.Inventory.SlotLeftClick.performed += SlotLeftClick;
        inputManager.controls.Inventory.SlotRightClickHold.performed += SlotRightClickHold;
        inputManager.controls.Inventory.SlotLeftClick.performed += SlotShiftClick;
    }
    void OnDisable()
    {
        inputManager.controls.Inventory.SlotLeftClick.performed -= SlotLeftClick;
        inputManager.controls.Inventory.SlotRightClickHold.performed -= SlotRightClickHold;
        inputManager.controls.Inventory.SlotLeftClick.performed -= SlotShiftClick;
    }
    protected override void Update()
    {
        base.Update();
        if (isOpened)
        {
            if (prod)
            {
                progressBar.SetProgress(prod.GetProgress());
                if (prod.GetType().Name == "Furnace")
                {
                    energyBar.SetProgress(prod.GetFuel());
                }
            }
            else if(tank)
            {
                progressBar.SetProgress(tank.GetProgress());
            }
        }
    }

    void SlotShiftClick(InputAction.CallbackContext ctx)
    {
        if (inventory == null) return;
        if (!inputManager.shift) return;

        if (focusedSlot != null)
        {
            if (focusedSlot.item != null)
            {
                inventory.StrToPlayer(focusedSlot.slotNum);
                soundManager.PlayUISFX("ItemSelect");
            }
        }
    }

    public void SetCooldownText(float cooldown)
    {
        if (cooldown == 0)
        {
            cooldownText.text = "";
        }
        else
        {
            cooldownText.text = cooldown + "s";
        }
    }

    public void ReleaseInven()
    {
        ResetInvenOption();
        prod = null;
        progressBar.SetMaxProgress(1);
        energyBar.SetMaxProgress(1);
        cooldownText.text = "";
    }

    public void ResetInvenOption()
    {
        for (int i = 0; i < slots.Length; i++)  
        {
            Slot slot = slots[i];
            slot.ResetOption();
        }
    }

    public void ClearInvenOption()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            Slot slot = slots[i];
            slot.ResetOption();
            slot.ClearSlot();
        }
    }

    public void InvenInit()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            Slot slot = slots[i];
            slot.slotNum = i;
            slot.outputSlot = true;
        }
    }

    public void SetProd(Production _prod)
    {
        prod = _prod;
    }

    public void SetTank(TankCtrl _tank)
    {
        tank = _tank;
    }

    public void PlayerToStrInven(int pInvenSlotNum, Item item, int amount)
    {
        // input 슬롯으로 지정된 칸에 아이템을 넣을 때 사용
        bool isStorageBuilding = false;

        if (prod != null)
            isStorageBuilding = prod.isStorageBuilding;

        if (isStorageBuilding)
        {
            inventory.PlayerToStorage(pInvenSlotNum);
        }
        else
        {
            for (int i = 0; i < slots.Length; i++)
            {
                Slot slot = slots[i];
                if ((slot.inputItem.Contains(item)) && (slot.item == item || slot.item == null) && !slot.outputSlot)
                {
                    //if (amount + slot.amount > inventory.maxAmount)
                    //{
                    //    containable = inventory.maxAmount - slot.amount;
                    //}
                    //else
                    //{
                    //    containable = amount;
                    //}
                    //inventory.SlotAdd(slot.slotNum, item, containable);

                    inventory.PlayerToStr(pInvenSlotNum, slot.slotNum);

                    break;
                }
            }
        }
    }

    public void EmptySlot()
    {
        Debug.Log("EmptySlot");
        playerInven = gameManager.inventory;

        for (int i = 0; i < slots.Length; i++)
        {
            Slot slot = slots[i];
            if (slot.item != null)
            {
                playerInven.Add(slot.item, slot.amount);
                inventory.RemoveServerRpc(slot.slotNum);
            }
        }
    }

    //Transporter UI 전용
    public void ToggleControl()
    {
        if (prod != null && prod.TryGetComponent(out Transporter trBuild))
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

    public void TransporterSetting(bool toggleOn, int amount)
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
    //Transporter UI 전용

    //Portal UI 전용
    public void PortalProductionSet()
    {
        PortalSciManager portalSciManager = PortalSciManager.instance;
        foreach(var data in portalSciManager.UIBtnData)
        {
            data.Value.SetProduction(prod);
        }
    }
    //Portal UI 전용

    public override void OpenUI()
    {
        structureInfoUI.SetActive(true);
        inventoryUI.SetActive(true);
        Debug.Log("strOpenUI");
        gameManager.onUIChangedCallback?.Invoke(structureInfoUI);
        isOpened = true;
    }

    public override void CloseUI()
    {
        focusedSlot = null;
        structureInfoUI.SetActive(false);
        inventoryUI.SetActive(false);
        itemInfoWindow.CloseWindow();
        Debug.Log("strCloseUI");
        gameManager.onUIChangedCallback?.Invoke(structureInfoUI);
        isOpened = false;
    }

    public void OpenTankUI()
    {
        inventoryUI.SetActive(true);
    }

    public void CloseTankUI()
    {
        focusedSlot = null;
        inventoryUI.SetActive(false);
        itemInfoWindow.CloseWindow();
    }
}
