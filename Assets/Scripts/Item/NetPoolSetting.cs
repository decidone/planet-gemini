using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetPoolSetting : NetworkBehaviour
{
    NetworkObjectPool networkPool;
    public List<GameObject> findPoolObj = new List<GameObject>();

    #region Singleton
    public static NetPoolSetting instance;

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

    void Start()
    {
        networkPool = NetworkObjectPool.Singleton;
        TestGetPool();
    }

    void TestGetPool()
    {
        findPoolObj.Add(networkPool.PoolObjFind("Item"));
        findPoolObj.Add(networkPool.PoolObjFind("Bullet"));

    }
}
