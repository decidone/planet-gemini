using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourcesManager : MonoBehaviour
{
    #region Singleton
    public static ResourcesManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of ResourcesManager found!");
            return;
        }

        instance = this;
    }
    #endregion

    public Building tempMiner = null;
    public TempMinerUi tempMinerUI;
    public GameObject fogOfWar;
    
}
