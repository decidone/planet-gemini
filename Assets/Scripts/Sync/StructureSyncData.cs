using Unity.Netcode;
using UnityEngine;

public struct StructureSyncData : INetworkSerializable
{
    // ===== From: Structure.ClientConnectSyncClientRpc =====
    public int level;
    public int dirNum;
    public int height;
    public int width;
    public bool isInHostMap;
    public float hp;

    // ===== From: Structure.NearAndInOutObjSyncClientRpc =====
    public NetworkObjectReference[] nearObjRefs;
    public bool[] nearObjValids;
    public NetworkObjectReference[] outObjRefs;
    public NetworkObjectReference[] inObjRefs;

    // ===== From: Structure.ClientMapDataSetClientRpc =====
    public Vector3 position;

    public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
    {
        s.SerializeValue(ref level);
        s.SerializeValue(ref dirNum);
        s.SerializeValue(ref height);
        s.SerializeValue(ref width);
        s.SerializeValue(ref isInHostMap);
        s.SerializeValue(ref hp);

        s.SerializeValue(ref nearObjRefs);
        s.SerializeValue(ref nearObjValids);
        s.SerializeValue(ref outObjRefs);
        s.SerializeValue(ref inObjRefs);

        s.SerializeValue(ref position);
    }
}