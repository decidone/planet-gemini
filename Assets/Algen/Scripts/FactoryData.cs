using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Factory Data", menuName = "Scriptable Object/Factory Data", order = int.MaxValue)]
public class FactoryData : ScriptableObject
{
    [SerializeField]
    private string factoryName;
    public string FactoryName { get { return factoryName; } }

    [SerializeField]
    private int fullItemNum;
    public int FullItemNum { get { return fullItemNum; } }
}
