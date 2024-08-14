using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
public class InventoryList : MonoBehaviour
{
    public GameObject[] InventoryArr = null;
    public GameObject[] StructureStorageArr = null;
    public GameObject[] LogisticsArr = null;

    #region Singleton
    public static InventoryList instance;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }
    #endregion

}
