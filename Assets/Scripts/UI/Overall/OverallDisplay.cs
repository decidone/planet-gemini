using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverallDisplay : MonoBehaviour
{
    [SerializeField] GameObject displayObj;
    [SerializeField] GameObject overallSlotsObj;
    [SerializeField] GameObject overallSlotsPref;
    [SerializeField] ItemListSO itemListSO;
    List<Item> itemList;
    List<OverallSlot> overallSlots;
    SoundManager soundManager;
    #region Singleton
    public static OverallDisplay instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        itemList = itemListSO.itemSOList;
    }
    #endregion

    void Start()
    {
        soundManager = SoundManager.instance;
        overallSlots = new List<OverallSlot>();

        for (int i = 0; i < itemList.Count; i++)
        {
            GameObject slotObj = Instantiate(overallSlotsPref, overallSlotsObj.transform);
            OverallSlot overallSlot = slotObj.GetComponent<OverallSlot>();
            overallSlot.SlotInit(itemList[i]);
            overallSlots.Add(overallSlot);
        }
    }

    public void SetProdAmount(int order, int amount)
    {
        overallSlots[order].SetProdAmount(amount);
    }

    public void SetConsumptionAmount(int order, int amount)
    {
        overallSlots[order].SetConsumptionAmount(amount);
    }

    public void SetPurchasedAmount(int order, int amount)
    {
        overallSlots[order].SetPurchasedAmount(amount);
    }

    public void SetSoldAmount(int order, int amount)
    {
        overallSlots[order].SetSoldAmount(amount);
    }

    public void SetSentAmount(int order, int amount)
    {
        overallSlots[order].SetSentAmount(amount);
    }

    public void SetReceivedAmount(int order, int amount)
    {
        overallSlots[order].SetReceivedAmount(amount);
    }

    public void ToggleUI()
    {
        if (!displayObj.activeSelf)
            OpenUI();
        else
            CloseUI();
    }

    public void OpenUI()
    {
        displayObj.SetActive(true);
        GameManager.instance.onUIChangedCallback?.Invoke(displayObj);
        soundManager.PlayUISFX("SidebarClick");
    }

    public void CloseUI()
    {
        displayObj.SetActive(false);
        GameManager.instance.onUIChangedCallback?.Invoke(displayObj);
        soundManager.PlayUISFX("CloseUI");
    }
}
