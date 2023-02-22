using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemsList : MonoBehaviour
{
    // ��ũ��Ʈ���� ������ ��� �� �κ��丮 ������ ���Ŀ� ���
    // ������ ����� ���� <string, Item>���� Document ���� �ʿ䰡 ����
    public List<Item> itemsList = new List<Item>();

    #region Singleton
    public static ItemsList instance;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of itemsList found!");
            return;
        }

        instance = this;
    }
    #endregion
}
