using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainGameSetting : MonoBehaviour
{
    public bool isNewGame;
    public int mapSizeIndex;
    public int difficultylevel;
    public int loadDataIndex;
    public int randomSeed;
    public bool isPublic;

    #region Singleton
    public static MainGameSetting instance;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    public void NewGameState(bool isNew)
    {
        isNewGame = isNew;
    }

    public void MapSizeSet(int sizeIndex)
    {
        mapSizeIndex = sizeIndex;
    }

    public void DifficultylevelSet(int level)
    {
        difficultylevel = level;
    }

    public void LoadDataIndexSet(int index)
    {
        loadDataIndex = index;
    }

    public void RandomSeedValue(int index)
    {
        randomSeed = index;
    }
}
