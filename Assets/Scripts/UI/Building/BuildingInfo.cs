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
    }

    public void BuildingClick()
    {
        if (ItemDragManager.instance.isDrag) return;
        
        if (selectBuildingData != null)
        {
            //preBuilding.SetActive(true);
            int sendAmount = CanBuildAmount();
            preBuilding.SetImage(selectBuilding, sendAmount, GameManager.instance.isPlayerInHostMap);
            preBuilding.isEnough = AmountsEnoughCheck();
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
        if (preBuilding.isBuildingOn)
        {
            if (selectBuilding != null && selectBuilding != select)
            {
                preBuilding.ReSetImage();
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

    public void BuildingEnd()
    {
        for (int i = 0; i < buildingNeedList.Length; i++)
        {
            if(buildingNeedList[i].item != null)
            {
                GameManager.instance.inventory.Sub(buildingNeedList[i].item, buildingNeedList[i].amount);
                Overall.instance.OverallConsumption(buildingNeedList[i].item, buildingNeedList[i].amount);
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
                GameManager.instance.inventory.Sub(buildingNeedList[i].item, buildingNeedList[i].amount * amount);
                Overall.instance.OverallConsumption(buildingNeedList[i].item, buildingNeedList[i].amount * amount);
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
