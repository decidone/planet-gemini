using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : Inventory
{
    #region Singleton
    public static Inventory instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of inventory found!");
            return;
        }

        instance = this;
    }
    #endregion
}
