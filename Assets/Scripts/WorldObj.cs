using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WorldObj : NetworkBehaviour
{
    #region AwakeSet
    protected Dictionary<Type, Component> _cache = new();

    protected virtual void Awake()
    {
        foreach (var comp in GetComponents<Component>())
        {
            var type = comp.GetType();

            // 자기 자신부터 Component까지 올라가면서 전부 등록
            while (type != null && type != typeof(MonoBehaviour)
                                && type != typeof(Behaviour)
                                && type != typeof(Component))
            {
                if (!_cache.ContainsKey(type))
                    _cache[type] = comp;

                type = type.BaseType;
            }
        }
    }

    public bool Has<T>() where T : Component => _cache.ContainsKey(typeof(T));

    public T Get<T>() where T : Component => _cache.TryGetValue(typeof(T), out var c) ? c as T : null;

    public bool TryGet<T>(out T result) where T : Component
    {
        if (_cache.TryGetValue(typeof(T), out var c))
        {
            result = c as T;
            return true;
        }
        result = null;
        return false;
    }
    #endregion
}
