using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemsList : MonoBehaviour
{
    // 스크립트에서 아이템 사용 및 인벤토리 아이템 정렬에 사용
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
