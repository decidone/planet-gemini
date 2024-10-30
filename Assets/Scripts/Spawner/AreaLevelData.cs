using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
[CreateAssetMenu(fileName = "Area Level Data", menuName = "Data/Area Level Data")]

public class AreaLevelData : ScriptableObject
{
    public int sppawnerLevel;
    public int maxWeakSpawn;
    public int maxNormalSpawn;
    public int maxStrongSpawn;
    public int maxGuardianSpawn;
}
