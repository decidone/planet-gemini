using Unity.Netcode;
using UnityEngine;

public struct FilterSyncData : INetworkSerializable
{
    public bool isFilterOn;
    public bool isReverseFilterOn;
    public int itemIndex;
    public bool hasOutObj;
    public NetworkObjectReference networkObjectReference;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref isFilterOn);
        serializer.SerializeValue(ref isReverseFilterOn);
        serializer.SerializeValue(ref itemIndex);
        serializer.SerializeValue(ref hasOutObj);
        if (hasOutObj)
            serializer.SerializeValue(ref networkObjectReference);
    }
}
