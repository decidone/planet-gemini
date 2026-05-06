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
    public float buildHp;

    public NetworkObjectReference[] nearObjRefs;
    public bool[] nearObjValids;
    public NetworkObjectReference[] outObjRefs;
    public NetworkObjectReference[] inObjRefs;

    public Vector3 position;

    public bool isPreBuilding;
    public bool destroyStart;
    public float repairGauge;
    public float destroyTimer;

    //itemList
    public int[] itemIndexes;

    //inven
    public int[] inventorySlotNums;
    public int[] inventoryItemIndexes;
    public int[] inventoryItemAmounts;

    public int recipeIndex;  // -1이면 미사용

    public float stored;  // EnergyBattery

    public Vector3[] connectedLinePositions;    // LDConnector

    public float saveFluidNum;
    public string fluidName;

    // Buyer
    public int maxBuyAmount;
    public int buyInterval;

    // Transporter
    public Vector3 takeBuildPos;
    public bool hasTakeBuild;
    public bool isToggleOn;
    public int sendAmount;

    // UnitFactory
    public Vector2 movePos;
    public bool isSetPos;

    public int energyBulletAmount;

    // SendUnderBelt
    public int[] sendingItemIndexes;
    public float[] sendingItemTimes;

    // BeltCtrl
    public bool isUp;
    public bool isRight;
    public bool isDown;
    public bool isLeft;
    public int modelMotion;
    public bool isTurn;
    public bool isRightTurn;
    public int beltStateInt;

    // Disintegrator
    public bool isAuto;

    public int unloaderSelectItemIndex;

    public int fuel;

    public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
    {
        s.SerializeValue(ref level);
        s.SerializeValue(ref dirNum);
        s.SerializeValue(ref height);
        s.SerializeValue(ref width);
        s.SerializeValue(ref isInHostMap);
        s.SerializeValue(ref hp);
        s.SerializeValue(ref buildHp);

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

        s.SerializeValue(ref stored);

        s.SerializeValue(ref connectedLinePositions);

        s.SerializeValue(ref saveFluidNum);
        s.SerializeValue(ref fluidName);

        s.SerializeValue(ref maxBuyAmount);
        s.SerializeValue(ref buyInterval);

        s.SerializeValue(ref takeBuildPos);
        s.SerializeValue(ref hasTakeBuild);
        s.SerializeValue(ref isToggleOn);
        s.SerializeValue(ref sendAmount);

        s.SerializeValue(ref movePos);
        s.SerializeValue(ref isSetPos);

        s.SerializeValue(ref energyBulletAmount);
        s.SerializeValue(ref sendingItemIndexes);
        s.SerializeValue(ref sendingItemTimes);

        s.SerializeValue(ref isUp);
        s.SerializeValue(ref isRight);
        s.SerializeValue(ref isDown);
        s.SerializeValue(ref isLeft);
        s.SerializeValue(ref modelMotion);
        s.SerializeValue(ref isTurn);
        s.SerializeValue(ref isRightTurn);
        s.SerializeValue(ref beltStateInt);

        s.SerializeValue(ref isAuto);

        s.SerializeValue(ref unloaderSelectItemIndex);

        s.SerializeValue(ref fuel);
    }
}