using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Production Data", menuName = "Scriptable Object/Production Data", order = int.MaxValue)]

public class ProductionData : ScriptableObject
{
    [SerializeField]
    private string factoryName;
    public string FactoryName { get { return factoryName; } }

    [SerializeField]
    private int[] maxHp;//Ã¼·Â
    public int[] MaxHp { get { return maxHp; } }

    [SerializeField]
    private int fullItemNum;
    public int FullItemNum { get { return fullItemNum; } }

    [SerializeField]
    private float[] sendSpeed;
    public float[] SendSpeed { get { return sendSpeed; } }

    [SerializeField]
    private float sendDelay;
    public float SendDelay { get { return sendDelay; } }

    [SerializeField]
    private float maxBuildingGauge;
    public float MaxBuildingGauge { get { return maxBuildingGauge; } }

    [SerializeField]
    private float maxRepairGauge;
    public float MaxRepairGauge { get { return maxRepairGauge; } }
}
