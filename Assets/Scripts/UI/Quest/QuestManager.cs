using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public QuestListSO questListSO;
    List<Quest> quests;

    #region Singleton
    public static QuestManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of QuestManager found!");
            return;
        }

        instance = this;
    }
    #endregion

    void Start()
    {
        //quests = questListSO.QuestSOList;
    }
}
