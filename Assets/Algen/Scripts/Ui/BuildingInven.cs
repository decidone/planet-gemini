using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingInven : MonoBehaviour
{
    public delegate void OnItemChanged();
    public OnItemChanged onItemChangedCallback;

    public List<Building> itemList;

    [SerializeField]
    int space;   // ������ ���� ����, �巡�׿� ���� ��ȣ�� �� ��

    public Dictionary<int, Building> towerList = new Dictionary<int, Building>();

    // Start is called before the first frame update
    void Start()
    {
        itemList = BuildingList.instance.itemList;
        BuildSet();
    }

    void BuildSet()
    {// ���� �� ���� Ʈ���� ���� ����Ʈ �޾ƿ��� �߰� �ؾ���
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
