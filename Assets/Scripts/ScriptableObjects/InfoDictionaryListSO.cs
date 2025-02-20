using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InfoDictionaryListSO", menuName = "SOList/InfoDictionaryListSO")]
public class InfoDictionaryListSO : ScriptableObject
{
    public List<InfoDictionarySO> infoDictionarySOList;
}
