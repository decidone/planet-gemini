using Unity.Netcode;

public enum FluidType : sbyte
{
    None = -1,
    Water = 0,
    CrudeOil = 1
}

public struct FluidSyncData : INetworkSerializable
{
    public NetworkObjectReference fluidRef;
    public float saveFluidNum;
    public sbyte fluidTypeIndex;

    public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
    {
        s.SerializeValue(ref fluidRef);
        s.SerializeValue(ref saveFluidNum);
        s.SerializeValue(ref fluidTypeIndex);
    }
}

public static class FluidTypeHelper
{
    public static sbyte StringToIndex(string fluidName)
    {
        if (string.IsNullOrEmpty(fluidName)) return -1;
        if (fluidName == "Water") return 0;
        if (fluidName == "CrudeOil") return 1;
        return -1;
    }

    public static string IndexToString(sbyte index)
    {
        switch (index)
        {
            case 0: return "Water";
            case 1: return "CrudeOil";
            default: return "";
        }
    }
}