using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleBGMCtrl : MonoBehaviour
{
    SoundManager soundManager;
    public List<GameObject> battleMonsters = new List<GameObject>();
    public List<GameObject> colonyCallMonsters = new List<GameObject>();
    public List<GameObject> waveMonsters = new List<GameObject>();
    bool isHostMapBattleBGMOn = false;
    bool isHostMapWaveState = false;
    bool isClientMapBattleBGMOn = false;
    bool isClientMapWaveState = false;

    #region Singleton
    public static BattleBGMCtrl instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }
    #endregion

    void Start()
    {
        soundManager = SoundManager.instance;
    }

    public void BattleAddMonster(GameObject monster, bool isInHostMap)
    {
        if (!battleMonsters.Contains(monster))
        {
            battleMonsters.Add(monster);
            if (isInHostMap)
            {
                if (!isHostMapBattleBGMOn)
                {
                    isHostMapBattleBGMOn = true;
                    soundManager.BattleStateSetServerRpc(isHostMapBattleBGMOn, false, isInHostMap);
                }
            }
            else
            {
                if (!isClientMapBattleBGMOn)
                {
                    isClientMapBattleBGMOn = true;
                    soundManager.BattleStateSetServerRpc(isClientMapBattleBGMOn, false, isInHostMap);
                }
            }
        }
    }

    public void ColonyCallAddMonster(List<GameObject> monsters, bool isInHostMap)
    {
        colonyCallMonsters.AddRange(monsters);
        if (isInHostMap)
        {
            if (!isHostMapBattleBGMOn)
            {
                isHostMapBattleBGMOn = true;
                soundManager.BattleStateSetServerRpc(isHostMapBattleBGMOn, false, isInHostMap);
            }
        }
        else
        {
            if (!isClientMapBattleBGMOn)
            {
                isClientMapBattleBGMOn = true;
                soundManager.BattleStateSetServerRpc(isClientMapBattleBGMOn, false, isInHostMap);
            }
        }
    }

    public void ColonyCallAddMonster(GameObject monster, bool isInHostMap)
    {
        colonyCallMonsters.Add(monster);
        if (isInHostMap)
        {
            if (!isHostMapBattleBGMOn)
            {
                isHostMapBattleBGMOn = true;
                soundManager.BattleStateSetServerRpc(isHostMapBattleBGMOn, false, isInHostMap);
            }
        }
        else
        {
            if (!isClientMapBattleBGMOn)
            {
                isClientMapBattleBGMOn = true;
                soundManager.BattleStateSetServerRpc(isClientMapBattleBGMOn, false, isInHostMap);
            }
        }
    }

    public void WaveStart(bool isInHostMap)
    {
        if (isInHostMap)
        {
            isHostMapWaveState = true;
            isHostMapBattleBGMOn = true;
            soundManager.BattleStateSetServerRpc(isHostMapBattleBGMOn, true, isInHostMap);
        }
        else
        {
            isClientMapWaveState = true;
            isClientMapBattleBGMOn = true;
            soundManager.BattleStateSetServerRpc(isClientMapBattleBGMOn, true, isInHostMap);
        }      
    }

    public void WaveAddMonster(List<GameObject> monsters)
    {
        waveMonsters.AddRange(monsters);
    }

    public void WaveAddMonster(GameObject monster)
    {
        waveMonsters.Add(monster);
    }

    public void BattleRemoveMonster(GameObject monster, bool isInHostMap)
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

        BattleBGMOffSet(isInHostMap);
    }

    void BattleBGMOffSet(bool isInHostMap)
    {
        if (isInHostMap)
        {
            if (isHostMapWaveState)
            {
                if (waveMonsters.Count == 0)
                {
                    if (battleMonsters.Count == 0 && colonyCallMonsters.Count == 0)
                    {
                        isHostMapBattleBGMOn = false;
                        soundManager.BattleStateSetServerRpc(isHostMapBattleBGMOn, false, isInHostMap);
                    }
                    else
                    {
                        isHostMapBattleBGMOn = true;
                        soundManager.BattleStateSetServerRpc(isHostMapBattleBGMOn, false, isInHostMap);
                    }
                    isHostMapWaveState = false;
                    WavePoint.instance.WaveEnd(true);
                    MonsterSpawnerManager.instance.WaveEnd();
                }
            }
            else if (battleMonsters.Count == 0 && colonyCallMonsters.Count == 0 && isHostMapBattleBGMOn)
            {
                isHostMapBattleBGMOn = false;
                soundManager.BattleStateSetServerRpc(isHostMapBattleBGMOn, false, isInHostMap);
            }
        }
        else
        {
            if (isClientMapWaveState)
            {
                if (waveMonsters.Count == 0)
                {
                    if (battleMonsters.Count == 0 && colonyCallMonsters.Count == 0)
                    {
                        isClientMapBattleBGMOn = false;
                        soundManager.BattleStateSetServerRpc(isClientMapBattleBGMOn, false, isInHostMap);
                    }
                    else
                    {
                        isClientMapBattleBGMOn = true;
                        soundManager.BattleStateSetServerRpc(isClientMapBattleBGMOn, false, isInHostMap);
                    }
                    isClientMapWaveState = false;
                    WavePoint.instance.WaveEnd(true);
                    MonsterSpawnerManager.instance.WaveEnd();
                }
            }
            else if (battleMonsters.Count == 0 && colonyCallMonsters.Count == 0 && isClientMapBattleBGMOn)
            {
                isClientMapBattleBGMOn = false;
                soundManager.BattleStateSetServerRpc(isClientMapBattleBGMOn, false, isInHostMap);
            }
        }
    }
}
