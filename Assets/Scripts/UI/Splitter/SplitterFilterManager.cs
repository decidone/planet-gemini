using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
    bool isSmartSp = false;
    protected GameManager gameManager;

    [SerializeField]
    Slot[] slots;
    [SerializeField]
    Button[] recipeMenuBtns;
    [SerializeField]
    ToggleButton[] fillterOnOffBtns;
    [SerializeField]
    ToggleButton[] reverseToggle;
    SoundManager soundManager;

    void Start()
    {
        gameManager = GameManager.instance;
        soundManager = SoundManager.instance;
    }
     
    public void GetObjArr(Slot[] slotArr, Button[] fillterMenuBtnArr, ToggleButton[] fillterOnOffBtnArr, ToggleButton[] reverseToggleArr ,bool isSmart)
    {
        slots = slotArr.ToArray();
        recipeMenuBtns = fillterMenuBtnArr.ToArray();
        fillterOnOffBtns = fillterOnOffBtnArr.ToArray();
        reverseToggle = reverseToggleArr.ToArray();
        isSmartSp = isSmart;
        SetMenu();
    }

    public void SetMenu()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].amountText.gameObject.SetActive(false);
        }

        for (int i = 0; i < recipeMenuBtns.Length; i++)
        {
            int buttonIndex = i;
            recipeMenuBtns[i].onClick.RemoveAllListeners();
            recipeMenuBtns[i].onClick.AddListener(() => OpenSplitterFillterMenu(buttonIndex));
        }

        for (int i = 0; i < fillterOnOffBtns.Length; i++)
        {
            int buttonIndex = i;
            fillterOnOffBtns[i].onToggleOn.RemoveAllListeners();
            fillterOnOffBtns[i].onToggleOn.AddListener(() => SentFillterInfo(buttonIndex));
        }

        for (int i = 0; i < reverseToggle.Length; i++)
        {
            int toggleIndex = i;
            reverseToggle[i].onToggleOn.RemoveAllListeners();
            reverseToggle[i].onToggleOn.AddListener(() => Function_Toggle(toggleIndex));
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
        splitter = null;
        ResetInvenOption();
    }

    public void ResetInvenOption()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            Slot slot = slots[i];
            slot.ResetOption();
            slot.ClearSlot();
        }
    }

    public void SetItem(Item _item, int slotIndex)
    {
        if (isSmartSp)
        {
            if (_item.name == "UICancel")
            {
                splitter.SlotResetServerRpc(slotIndex);
                reverseToggle[slotIndex].isOn = false;
                slots[slotIndex].ClearSlot();
            }
            else if (slots[slotIndex].item == null)
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
        }

        SentFillterInfo(slotIndex);
    }

    public void OpenUI()
    {
        if (splitter != null)
        {
            GetFillterInfo();
            splitter.isUIOpened = true;
        }

        spliterFilterUI.SetActive(true);
        gameManager.onUIChangedCallback?.Invoke(spliterFilterUI);
    }

    public void CloseUI()
    {
        if (splitter)
            splitter.isUIOpened = false;
        spliterFilterUI.SetActive(false);
        splitterFilterRecipe.CloseUI();
        gameManager.onUIChangedCallback?.Invoke(spliterFilterUI);
        ReleaseInven();
    }

    void GetFillterInfo()
    {
        for (int i = 0; i < splitter.arrFilter.Length; i++)
        {
            if(fillterOnOffBtns.Length != 0)
                fillterOnOffBtns[i].OpenSetting(splitter.arrFilter[i].isFilterOn);
            if (reverseToggle.Length != 0)
                reverseToggle[i].OpenSetting(splitter.arrFilter[i].isReverseFilterOn);
            if (slots.Length != 0)
            {
                if (splitter.arrFilter[i].selItem != null)
                {
                    slots[i].AddItem(splitter.arrFilter[i].selItem, 1);
                }
                else
                    slots[i].ClearSlot();
            }
        }
    }

    void SentFillterInfo(int i)
    {
        soundManager.PlayUISFX("ButtonClick");

        if (isSmartSp)
        {
            int itemIndex = -1; // -1은 아이템이 없음을 의미
            if(slots[i].item)
                itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(slots[i].item);
            splitter.FilterSetServerRpc(i, fillterOnOffBtns[i].isOn, reverseToggle[i].isOn, itemIndex);
        }
        else if (!isSmartSp)
        {
            splitter.FilterSetServerRpc(i, fillterOnOffBtns[i].isOn);
        }
    }

    public void UIReset()
    {
        if (splitter != null)
        {
            GetFillterInfo();
        }
        gameManager.onUIChangedCallback?.Invoke(spliterFilterUI);
    }
}