using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// UTF-8 설정
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
    GameManager gameManager;
    InputManager inputManager;

    SoundManager soundManager;
    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void Start()
    {
        gameManager = GameManager.instance;
        scienceDb = TempScienceDb.instance;
        soundManager = SoundManager.instance;
        buildingDataList = BuildingList.instance.buildingListSO.buildingSOList;
        buildingTagsBtn = buildingTagsPanel.GetComponentsInChildren<Button>();

        for (int i = 0; i < buildingTagsBtn.Length; i++)
        {
            int buttonIndex = i;
            buildingTagsBtn[i].onClick.AddListener(() => ButtonClicked(buttonIndex));
        }
        ButtonClicked(0);
    }
    void OnEnable()
    {
        inputManager = InputManager.instance;
        inputManager.controls.HotKey.Debug.performed += DebugMode;
        inputManager.controls.Building.BuildingInven.performed += OnBuildingInvenPerformed;
    }
    void OnDisable()
    {
        inputManager.controls.HotKey.Debug.performed -= DebugMode;
        inputManager.controls.Building.BuildingInven.performed -= OnBuildingInvenPerformed;
    }
    void DebugMode(InputAction.CallbackContext ctx)
    {
        Refresh();
    }

    private void ButtonClicked(int buttonIndex)
    {
        string itemType = GetItemType(buttonIndex);
        AddDicType(itemType);
        preBtnIndex = buttonIndex;
    }

    private void OnBuildingInvenPerformed(InputAction.CallbackContext context)
    {
        // 눌린 키 확인
        var key = context.control.displayName;

        switch (key)
        {
            case "1":
                ButtonClicked(0);
                break;
            case "2":
                ButtonClicked(1);
                break;
            case "3":
                ButtonClicked(2);
                break;
            case "4":
                ButtonClicked(3);
                break;
            case "5":
                ButtonClicked(4);
                break;
            default:
                break;
        }
    }

    private string GetItemType(int buttonIndex)
    {
        switch (buttonIndex)
        {
            case 0:
                return "Factory";
            case 1:
                return "Logistics";
            case 2:
                return "Fluid";
            case 3:
                return "Energy";
            case 4:
                return "Battle";
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
            if (!gameManager.debug)
            {
                for (int a = 0; a < scienceDb.scienceNameDb.Count; a++)
                {
                    if (scienceDb.scienceNameDb.ContainsKey(buildingDataList[i].scienceName) && buildingDataList[i].type == itemType)
                    {
                        Dictionary<int, int> values;
                        if (scienceDb.scienceNameDb.TryGetValue(buildingDataList[i].scienceName, out values) && values.ContainsKey(buildingDataList[i].level))
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

        soundManager.PlayUISFX("SidebarClick");
        onItemChangedCallback?.Invoke();
    }

    public void Refresh()
    {
        string itemType = GetItemType(preBtnIndex);
        AddDicType(itemType);
        onItemChangedCallback?.Invoke();
    }
}
