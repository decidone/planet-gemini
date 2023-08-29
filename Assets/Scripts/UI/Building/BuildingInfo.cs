using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    public Inventory inventory = null;

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

    public void BuildingClick()
    {
        if (selectBuildingData != null)
        {
            preBuilding.SetActive(true);
            PreBuilding.instance.SetImage(selectBuilding.gameObj, selectBuilding.level, selectBuilding.height, selectBuilding.width, selectBuilding.dirCount);
            PreBuilding.instance.isEnough = AmountsEnoughCheck();
        }
    }

    public void ClearArr()
    {
        for (int i = 0; i < buildingNeedList.Length; i++) 
        {
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

        for (int i = 0; i < buildingDatas.GetItemCount(); i++)
        {
            int value;
            bool hasItem = inventory.totalItems.TryGetValue(ItemList.instance.itemDic[buildingDatas.items[i]], out value);
            isEnough = hasItem && value >= buildingDatas.amounts[i];

            if (isEnough && totalAmountsEnough)
                totalAmountsEnough = true;
            else
                totalAmountsEnough = false;

            buildingNeedList[i].gameObject.SetActive(true);
            buildingNeedList[i].AddItem(ItemList.instance.itemDic[buildingDatas.items[i]], buildingDatas.amounts[i], isEnough);
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
                inventory.Sub(buildingNeedList[i].item, buildingNeedList[i].amount);
        }

        SetItemSlot();
    }

    public bool AmountsEnoughCheck()
    {
        SetItemSlot();
        return totalAmountsEnough;
    }
}
