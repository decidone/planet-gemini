using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingInven : MonoBehaviour
{
    public delegate void OnItemChanged();
    public OnItemChanged onItemChangedCallback;

    public List<Building> buildingDataList;

    public Dictionary<int, Building> BuildingDic = new Dictionary<int, Building>();

    public GameObject BuildingTagsPanel = null;
    Button[] BuildingTagsBtn;

    // Start is called before the first frame update
    void Start()
    {
        buildingDataList = BuildingList.instance.buildingDataList;
        BuildingTagsBtn = BuildingTagsPanel.GetComponentsInChildren<Button>();

        for (int i = 0; i < BuildingTagsBtn.Length; i++)
        {
            int buttonIndex = i;
            BuildingTagsBtn[i].onClick.AddListener(() => ButtonClicked(buttonIndex));

        }
        ButtonClicked(0);
    }

    private void ButtonClicked(int buttonIndex)
    {
        if (buttonIndex == 0)
        {
            AddDicType("Factory");
        }
        else if (buttonIndex == 1)
        {
            AddDicType("Transport");
        }
        else if (buttonIndex == 2)
        {
            AddDicType("Energy");
        }
        else if (buttonIndex == 3)
        {
            AddDicType("Tower");
        }
        else if (buttonIndex == 4)
        {
            AddDicType("Wall");
        }
        else if (buttonIndex == 5)
        {
            AddDicType("Etc");
        }
    }

    private void AddDicType(string itemType)
    {
        ResetDic();

        int index = 0;
        for (int i = 0; i < buildingDataList.Count; i++)
        {// 과학 등급이 저장된 파일과 연동해야됨
            // 이후 수정하느걸로
            if (buildingDataList[i].type == itemType)
            {
                if (!BuildingDic.ContainsKey(index))
                {
                    BuildingDic[index] = buildingDataList[i];
                    index++;
                }
            }
        }
        onItemChangedCallback?.Invoke();
    }

    public void Refresh()
    {
        onItemChangedCallback?.Invoke();
    }

    void ResetDic()
    {
        BuildingDic.Clear();
        onItemChangedCallback?.Invoke();
    }
}
