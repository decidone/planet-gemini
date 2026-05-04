using Unity.Netcode;
using UnityEngine;

public struct BeltGroupSyncData : INetworkSerializable
{
    // belt 순서 정보
    public NetworkObjectReference[] beltRefs;

    // 인접 객체 정보
    public NetworkObjectReference nextObjRef;
    public bool hasNextObj;
    public NetworkObjectReference preObjRef;
    public bool hasPreObj;

    // belt 위 아이템 정보
    public int[] itemIndexes;
    public Vector2[] itemPositions;
    public int[] itemBeltGroupIndexes;
    public int[] itemBeltIndexes;

    public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
    {
        s.SerializeValue(ref beltRefs);
        s.SerializeValue(ref nextObjRef);
        s.SerializeValue(ref hasNextObj);
        s.SerializeValue(ref preObjRef);
        s.SerializeValue(ref hasPreObj);
        s.SerializeValue(ref itemIndexes);
        s.SerializeValue(ref itemPositions);
        s.SerializeValue(ref itemBeltGroupIndexes);
        s.SerializeValue(ref itemBeltIndexes);
    }
}