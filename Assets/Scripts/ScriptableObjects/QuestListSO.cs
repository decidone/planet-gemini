using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New QuestListSO", menuName = "SOList/QuestListSO")]
public class QuestListSO : ScriptableObject
{
    public List<Quest> QuestSOList;
}
