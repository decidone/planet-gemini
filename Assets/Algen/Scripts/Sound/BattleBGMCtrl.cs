using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleBGMCtrl : MonoBehaviour
{
    SoundManager soundManager;
    public List<GameObject> battleMonsters = new List<GameObject>();
    public List<GameObject> colonyCallMonsters = new List<GameObject>();
    public List<GameObject> waveMonsters = new List<GameObject>();
    bool battleBGMOn = false;
    bool waveState = false;
    #region Singleton
    public static BattleBGMCtrl instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of GameManager found!");
            return;
        }

        instance = this;
    }
    #endregion

    void Start()
    {
        soundManager = SoundManager.Instance;
    }

    public void BattleAddMonster(GameObject monster)
    {
        if (!battleMonsters.Contains(monster))
        {
            battleMonsters.Add(monster);
            if (!battleBGMOn)
            {
                battleBGMOn = true;
                soundManager.BattleStateSet(battleBGMOn, false);
            }
        }
    }

    public void ColonyCallAddMonster(List<GameObject> monsters)
    {
        colonyCallMonsters.AddRange(monsters);

        if (!battleBGMOn)
        {
            battleBGMOn = true;
            soundManager.BattleStateSet(battleBGMOn, false);
        }
    }

    public void WaveAddMonster(List<GameObject> monsters)
    {
        waveMonsters.AddRange(monsters);

        if (!waveState)
        {
            waveState = true;
            battleBGMOn = true;
            soundManager.BattleStateSet(battleBGMOn, true);
        }
    }

    public void BattleRemoveMonster(GameObject monster)
    {
        if (battleMonsters.Contains(monster))
        {
            battleMonsters.Remove(monster);
        }

        if (colonyCallMonsters.Contains(monster))
        {
            colonyCallMonsters.Remove(monster);
        }

        if (waveMonsters.Contains(monster))
        {
            waveMonsters.Remove(monster);
        }

        BattleBGMOffSet();
    }

    void BattleBGMOffSet()
    {
        if (waveState)
        {
            if (waveMonsters.Count == 0)
            {
                if (battleMonsters.Count == 0 && colonyCallMonsters.Count == 0)
                {
                    battleBGMOn = false;
                    soundManager.BattleStateSet(battleBGMOn, false);
                }
                else
                {
                    battleBGMOn = true;
                    soundManager.BattleStateSet(battleBGMOn, false);
                }
                waveState = false;
            }
        }
        else if (battleMonsters.Count == 0 && colonyCallMonsters.Count == 0 && battleBGMOn)
        {
            battleBGMOn = false;
            soundManager.BattleStateSet(battleBGMOn, false);
        }
    }
}
