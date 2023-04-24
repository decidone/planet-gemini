using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingInven : MonoBehaviour
{
    public delegate void OnItemChanged();
    public OnItemChanged onItemChangedCallback;

    //List<Item> itemList;
    public List<Building> itemList;

    [SerializeField]
    int space;   // ������ ���� ����, �巡�׿� ���� ��ȣ�� �� ��

    //public Dictionary<int, Item> items = new Dictionary<int, Item>();
    public Dictionary<int, Building> items = new Dictionary<int, Building>();
    public Dictionary<int, int> amounts = new Dictionary<int, int>();

    // Start is called before the first frame update
    void Start()
    {
        itemList = BuildingList.instance.itemList;
        BuildSet();
    }

    void BuildSet()
    {
        for (int i = 0; i < itemList.Count; i++)
        {
            if (!items.ContainsKey(i))
            {
                items[i] = itemList[i];
                amounts[i] = 0;
            }
        }

        onItemChangedCallback?.Invoke();
    }
    public void Refresh()
    {
        onItemChangedCallback?.Invoke();
    }

}
