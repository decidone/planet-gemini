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
    PreBuilding preBuilding;
    BeltPreBuilding beltPreBuilding;

    bool totalAmountsEnough = false;

    #region Singleton
    public static BuildingInfo instance;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }
    #endregion

    private void Start()
    {
        preBuilding = PreBuilding.instance;
        beltPreBuilding = BeltPreBuilding.instanceBeltBuilding;
    }

    public void BuildingClick()
    {
        if (ItemDragManager.instance.isDrag || GameManager.instance.isPlayerInMarket) return;
        
        if (selectBuildingData != null)
        {
            int sendAmount = CanBuildAmount();
            if (selectBuilding.item.name != "Belt")
            {
                beltPreBuilding.SwapBuilding();
                preBuilding.SetImage(selectBuilding, sendAmount, GameManager.instance.isPlayerInHostMap);
                preBuilding.isEnough = AmountsEnoughCheck();
            }
            else
            {
                preBuilding.SwapBuilding();
                beltPreBuilding.SetImage(selectBuilding, sendAmount, GameManager.instance.isPlayerInHostMap);
                beltPreBuilding.isEnough = AmountsEnoughCheck();
            }
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

        if (select.item.name != "Belt")
        {
            if (preBuilding.isBuildingOn)
            {
                if (selectBuilding != null && selectBuilding != select)
                {
                    preBuilding.ReSetImage();
                }
            }
        }
        else
        {
            if (beltPreBuilding.isBuildingOn)
            {
                if (selectBuilding != null && selectBuilding != select)
                {
                    beltPreBuilding.ReSetImage();
                }
            }
        }

        totalAmountsEnough = true;

        selectBuilding = select;

        selectBuildingData = buildingDatas;

        bool isEnough;

        for (int i = 0; i < selectBuildingData.GetItemCount(); i++)
        {
            int value;
            bool hasItem = GameManager.instance.inventory.totalItems.TryGetValue(ItemList.instance.itemDic[selectBuildingData.items[i]], out value);
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

    public void BuildingEnd(int amount)
    {
        for (int i = 0; i < buildingNeedList.Length; i++)
        {
            if (buildingNeedList[i].item != null)
            {
                Overall.instance.OverallConsumption(buildingNeedList[i].item, buildingNeedList[i].amount * amount);
                GameManager.instance.inventory.Sub(buildingNeedList[i].item, buildingNeedList[i].amount * amount);
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
                bool hasItem = GameManager.instance.inventory.totalItems.TryGetValue(ItemList.instance.itemDic[selectBuildingData.items[i]], out value);
                isEnough = hasItem && value >= selectBuildingData.amounts[i];

                if (isEnough)
                {
                    int maxAmount = 0;
                    if (selectBuildingData.amounts[i] > 0)
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
