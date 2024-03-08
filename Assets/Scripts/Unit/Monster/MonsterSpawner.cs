using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

// UTF-8 설정
public class MonsterSpawner : NetworkBehaviour
{
    [SerializeField]
    public StructureData structureData; 
    protected StructureData StructureData { set { structureData = value; } }
    public SpawnerGroupManager groupManager;

    MonsterSpawnerManager monsterSpawn;
    GameObject[] weakMonster;
    GameObject[] normalMonster;
    GameObject[] strongMonster;
    GameObject guardian;

    // 최대 소환 개수 정보
    int maxWeakSpawn;
    int maxNormalSpawn;
    int maxStrongSpawn;
    int maxExtraSpawn;
    int maxGuardianSpawn;

    int totalSpawnNum;  // 가딘언을 제외한 최대 소환 수
    public int extraSpawnNum;  // 임시 저장 소환 수

    // 현재 소환 개수 정보
    public int currentWeakSpawn;
    public int currentNormalSpawn;
    public int currentStrongSpawn;

    public List<GameObject> totalMonsterList = new List<GameObject>(); // 가딘언을 제외한 몬스터 리스트
    public List<GuardianAi> guardianList = new List<GuardianAi>();
    // 가디언은 초기에 배치하고 그 이후로는 관리 안함

    int areaLevel;
    AreaLevelData areaLevelData;
    string biome;

    float spawnInterval;
    float spawnTimer;

    public bool nearUserObjExist;

    public SpriteRenderer unitSprite;
    public GameObject unitCanvas;
    public Image hpBar;
    public float hp;
    bool dieCheck = false;
    protected BoxCollider2D boxColl2D;
    bool extraSpawn;
    bool takeDamageCheck;
    float guardianCallInterval;
    float guardianCallTimer;

    Vector3 wavePos;
    [SerializeField]
    bool waveStart = false;

    BattleBGMCtrl battleBGM;
    MonsterSpawnerManager monsterSpawnerManager;

    [SerializeField]
    GameObject searchColl;

    void Start()
    {
        monsterSpawn = MonsterSpawnerManager.instance;
        battleBGM = BattleBGMCtrl.instance;
        monsterSpawnerManager = MonsterSpawnerManager.instance;
        weakMonster = monsterSpawn.weakMonsters;
        normalMonster = monsterSpawn.normalMonsters;
        strongMonster = monsterSpawn.strongMonsters;
        guardian = monsterSpawn.guardian;
        boxColl2D = GetComponent<BoxCollider2D>();
        spawnInterval = 20;
        guardianCallInterval = 2;
        maxExtraSpawn = 10;
        extraSpawn = false;
        if (!IsServer)
            searchColl.SetActive(false);
        InitializeMonsterSpawn();
    }

    void Update()
    {
        if (!IsServer)
            return;

        if (totalSpawnNum > totalMonsterList.Count)
        {
            spawnTimer += Time.deltaTime;
            if(spawnTimer >= spawnInterval)
            {
                MonsterSpawn();
                spawnTimer = 0;
            }
        }
        else if(maxExtraSpawn > extraSpawnNum)
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnInterval)
            {
                ExtraSpawnCount();
                spawnTimer = 0;
            }
        }

        if(!extraSpawn && nearUserObjExist && totalMonsterList.Count < totalSpawnNum / 2 && extraSpawnNum > 0)
        {
            ExtraMonsterSpawn();
        }

        if (takeDamageCheck)
        {
            guardianCallTimer += Time.deltaTime;
            if (guardianCallTimer >= guardianCallInterval)
            {
                takeDamageCheck = false;
            }
        }

        if(waveStart)
        { 
            WaveStart();
            waveStart = false;
        }
    }

    public void SpawnerSetting(AreaLevelData levelData, string _biome, Vector3 _basePos)
    {
        areaLevelData = levelData;
        areaLevel = levelData.areaLevel;
        biome = _biome;

        //maxWeakSpawn = 1;
        //maxNormalSpawn = 0;
        //maxStrongSpawn = 0;
        //maxGuardianSpawn = 0;

        maxWeakSpawn = levelData.maxWeakSpawn;
        maxNormalSpawn = levelData.maxNormalSpawn;
        maxStrongSpawn = levelData.maxStrongSpawn;
        maxGuardianSpawn = levelData.maxGuardianSpawn;

        wavePos = _basePos;
    }

    public void InitializeMonsterSpawn()
    {
        for (int a = 0; a < maxWeakSpawn; a++)
        {
            RandomMonsterSpawn(0);
        }
        for (int a = 0; a < maxNormalSpawn; a++)
        {
            RandomMonsterSpawn(1);
        }
        for (int a = 0; a < maxStrongSpawn; a++)
        {
            RandomMonsterSpawn(2);
        }
        for (int a = 0; a < maxGuardianSpawn; a++)
        {
            SpawnMonster(3, 0);
        }

        totalSpawnNum = maxWeakSpawn + maxNormalSpawn + maxStrongSpawn;
    }

    void MonsterSpawn()
    {
        float weakSpawnWeight = (float)maxWeakSpawn / totalSpawnNum;
        float normalSpawnWeight = (float)maxNormalSpawn / totalSpawnNum;

        bool canSpawn = true;

        while (canSpawn)
        {
            float spawnRandom = Random.Range(0f, 1f);

            if (spawnRandom < weakSpawnWeight)
            {
                if (maxWeakSpawn > currentWeakSpawn)
                {
                    RandomMonsterSpawn(0);
                    canSpawn = false;
                }
            }
            else if (spawnRandom < weakSpawnWeight + normalSpawnWeight)
            {
                if (maxNormalSpawn > currentNormalSpawn)
                {
                    RandomMonsterSpawn(1);
                    canSpawn = false;
                }
            }
            else
            {
                if (maxStrongSpawn > currentStrongSpawn)
                {
                    RandomMonsterSpawn(2);
                    canSpawn = false;
                }
            }
        }
    }

    void ExtraSpawnCount()
    {
        extraSpawnNum++;
    }

    void ExtraMonsterSpawn()
    {
        extraSpawn = true;
        for (int i = 0; i < extraSpawnNum; i++)
        {
            if (totalMonsterList.Count == totalSpawnNum)
            {
                extraSpawn = false;
                return;
            }
            MonsterSpawn();
            extraSpawnNum--;
        }

        extraSpawn = false;
    }

    GameObject RandomMonsterSpawn(int monsterType)
    {
        int randomIndex = 0;
        switch (monsterType)
        {
            case 0:
                randomIndex = weakMonster.Length;
                break;
            case 1:
                randomIndex = normalMonster.Length;
                break;
            case 2:
                randomIndex = strongMonster.Length;
                break;
            default:
                break;
        }

        int randomMob = Random.Range(0, randomIndex);
        GameObject monster = SpawnMonster(monsterType, randomMob);

        return monster;
    }

    GameObject SpawnMonster(int monserType, int monsterIndex)
    {
        GameObject newMonster = null;
        if (monserType == 0)
        {
            newMonster = Instantiate(weakMonster[monsterIndex]);
            totalMonsterList.Add(newMonster);
            currentWeakSpawn++;
        }
        else if (monserType == 1)
        {
            newMonster = Instantiate(normalMonster[monsterIndex]);
            totalMonsterList.Add(newMonster);
            currentNormalSpawn++;
        }
        else if (monserType == 2)
        {
            newMonster = Instantiate(strongMonster[monsterIndex]);
            totalMonsterList.Add(newMonster);
            currentStrongSpawn++;
        }
        else if (monserType == 3)
        {
            newMonster = Instantiate(guardian);
            guardianList.Add(newMonster.GetComponent<GuardianAi>());
        }

        NetworkObject networkObject = newMonster.GetComponent<NetworkObject>();
        networkObject.Spawn();

        newMonster.transform.SetParent(this.transform, false);

        int x = 0;
        int y = 0;
        if (!nearUserObjExist)
        {
            x = (int)Random.Range(-5, 5);
            y = (int)Random.Range(-5, 5);
        }

        Vector3 setPos = this.transform.position + new Vector3(x, y);
        newMonster.transform.position = setPos;

        newMonster.GetComponent<MonsterAi>().MonsterSpawnerSet(this, monserType);
        if (!nearUserObjExist)
        {
            if(IsServer)
                newMonster.GetComponent<MonsterAi>().MonsterScriptSetClientRpc(false);
        }
        else
        {
            if (IsServer)
                newMonster.GetComponent<MonsterAi>().MonsterScriptSetClientRpc(true);
        }
        
        return newMonster;
    }

    public void MonsterScriptSet(bool scriptState, bool guardianState)
    {
        foreach (GameObject monster in totalMonsterList)
        {
            monster.GetComponent<MonsterAi>().MonsterScriptSetClientRpc(scriptState);
        }
        if (guardianState)
        {
            foreach (GuardianAi guardian in guardianList)
            {
                guardian.GetComponent<MonsterAi>().MonsterScriptSetClientRpc(scriptState);
            }
        }

    }

    public void MonsterDieChcek(GameObject monster, int type)
    {
        if (type == 3)  // 가디언
        {
            guardianList.Remove(monster.GetComponent<GuardianAi>());
            return;
        }

        totalMonsterList.Remove(monster);
        switch (type)
        {
            case 0:
                currentWeakSpawn--;
                break;
            case 1:
                currentNormalSpawn--;
                break;
            case 2:
                currentStrongSpawn--;
                break;
            default:
                break;
        }
    }

    public virtual void TakeDamage(float damage, GameObject attackObj)
    {
        if (!unitCanvas.activeSelf)
            unitCanvas.SetActive(true);

        if (!takeDamageCheck)
            GuardianCall(attackObj);

        hp -= damage;
        hpBar.fillAmount = hp / structureData.MaxHp[areaLevel - 1];

        if (hp <= 0f && !dieCheck)
        {
            hp = 0f;
            dieCheck = true;
            //DieFunc();
        }
    }

    protected virtual void DieFunc()
    {
        unitSprite.color = new Color(1f, 1f, 1f, 0f);
        unitCanvas.SetActive(false);
        monsterSpawnerManager.AreaGroupRemove(this, areaLevel);
        boxColl2D.enabled = false;
    }

    public void SearchObj(bool find)
    {
        MonsterScriptSet(find, true);
        nearUserObjExist = find;
    }

    void GuardianCall(GameObject attackObj)
    {
        takeDamageCheck = true;

        foreach (GuardianAi guardian in guardianList)
        {
            guardian.SpawnerCollCheck(attackObj);
        }
    }

    public void WaveTeleport(Vector3 pos)
    {
        MonsterScriptSet(true, false);
        foreach (GameObject monster in totalMonsterList)
        {
            MonsterAi monsterAi = monster.GetComponent<MonsterAi>();
            monsterAi.WaveTeleport(pos);
        }

        Invoke(nameof(WaveStart), 40.0f);
    }

    public void WaveStart()
    {
        MonsterScriptSet(true, false);
        foreach (GameObject monster in totalMonsterList)
        {
            MonsterAi monsterAi = monster.GetComponent<MonsterAi>();
            monsterAi.WaveStart(wavePos);
        }

        battleBGM.WaveAddMonster(totalMonsterList);

        totalMonsterList.Clear();
        currentWeakSpawn = 0;
        currentNormalSpawn = 0;
        currentStrongSpawn = 0;
    }

    public void ColonyCall(EnergyColony colony)
    {
        MonsterScriptSet(true, false);
        foreach (GameObject monster in totalMonsterList)
        {
            MonsterAi monsterAi = monster.GetComponent<MonsterAi>();
            monsterAi.ColonyAttackStart(colony.transform.position);
        }

        battleBGM.ColonyCallAddMonster(totalMonsterList);

        totalMonsterList.Clear();
        currentWeakSpawn = 0;
        currentNormalSpawn = 0;
        currentStrongSpawn = 0;
    }
}
