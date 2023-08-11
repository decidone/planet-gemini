using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingInven : MonoBehaviour
{
    public delegate void OnItemChanged();
    public OnItemChanged onItemChangedCallback;

    private List<Building> buildingDataList;
    public Dictionary<int, Building> buildingDic = new Dictionary<int, Building>();
    private TempScienceDb scienceDb;
    private Button[] buildingTagsBtn;
    private int preBtnIndex = 0;
    public static BuildingInven instance;
    [SerializeField]
    private GameObject buildingTagsPanel;
    public bool debugMode = false;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of building inven found!");
            return;
        }

        instance = this;
    }

    private void Start()
    {
        scienceDb = TempScienceDb.instance;
        buildingDataList = BuildingList.instance.buildingDataList;
        buildingTagsBtn = buildingTagsPanel.GetComponentsInChildren<Button>();

        for (int i = 0; i < buildingTagsBtn.Length; i++)
        {
            int buttonIndex = i;
            buildingTagsBtn[i].onClick.AddListener(() => ButtonClicked(buttonIndex));
        }

        ButtonClicked(0);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F3))
        {
            debugMode = !debugMode;
            Refresh();
        }
    }

    private void ButtonClicked(int buttonIndex)
    {
        string itemType = GetItemType(buttonIndex);
        AddDicType(itemType);
        preBtnIndex = buttonIndex;
    }

    private string GetItemType(int buttonIndex)
    {
        switch (buttonIndex)
        {
            case 0:
                return "Factory";
            case 1:
                return "Transport";
            case 2:
                return "Energy";
            case 3:
                return "Tower";
            case 4:
                return "Wall";
            case 5:
                return "Etc";
            default:
                return "";
        }
    }

    private void AddDicType(string itemType)
    {
        buildingDic.Clear();
        int index = 0;
        for (int i = 0; i < buildingDataList.Count; i++)
        {
            if (!debugMode)
            {
                for (int a = 0; a < scienceDb.scienceNameDb.Count; a++)
                {
                    if (scienceDb.scienceNameDb.ContainsKey(buildingDataList[i].scienceName) && buildingDataList[i].type == itemType)
                    {
                        List<int> values;
                        if (scienceDb.scienceNameDb.TryGetValue(buildingDataList[i].scienceName, out values) && values.Contains(buildingDataList[i].level))
                        {
                            buildingDic[index] = buildingDataList[i];
                            index++;
                            break;
                        }
                    }
                }
            }
            else
            {
                if (buildingDataList[i].type == itemType)
                {
                    buildingDic[index] = buildingDataList[i];
                    index++;
                }                
            }
        }

        onItemChangedCallback?.Invoke();
    }

    public void Refresh()
    {
        string itemType = GetItemType(preBtnIndex);
        AddDicType(itemType);
        onItemChangedCallback?.Invoke();
    }
}
