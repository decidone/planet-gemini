using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemsList : MonoBehaviour
{
    // ��ũ��Ʈ���� ������ ��� �� �κ��丮 ������ ���Ŀ� ���
    public List<Item> itemsList = new List<Item>();

    #region Singleton
    public static ItemsList instance;

    private void Awake()
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
