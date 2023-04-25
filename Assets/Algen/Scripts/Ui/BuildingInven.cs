using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingInven : MonoBehaviour
{
    public delegate void OnItemChanged();
    public OnItemChanged onItemChangedCallback;

    public List<Building> itemList;

    [SerializeField]
    int space;   // 아이템 슬롯 상한, 드래그용 슬롯 번호를 겸 함

    public Dictionary<int, Building> towerList = new Dictionary<int, Building>();

    // Start is called before the first frame update
    void Start()
    {
        itemList = BuildingList.instance.itemList;
        BuildSet();
    }

    void BuildSet()
    {// 종류 및 과학 트리에 따른 리스트 받아오기 추가 해야함
        int index = 0;
        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemList[i].type == "Tower")
            {
                if (!towerList.ContainsKey(index))
                {
                    towerList[index] = itemList[i];
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

}
