using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourcesManager : MonoBehaviour
{
    public GameObject fogOfWar;

    public Material outlintMat;
    public Material noOutlineMat;

    #region Singleton
    public static ResourcesManager instance;

    private void Awake()
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
