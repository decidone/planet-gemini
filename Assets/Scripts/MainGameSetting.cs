using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainGameSetting : MonoBehaviour
{
    public bool isNewGame;
    public int mapSizeIndex;
    public int loadDataIndex;

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

    public void LoadDataIndexSet(int index)
    {
        loadDataIndex = index;
    }
}
