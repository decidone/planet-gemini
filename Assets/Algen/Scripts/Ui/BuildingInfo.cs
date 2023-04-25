using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingInfo : MonoBehaviour
{
    public GameObject itemImg = null;
    public GameObject buildingInfoPanel = null;
    //List<GameObject> buildingNeedList = new List<GameObject>();
    [SerializeField]
    BuildingImgCtrl[] buildingNeedList;

    Building selectBuilding = null;
    BuildingData selectBuildingData = null;
    public GameObject preBuilding;

    //protected PreBuilding preBuilding;

    public Button buildingBtn;
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

    void Start()
    {
        //buildingNeedList = buildingInfoPanel.GetComponentsInChildren<BuildingImgCtrl>();

        if (buildingBtn != null)
            buildingBtn.onClick.AddListener(BuildingClick);
    }

    void BuildingClick()
    {
        if (totalAmountsEnough)
        {
            preBuilding.SetActive(true);
            PreBuilding.instance.SetImage(selectBuilding.item.icon, selectBuilding.gameObj);
            //preBuilding.GetComponent<PreBuilding>().SetImage(selectBuilding.item.icon, selectBuilding.gameObj);          
            //preBuilding.GetComponent<PreBuilding>().SetImage(selectBuilding.icon);
        }
    }

    void ClearArr()
    {
        for (int i = 0; i < buildingNeedList.Length; i++) 
        {
            buildingNeedList[i].gameObject.SetActive(false);
        }

        //buildingNeedList.Clear();
    }

    //public void CreateItemSlot(BuildingData buildingDatas, Item select)
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

    public void BuildingEnd()
    {
        for (int i = 0; i < buildingNeedList.Length; i++)
        {
            if(buildingNeedList[i].item != null)
                inventory.Sub(buildingNeedList[i].item, buildingNeedList[i].amount);
        }

        SetItemSlot();

        if (!totalAmountsEnough)
        {
            PreBuilding.instance.ReSetImage();
            preBuilding.SetActive(false);
        }
    }
}
