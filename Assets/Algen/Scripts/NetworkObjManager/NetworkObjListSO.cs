using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[CreateAssetMenu(fileName = "NetworkObjListSO", menuName = "SOList/NetworkObjListSO")]
public class NetworkObjListSO : ScriptableObject
{
    public List<GameObject> networkObjList;
}
