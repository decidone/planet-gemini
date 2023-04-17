using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FluidFactory Data", menuName = "Scriptable Object/FluidFactory Data", order = int.MaxValue)]
public class FluidFactoryData : ScriptableObject
{
    [SerializeField]
    private string factoryName;
    public string FactoryName { get { return factoryName; } }

    [SerializeField]
    private float fullFluidNum;
    public float FullFluidNum { get { return fullFluidNum; } }

    [SerializeField]
    private float sendFluid;
    public float SendFluid { get { return sendFluid; } }

    [SerializeField]
    private float sendDelay;
    public float SendDelay { get { return sendDelay; } }
}
