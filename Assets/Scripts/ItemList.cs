using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemList : MonoBehaviour
{
    public List<Item> itemsList = new List<Item>();
    public Dictionary<int, Item> items = new Dictionary<int, Item>();
    public Dictionary<int, int> amounts = new Dictionary<int, int>();

    void Start()
    {
        // ���⼭ ����Ʈ ������� Dictionary �׸� ����
    }
}
