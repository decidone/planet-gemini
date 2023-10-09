using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// UTF-8 설정
public class SplitterFilterManager : MonoBehaviour
{
    public GameObject spliterFilterUI;
    [SerializeField]
    SplitterFilterRecipe splitterFilterRecipe;

    SplitterCtrl splitter = null;

    public Slot[] slots;
    protected GameManager gameManager;

    [SerializeField]
    Button[] fillterMenuBtns;
    [SerializeField]
    ToggleButton[] fillterOnOffBtns;
    [SerializeField]
    Toggle[] reverseToggle;

    void Start()
    {
        gameManager = GameManager.instance;
        //SetInven(buildingInventory, sliterFilterUI);

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].amountText.gameObject.SetActive(false);
        }

        for (int i = 0; i < fillterMenuBtns.Length; i++)
        {
            int buttonIndex = i;
            fillterMenuBtns[i].onClick.AddListener(() => OpenSplitterFillterMenu(buttonIndex));
        }

        for (int i = 0; i < fillterOnOffBtns.Length; i++)
        {
            int buttonIndex = i;
            fillterOnOffBtns[i].onToggleOn.AddListener(() => SentFillterInfo(buttonIndex));
        }

        for (int i = 0; i < fillterOnOffBtns.Length; i++)
        {
            int toggleIndex = i;
            reverseToggle[i].onValueChanged.AddListener(isOn => Function_Toggle(toggleIndex));
        }
    }

    void OpenSplitterFillterMenu(int buttonIndex)
    {
        splitterFilterRecipe.OpenUI();
        splitterFilterRecipe.GetFillterNum(buttonIndex, "SplitterFilterManager");
    }

    private void Function_Toggle(int toggleIndex)
    {
        SentFillterInfo(toggleIndex);
    }

    public void SetSplitter(SplitterCtrl _split)
    {
        splitter = _split;
    }

    public void ReleaseInven()
    {
        ResetInvenOption();
        splitter = null;
    }

    public void ResetInvenOption()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            Slot slot = slots[i];
            slot.ResetOption();
        }
    }

    //public void InvenInit()
    //{
    //    for (int i = 0; i < slots.Length; i++)
    //    {
    //        Slot slot = slots[i];
    //        slot.outputSlot = true;
    //    }
    //}

    public void SetItem(Item _item, int slotIndex)
    {
        if (_item.name == "EmptyFilter")
        {
            splitter.SlotReset(slotIndex);
            fillterOnOffBtns[slotIndex].ButtonSetModle(false);
            reverseToggle[slotIndex].isOn = false;
            slots[slotIndex].ClearSlot();
        }
        else
        {
            if (slots[slotIndex].item == null)
            {
                slots[slotIndex].AddItem(_item, 1);
            }
            else if (slots[slotIndex].item != _item)
            {
                slots[slotIndex].ClearSlot();
                slots[slotIndex].AddItem(_item, 1);
            }
            else if (slots[slotIndex].item == _item)
                return;

            SentFillterInfo(slotIndex);
        }
    }

    public void OpenUI()
    {
        if (splitter != null)
        {
            GetFillterInfo();
        }

        spliterFilterUI.SetActive(true);
        gameManager.onUIChangedCallback?.Invoke(spliterFilterUI);
    }

    public void CloseUI()
    {
        spliterFilterUI.SetActive(false);
        splitterFilterRecipe.CloseUI();
        gameManager.onUIChangedCallback?.Invoke(spliterFilterUI);
    }

    void GetFillterInfo()
    {
        for (int i = 0; i < splitter.arrFilter.Length; i++)
        {
            fillterOnOffBtns[i].OpenSetting(splitter.arrFilter[i].isFilterOn);
            if (splitter.arrFilter[i].selItem != null)
            {
                slots[i].AddItem(splitter.arrFilter[i].selItem, 1);
            }
            else
                slots[i].ClearSlot();
        }
    }

    void SentFillterInfo(int i)
    {
        if (slots[i].item != null)
        {
            if (slots[i].item.name != "FullFilter")
            {
                splitter.FilterSet(i, fillterOnOffBtns[i].isOn, false, true, reverseToggle[i].isOn, slots[i].item);
            }
            else if (slots[i].item.name == "FullFilter")
            {
                splitter.FilterSet(i, fillterOnOffBtns[i].isOn, true, false, false, slots[i].item);
            }
            splitter.ItemFilterCheck();
        }
        else
            return;
    }

    public void UiReset()
    {
        if (splitter != null)
        {
            GetFillterInfo();
        }
        gameManager.onUIChangedCallback?.Invoke(spliterFilterUI);
    }
}