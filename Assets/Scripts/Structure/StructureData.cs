using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Structure Data", menuName = "Scriptable Object/Structure Data", order = int.MaxValue)]
public class StructureData : ScriptableObject
{
    [SerializeField]
    private string factoryName;
    public string FactoryName { get { return factoryName; } }

    [SerializeField]
    private int[] maxHp;
    public int[] MaxHp { get { return maxHp; } }

    [SerializeField]
    private float maxItemStorageLimit;
    public float MaxItemStorageLimit { get { return maxItemStorageLimit; } }

    [SerializeField]
    private float maxFulidStorageLimit;
    public float MaxFulidStorageLimit { get { return maxFulidStorageLimit; } }

    [SerializeField]
    private float[] sendSpeed; // only Item
    public float[] SendSpeed { get { return sendSpeed; } }

    [SerializeField]
    private float sendFluidAmount; // only Fluid
    public float SendFluidAmount { get { return sendFluidAmount; } }

    [SerializeField]
    private float sendDelay;
    public float SendDelay { get { return sendDelay; } }

    [SerializeField]
    private float maxBuildingGauge;
    public float MaxBuildingGauge { get { return maxBuildingGauge; } }

    [SerializeField]
    private float maxRepairGauge;
    public float MaxRepairGauge { get { return maxRepairGauge; } }

    [SerializeField]
    private float colliderRadius;//Å¸°Ù Å½»ö ¹üÀ§
    public float ColliderRadius { get { return colliderRadius; } }
}
