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
            preBuilding.GetComponent<PreBuilding>().SetImage(selectBuilding.item.icon, selectBuilding.gameObj);
            //preBuilding.GetComponent<PreBuilding>().SetImage(selectBuilding.icon);
        }
    }

    void CrearList()
    {
        for (int i = 0; i < buildingNeedList.Length; i++) 
        {
            buildingNeedList[i].gameObject.SetActive(false);
        }

        //buildingNeedList.Clear();
    }
    //public void CreateItemSlot(BuildingData buildingDatas, Item select)
    public void CreateItemSlot(BuildingData buildingDatas, Building select)
    {
        CrearList();
        totalAmountsEnough = false;

        selectBuilding = select;

        selectBuildingData = buildingDatas;

        bool isEnough;

        for (int i = 0; i < buildingDatas.GetItemCount(); i++)
        {
            int value;
            bool hasItem = inventory.totalItems.TryGetValue(ItemList.instance.itemDic[buildingDatas.items[i]], out value);
            isEnough = hasItem && value >= buildingDatas.amounts[i];

            if (isEnough)
                totalAmountsEnough = true;
            else
                totalAmountsEnough = false;

            buildingNeedList[i].gameObject.SetActive(true);
            buildingNeedList[i].AddItem(ItemList.instance.itemDic[buildingDatas.items[i]], buildingDatas.amounts[i], isEnough);
        }
    }

    public void CreateItemSlot()
    {
        if (selectBuildingData != null && selectBuilding.item != null)
            CreateItemSlot(selectBuildingData, selectBuilding);
    }
}
