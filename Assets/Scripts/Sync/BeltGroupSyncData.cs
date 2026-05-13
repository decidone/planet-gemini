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
    public int[] beltItemIndexes;
    public int[] beltItemBeltIndexes;       // 어느 벨트 소속인지
    public int[] beltItemBeltGroupIndexes;  // beltGroupIndex
    public Vector3[] beltItemPositions;
    public double[] beltItemEnterTimes;
    public Vector3[] beltItemStartPositions;
    public Vector3[] beltItemEndPositions;
    public double[] beltItemTravelDurations;

    public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
    {
        s.SerializeValue(ref beltRefs);
        s.SerializeValue(ref nextObjRef);
        s.SerializeValue(ref hasNextObj);
        s.SerializeValue(ref preObjRef);
        s.SerializeValue(ref hasPreObj);

        s.SerializeValue(ref beltItemIndexes);
        s.SerializeValue(ref beltItemBeltIndexes);
        s.SerializeValue(ref beltItemBeltGroupIndexes);
        s.SerializeValue(ref beltItemPositions);
        s.SerializeValue(ref beltItemEnterTimes);
        s.SerializeValue(ref beltItemStartPositions);
        s.SerializeValue(ref beltItemEndPositions);
        s.SerializeValue(ref beltItemTravelDurations);
    }
}