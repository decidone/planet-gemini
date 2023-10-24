using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// UTF-8 설정
public class BuildingInfo : MonoBehaviour
{
    public GameObject itemImg = null;
    public GameObject buildingInfoPanel = null;
    [SerializeField]
    BuildingImgCtrl[] buildingNeedList;

    Building selectBuilding = null;
    BuildingData selectBuildingData = null;
    public GameObject preBuilding;

    bool totalAmountsEnough = false;

    public PlayerInvenManager playerInvenManager;
    public Inventory inventory;
    DragSlot dragSlot;

    #region Singleton
    public static BuildingInfo instance;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of recipeList found!");
            return;
        }
        instance = this;
    }
    #endregion

    private void Start()
    {
        dragSlot = DragSlot.instance;
    }

    public void BuildingClick()
    {
        if(dragSlot.slot.item != null) 
        {
            playerInvenManager.DragItemToInven();
        }
        if (selectBuildingData != null)
        {
            preBuilding.SetActive(true);
            int sendAmount = CanBuildAmount();
            PreBuilding.instance.SetImage(selectBuilding, false, sendAmount);
            PreBuilding.instance.isEnough = AmountsEnoughCheck();
        }
    }

    public void ClearArr()
    {
        for (int i = 0; i < buildingNeedList.Length; i++) 
        {
            buildingNeedList[i].item = null;
            buildingNeedList[i].amount = 0;
            buildingNeedList[i].gameObject.SetActive(false);
        }
        ResetBuildingData();
    }

    public void SetItemSlot(BuildingData buildingDatas, Building select)
    {
        ClearArr();
        if (preBuilding.activeSelf)
        {
            if (selectBuilding != null && selectBuilding != select)
            {
                PreBuilding.instance.ReSetImage();
            }
        }

        totalAmountsEnough = true;

        selectBuilding = select;

        selectBuildingData = buildingDatas;

        bool isEnough;

        for (int i = 0; i < selectBuildingData.GetItemCount(); i++)
        {
            int value;
            bool hasItem = inventory.totalItems.TryGetValue(ItemList.instance.itemDic[selectBuildingData.items[i]], out value);
            isEnough = hasItem && value >= selectBuildingData.amounts[i];

            if (isEnough && totalAmountsEnough)
                totalAmountsEnough = true;
            else
                totalAmountsEnough = false;

            buildingNeedList[i].gameObject.SetActive(true);
            buildingNeedList[i].AddItem(ItemList.instance.itemDic[selectBuildingData.items[i]], selectBuildingData.amounts[i], isEnough);
        }
    }

    public void SetItemSlot()
    {
        if (selectBuildingData != null && selectBuilding.item != null)
            SetItemSlot(selectBuildingData, selectBuilding);
    }

    void ResetBuildingData()
    {
        if (selectBuildingData != null && selectBuilding.item != null)        
            selectBuildingData = null;        
    }

    public void BuildingEnd()
    {
        for (int i = 0; i < buildingNeedList.Length; i++)
        {
            if(buildingNeedList[i].item != null)
            {
                inventory.Sub(buildingNeedList[i].item, buildingNeedList[i].amount);
            }
        }
        GameManager.instance.BuildAndSciUiReset();
        SetItemSlot();
    }

    public void BuildingEnd(int amount)
    {
        for (int i = 0; i < buildingNeedList.Length; i++)
        {
            if (buildingNeedList[i].item != null)
            {
                inventory.Sub(buildingNeedList[i].item, buildingNeedList[i].amount * amount);
            }
        }
        GameManager.instance.BuildAndSciUiReset();
        SetItemSlot();
    }

    public bool AmountsEnoughCheck()
    {
        SetItemSlot();
        return totalAmountsEnough;
    }

    public int CanBuildAmount()
    {
        int amount = 0;

        if (selectBuildingData != null && selectBuilding.item != null)
        {
            bool isEnough;

            for (int i = 0; i < selectBuildingData.GetItemCount(); i++)
            {
                int value;
                bool hasItem = inventory.totalItems.TryGetValue(ItemList.instance.itemDic[selectBuildingData.items[i]], out value);
                isEnough = hasItem && value >= selectBuildingData.amounts[i];

                if (isEnough)
                {
                    int maxAmount = 0;
                    if(selectBuildingData.amounts[i] > 0)
                        maxAmount = (value / selectBuildingData.amounts[i]);

                    if (amount == 0 || amount > maxAmount)             
                        amount = maxAmount;
                }
                else
                {
                    amount = 0;
                    break;
                }
            }
        }

        return amount;
    }    
}
