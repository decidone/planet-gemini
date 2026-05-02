using Unity.Netcode;
using UnityEngine;

public struct StructureSyncData : INetworkSerializable
{
    // ===== 기존 필드들 =====
    public int level;
    public int dirNum;
    public int height;
    public int width;
    public bool isInHostMap;
    public float hp;

    public NetworkObjectReference[] nearObjRefs;
    public bool[] nearObjValids;
    public NetworkObjectReference[] outObjRefs;
    public NetworkObjectReference[] inObjRefs;

    public Vector3 position;

    public bool isPreBuilding;
    public bool destroyStart;
    public float repairGauge;
    public float destroyTimer;

    // ===== 추가: ItemSync (Structure.itemList) =====
    public int[] itemIndexes;

    // ===== 추가: ItemSync (Production.inventory) =====
    public int[] inventorySlotNums;
    public int[] inventoryItemIndexes;
    public int[] inventoryItemAmounts;

    // ===== 추가: Recipe =====
    public int recipeIndex;  // -1이면 미사용

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

        s.SerializeValue(ref isPreBuilding);
        s.SerializeValue(ref destroyStart);
        s.SerializeValue(ref repairGauge);
        s.SerializeValue(ref destroyTimer);

        s.SerializeValue(ref itemIndexes);

        s.SerializeValue(ref inventorySlotNums);
        s.SerializeValue(ref inventoryItemIndexes);
        s.SerializeValue(ref inventoryItemAmounts);

        s.SerializeValue(ref recipeIndex);
    }
}