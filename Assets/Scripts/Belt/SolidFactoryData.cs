using UnityEngine;

[CreateAssetMenu(fileName = "SolidFactory Data", menuName = "Scriptable Object/SolidFactory Data", order = int.MaxValue)]
public class SolidFactoryData : ScriptableObject
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
