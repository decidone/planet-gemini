using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
[CreateAssetMenu(fileName = "Structure Data", menuName = "Data/Structure Data", order = int.MaxValue)]
public class StructureData : ScriptableObject
{
    [SerializeField]
    private string factoryName;
    public string FactoryName { get { return factoryName; } }

    [SerializeField]
    private int[] maxHp;
    public int[] MaxHp { get { return maxHp; } }

    [SerializeField]
    private int maxItemStorageLimit;
    public int MaxItemStorageLimit { get { return maxItemStorageLimit; } }

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
    private float colliderRadius;//타겟 탐색 범위
    public float ColliderRadius { get { return colliderRadius; } }

    [SerializeField]
    private int maxLevel;//타겟 탐색 범위
    public int MaxLevel { get { return maxLevel; } }

    [SerializeField]
    private float cooldown;
    public float Cooldown { get { return cooldown; } }

    [SerializeField]
    private bool isEnergyStr;
    public bool IsEnergyStr { get { return isEnergyStr; } }

    [SerializeField]
    private float production;
    public float Production { get { return production; } }

    [SerializeField]
    private bool[] energyUse;
    public bool[] EnergyUse { get { return energyUse; } }

    [SerializeField]
    private float[] consumption;
    public float[] Consumption { get { return consumption; } }
}