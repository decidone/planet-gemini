using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// 네트워크 개체에 대한 개체 풀은 Netcode에서 개체를 생성하는 방법을 제어하는 ​​데 사용됩니다. 기본적으로 넷코드는 새 개체를 생성할 때 새 메모리를 할당합니다.
/// 이 네트워크 풀에서는 사용자 지정 생성을 사용하여 개체를 재사용합니다.
/// NetworkManager의 프리팹 핸들러에 연결하여 객체 생성을 가로채고 사용자 정의 작업을 수행합니다.
/// </summary>
public class NetworkObjectPool : NetworkBehaviour
{
    private static NetworkObjectPool _instance;

    public static NetworkObjectPool Singleton { get { return _instance; } }

    [SerializeField]
    List<PoolConfigObject> PooledPrefabsList;

    HashSet<GameObject> prefabs = new HashSet<GameObject>();

    Dictionary<GameObject, Queue<NetworkObject>> pooledObjects = new Dictionary<GameObject, Queue<NetworkObject>>();
    private bool m_HasInitialized = false;

    public void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    public override void OnNetworkSpawn()
    {
        InitializePool();
    }

    public override void OnNetworkDespawn()
    {
        ClearPool();
    }

    public void OnValidate()
    {
        for (var i = 0; i < PooledPrefabsList.Count; i++)
        {
            var prefab = PooledPrefabsList[i].Prefab;
            if (prefab != null)
            {
                Assert.IsNotNull(prefab.GetComponent<NetworkObject>(), $"{nameof(NetworkObjectPool)}: Pooled prefab \"{prefab.name}\" at index {i.ToString()} has no {nameof(NetworkObject)} component.");
            }
        }
    }

    /// <summary>
    /// 풀에서 주어진 프리팹의 인스턴스를 가져옵니다. 프리팹을 풀에 등록해야 합니다.
    /// /// </summary>
    /// <param name="prefab"></param>
    /// <returns></returns>
    public NetworkObject GetNetworkObject(GameObject prefab)
    {
        return GetNetworkObjectInternal(prefab, Vector3.zero, Quaternion.identity);
    }

    /// <summary>
    /// 풀에서 지정된 프리팹의 인스턴스를 가져옵니다. 프리팹을 풀에 등록해야 합니다.
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="position">The position to spawn the object at.</param>
    /// <param name="rotation">The rotation to spawn the object with.</param>
    /// <returns></returns>
    public NetworkObject GetNetworkObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        return GetNetworkObjectInternal(prefab, position, rotation);
    }

    /// <summary>
    /// 객체를 풀에 반환합니다(반환하기 전에 객체를 재설정합니다).
    /// </summary>
    public void ReturnNetworkObject(NetworkObject networkObject, GameObject prefab)
    {
        var go = networkObject.gameObject;
        go.SetActive(false);
        pooledObjects[prefab].Enqueue(networkObject);
    }

    /// <summary>
    /// 생성 가능한 프리팹 목록에 프리팹을 추가합니다.
    /// </summary>
    /// <param name="prefab">The prefab to add.</param>
    /// <param name="prewarmCount"></param>
    public void AddPrefab(GameObject prefab, int prewarmCount = 0)
    {
        var networkObject = prefab.GetComponent<NetworkObject>();

        Assert.IsNotNull(networkObject, $"{nameof(prefab)} must have {nameof(networkObject)} component.");
        Assert.IsFalse(prefabs.Contains(prefab), $"Prefab {prefab.name} is already registered in the pool.");

        RegisterPrefabInternal(prefab, prewarmCount);
    }

    /// <summary>
    /// 프리팹에 대한 캐시를 구축합니다.
    /// </summary>
    private void RegisterPrefabInternal(GameObject prefab, int prewarmCount)
    {
        prefabs.Add(prefab);

        var prefabQueue = new Queue<NetworkObject>();
        pooledObjects[prefab] = prefabQueue;
        for (int i = 0; i < prewarmCount; i++)
        {
            var go = CreateInstance(prefab);
            ReturnNetworkObject(go.GetComponent<NetworkObject>(), prefab);
        }

        // Register Netcode Spawn handlers
        NetworkManager.Singleton.PrefabHandler.AddHandler(prefab, new PooledPrefabInstanceHandler(prefab, this));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private GameObject CreateInstance(GameObject prefab)
    {
        return Instantiate(prefab);
    }

    /// <summary>
    /// This matches the signature of <see cref="NetworkSpawnManager.SpawnHandlerDelegate"/>
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <returns></returns>
    private NetworkObject GetNetworkObjectInternal(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        var queue = pooledObjects[prefab];
        NetworkObject networkObject;
        if (queue.Count > 0)
        {
            networkObject = queue.Dequeue();
        }
        else
        {
            networkObject = CreateInstance(prefab).GetComponent<NetworkObject>();
        }

        // Here we must reverse the logic in ReturnNetworkObject.
        var go = networkObject.gameObject;
        go.SetActive(true);

        go.transform.position = position;
        go.transform.rotation = rotation;

        return networkObject;
    }

    /// <summary>
    /// Registers all objects in <see cref="PooledPrefabsList"/> to the cache.
    /// </summary>
    public void InitializePool()
    {
        if (m_HasInitialized) return;
        foreach (var configObject in PooledPrefabsList)
        {
            RegisterPrefabInternal(configObject.Prefab, configObject.PrewarmCount);
        }
        m_HasInitialized = true;
    }

    /// <summary>
    /// Unregisters all objects in <see cref="PooledPrefabsList"/> from the cache.
    /// </summary>
    public void ClearPool()
    {
        foreach (var prefab in prefabs)
        {
            // Unregister Netcode Spawn handlers
            NetworkManager.Singleton.PrefabHandler.RemoveHandler(prefab);
        }
        pooledObjects.Clear();
    }

    public GameObject PoolObjFind(string name)
    {
        foreach (var poolConfigObject in PooledPrefabsList)
        {
            if(poolConfigObject.Prefab.name == name)
                return poolConfigObject.Prefab;
        }
        return null;
    }

    public Queue<NetworkObject> FindAllObj(GameObject prefab)
    {
        Queue<NetworkObject> queue = null;
        queue = pooledObjects[prefab];
        return queue;
    }
}

[Serializable]
struct PoolConfigObject
{
    public GameObject Prefab;
    public int PrewarmCount;
}

class PooledPrefabInstanceHandler : INetworkPrefabInstanceHandler
{
    GameObject m_Prefab;
    NetworkObjectPool m_Pool;

    public PooledPrefabInstanceHandler(GameObject prefab, NetworkObjectPool pool)
    {
        m_Prefab = prefab;
        m_Pool = pool;
    }

    NetworkObject INetworkPrefabInstanceHandler.Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
    {
        var netObject = m_Pool.GetNetworkObject(m_Prefab, position, rotation);
        return netObject;
    }

    void INetworkPrefabInstanceHandler.Destroy(NetworkObject networkObject)
    {
        m_Pool.ReturnNetworkObject(networkObject, m_Prefab);
    }
}