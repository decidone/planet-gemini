using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

// UTF-8 설정
public class MonsterSpawner : NetworkBehaviour
{
    [SerializeField]
    public StructureData structureData;
    protected StructureData StructureData { set { structureData = value; } }
    public SpawnerGroupManager groupManager;

    MonsterSpawnerManager monsterSpawn;
    List<GameObject> weakMonster;
    List<GameObject> normalMonster;
    List<GameObject> strongMonster;
    GameObject guardian;

    [SerializeField] SpriteRenderer icon;

    // 최대 소환 개수 정보
    int maxWeakSpawn;
    int maxNormalSpawn;
    int maxStrongSpawn;
    int maxGuardianSpawn;

    int totalSpawnNum;  // 가딘언을 제외한 최대 소환 수    
    public int spawnNum;       // 일반 소환해야 하는 몬스터 수

    // 현재 소환 개수 정보
    public int currentWeakSpawn;
    public int currentNormalSpawn;
    public int currentStrongSpawn;

    public List<MonsterAi> totalMonsterList = new List<MonsterAi>(); // 가딘언을 제외한 몬스터 리스트
    public List<GuardianAi> guardianList = new List<GuardianAi>();
    public List<MonsterAi> waveMonsterList = new List<MonsterAi>();
    // 가디언은 초기에 배치하고 그 이후로는 관리 안함

    public int spawnerLevel;
    AreaLevelData spawnerLevelData;
    string biome;
    int spawnerGroupIndex;

    float spawnInterval = 20.0f;
    float spawnTimer;

    public bool nearUserObjExist;
    public bool nearEnergyObjExist;

    public SpriteRenderer unitSprite;
    public GameObject unitCanvas;
    public Image hpBar;
    public float hp;
    public float maxHp;
    public float defense;
    public bool dieCheck = false;
    protected CapsuleCollider2D capsuleCollider2D;

    Vector3 wavePos;

    //BattleBGMCtrl battleBGM;
    MonsterSpawnerManager monsterSpawnerManager;

    [SerializeField]
    GameObject searchColl;
    [SerializeField]
    GameObject awakeColl;
    public SpawnerSearchColl spawnerSearchColl;
    SpawnerAwake spawnerAwakeColl;
    float[] energyAggroMaxValue = new float[8] { 3000, 6000, 8000, 10000, 12000, 12000, 12000, 12000 }; // 어그로 최대 임계치
    //float[] violentEnergyAggroMaxValue = new float[8] {5000, 8000, 10000, 12000, 14000, 14000, 14000, 14000 }; // 광폭화시 최대 임계치
    public float energyAggroValue; // 어그로 현 임계치
    float[] energyAggroInterval = new float[8] { 120, 150, 180, 200, 220, 220, 220, 220 }; // 어그로 수치 체크타임
    float energyAggroTimer;
    float checkAggroInterval = 5;
    float checkAggroTimer;
    bool restTime;
    int attackLevelTier;    // 낮을수록 강한
    float[][] restAggroIntervals = new float[][] // 어그로 끌리고 쉬는 타임
    {
    new float[5] { 210, 240, 270, 300, 330 },   // Level 1
    new float[5] { 240, 270, 300, 330, 360 },   // Level 2
    new float[5] { 270, 300, 330, 360, 390 },   // Level 3
    new float[5] { 300, 330, 360, 390, 420 },   // Level 4
    new float[5] { 330, 360, 390, 420, 450 },   // Level 5
    new float[5] { 330, 360, 390, 420, 450 },   // Level 6
    new float[5] { 330, 360, 390, 420, 450 },   // Level 7
    new float[5] { 330, 360, 390, 420, 450 }    // Level 8
    };
    float restAggroTimer;
    public Dictionary<Structure, float> energyUseStrs = new Dictionary<Structure, float>();

    public bool isInHostMap;

    [HideInInspector]
    public bool violentDay;
    public float violentCollSize;
    [SerializeField]
    int safeAmount;
    [HideInInspector]
    public int safeCount;
    int ragePhase = 0;
    int maxRagePhase = 5;
    int[] ragePhaseSpawnCount = new int[5] { 2, 4, 6, 8, 10 };

    public delegate void OnHpChanged();
    public OnHpChanged onHpChangedCallback;

    [SerializeField]
    bool waveTest = false;

    void Awake()
    {
        monsterSpawn = MonsterSpawnerManager.instance;
        //battleBGM = BattleBGMCtrl.instance;
        monsterSpawnerManager = MonsterSpawnerManager.instance;
        weakMonster = GeminiNetworkManager.instance.unitListSO.weakMonsterList;
        normalMonster = GeminiNetworkManager.instance.unitListSO.normalMonsterList;
        strongMonster = GeminiNetworkManager.instance.unitListSO.strongMonsterList;
        guardian = GeminiNetworkManager.instance.unitListSO.guardian[0];
        capsuleCollider2D = GetComponent<CapsuleCollider2D>();
        spawnerSearchColl = searchColl.GetComponent<SpawnerSearchColl>();
        spawnerAwakeColl = awakeColl.GetComponent<SpawnerAwake>();
    }

    void Start()
    {
        if (!IsServer)
        {
            searchColl.SetActive(false);
            awakeColl.SetActive(false);
        }
        spawnerSearchColl.violentCollSize = violentCollSize;
        SpriteSet();

        if (IsServer && MainGameSetting.instance.isNewGame)
        {
            if (guardianList.Count < maxGuardianSpawn)
            {
                int guardianSpawnAmount = maxGuardianSpawn - guardianList.Count;
                for (int i = 0; i < guardianSpawnAmount; i++)
                {
                    SpawnMonster(3, 0, isInHostMap);
                }
            }
        }
    }

    void Update()
    {
        if (Time.timeScale == 0)
        {
            return;
        }

        if (!IsServer || dieCheck)
            return;

        if (totalSpawnNum > totalMonsterList.Count)
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnInterval)
            {
                if (nearUserObjExist)
                {
                    MonsterSpawn();
                }
                else
                {
                    spawnNum++;
                    if (totalMonsterList.Count + spawnNum > totalSpawnNum)
                    {
                        spawnNum = totalSpawnNum - totalMonsterList.Count;
                    }
                }
                spawnTimer = 0;
            }
        }

        if (!violentDay && nearEnergyObjExist && energyUseStrs.Count > 0)
        {
            if (restTime)
            {
                restAggroTimer += Time.deltaTime;

                if (restAggroTimer >= restAggroIntervals[spawnerLevel - 1][attackLevelTier])
                {
                    restTime = false;
                    restAggroTimer = 0;
                }
            }
            else
            {
                energyAggroTimer += Time.deltaTime;
                checkAggroTimer += Time.deltaTime;

                if (checkAggroTimer >= checkAggroInterval)
                {
                    StructuresEnergyCheck();
                    checkAggroTimer = 0;
                }

                if (energyAggroTimer >= energyAggroInterval[spawnerLevel - 1])
                {
                    energyAggroValue = 0;
                    energyAggroTimer = 0;
                }
            }
        }

        if(waveTest)
        {
            StartCoroutine(MonsterBaseMapCheck.instance.CheckPath(wavePos, isInHostMap));

            Invoke(nameof(WaveStart), 0.5f);
            waveTest = false;
        }
    }

    protected virtual void OnClientConnectedCallback(ulong clientId)
    {
        ClientConnectSyncServerRpc();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback -= OnClientConnectedCallback;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public virtual void ClientConnectSyncServerRpc()
    {
        ClientConnectSyncClientRpc(hp, maxHp);
    }

    [ClientRpc]
    public virtual void ClientConnectSyncClientRpc(float syncHp, float syncMaxHp)
    {
        hp = syncHp;
        maxHp = syncMaxHp;
        if (hp < maxHp)
        {
            hpBar.fillAmount = hp / maxHp;
            unitCanvas.SetActive(true);
        }
    }

    public void SpawnerSetting(AreaLevelData levelData, string _biome, Vector3 _basePos, bool isHostMap, int groupIndex)
    {
        spawnerLevelData = levelData;
        spawnerLevel = levelData.sppawnerLevel;
        biome = _biome;
        isInHostMap = isHostMap;
        spawnerGroupIndex = groupIndex;
        hp = structureData.MaxHp[levelData.sppawnerLevel - 1];
        maxHp = structureData.MaxHp[levelData.sppawnerLevel - 1];
        defense = structureData.Defense[levelData.sppawnerLevel - 1];
        maxWeakSpawn = levelData.maxWeakSpawn;
        maxNormalSpawn = levelData.maxNormalSpawn;
        maxStrongSpawn = levelData.maxStrongSpawn;
        maxGuardianSpawn = levelData.maxGuardianSpawn;
        totalSpawnNum = maxWeakSpawn + maxNormalSpawn + maxStrongSpawn;
        spawnNum = totalSpawnNum;
        wavePos = _basePos;
    }

    void MonsterSpawn()
    {
        float weakSpawnWeight = (float)maxWeakSpawn / totalSpawnNum;
        float normalSpawnWeight = (float)maxNormalSpawn / totalSpawnNum;

        // 현재 각 타입별로 더 소환할 수 있는지 미리 확인
        bool canWeak = maxWeakSpawn > currentWeakSpawn;
        bool canNormal = maxNormalSpawn > currentNormalSpawn;
        bool canStrong = maxStrongSpawn > currentStrongSpawn;

        // 소환 가능한 타입이 없으면 바로 리턴
        if (!canWeak && !canNormal && !canStrong)
        {
            return;
        }

        // 소환 가능한 타입만 가중치 재계산
        float totalWeight = 0f;
        float wWeight = canWeak ? weakSpawnWeight : 0f;
        float nWeight = canNormal ? normalSpawnWeight : 0f;
        float sWeight = canStrong ? 1f - weakSpawnWeight - normalSpawnWeight : 0f;
        totalWeight = wWeight + nWeight + sWeight;

        float spawnRandom = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        if (canWeak)
        {
            cumulative += wWeight;
            if (spawnRandom < cumulative)
            {
                RandomMonsterSpawn(0);
            }
        }
        if (canNormal)
        {
            cumulative += nWeight;
            if (spawnRandom < cumulative)
            {
                RandomMonsterSpawn(1);
            }
        }
        if (canStrong)
        {
            RandomMonsterSpawn(2);
        }
    }

    public IEnumerator ExtraMonsterSpawnCoroutine(int phase, GameObject attackObj = null)
    {
        int spawnCount = ragePhaseSpawnCount[phase - 1] + spawnerLevel;

        for (int i = 0; i < spawnCount; i++)
        {
            int monsterType = GetWeightedMonsterType(spawnerLevel);
            RandomMonsterSpawn(monsterType);

            yield return null; // 매 프레임 한 마리씩 생성
        }
        
        if(attackObj)
            TriggerRage(ragePhase, attackObj);
    }

    GameObject RandomMonsterSpawn(int monsterType)
    {
        int randomIndex = 0;
        switch (monsterType)
        {
            case 0:
                randomIndex = weakMonster.Count;
                break;
            case 1:
                randomIndex = normalMonster.Count;
                break;
            case 2:
                randomIndex = strongMonster.Count;
                break;
            default:
                break;
        }

        int randomMob = Random.Range(0, randomIndex);
        GameObject monster = SpawnMonster(monsterType, randomMob, isInHostMap);

        return monster;
    }

    int GetWeightedMonsterType(int spawnerLevel)
    {
        int total = maxWeakSpawn + maxNormalSpawn + maxStrongSpawn;
        if (total <= 0)
            return -1; // 스폰 불가 상황 (예외 처리)

        int rand = Random.Range(0, total);

        if (rand < maxWeakSpawn) return 0; // 약한 몬스터
        else if (rand < maxWeakSpawn + maxNormalSpawn) return 1; // 보통 몬스터
        else return 2; // 강한 몬스터
    }

    public GameObject SpawnMonster(int monserType, int monsterIndex, bool isInHostMap)
    {
        GameObject newMonster = null;
        if (monserType == 0)
        {
            newMonster = Instantiate(weakMonster[monsterIndex], transform.position, Quaternion.identity, this.transform);
            totalMonsterList.Add(newMonster.GetComponent<MonsterAi>());
            currentWeakSpawn++;
        }
        else if (monserType == 1)
        {
            newMonster = Instantiate(normalMonster[monsterIndex], transform.position, Quaternion.identity, this.transform);
            totalMonsterList.Add(newMonster.GetComponent<MonsterAi>());
            currentNormalSpawn++;
        }
        else if (monserType == 2)
        {
            newMonster = Instantiate(strongMonster[monsterIndex], transform.position, Quaternion.identity, this.transform);
            totalMonsterList.Add(newMonster.GetComponent<MonsterAi>());
            currentStrongSpawn++;
        }
        else if (monserType == 3)
        {
            newMonster = Instantiate(guardian, transform.position, Quaternion.identity, this.transform);
            guardianList.Add(newMonster.GetComponent<GuardianAi>());
        }

        NetworkObject networkObject = newMonster.GetComponent<NetworkObject>();
        if (!networkObject.IsSpawned) networkObject.Spawn(true);

        newMonster.transform.parent = this.transform;

        MonsterAi monsterAi = newMonster.GetComponent<MonsterAi>();
        monsterAi.MonsterSpawnerSet(this, monserType);
        monsterAi.AStarSet(isInHostMap);

        if (!nearUserObjExist && !dieCheck)
        {
            if (IsServer)
                monsterAi.MonsterScriptSetServerRpc(false);
        }
        else
        {
            if (IsServer)
                monsterAi.MonsterScriptSetServerRpc(true);
        }

        return newMonster;
    }

    public void ReturnMonster(MonsterAi monsterAi)
    {
        if (monsterAi.monsterType == 0)
        {
            totalMonsterList.Add(monsterAi);
            currentWeakSpawn++;
        }
        else if (monsterAi.monsterType == 1)
        {
            totalMonsterList.Add(monsterAi);
            currentNormalSpawn++;
        }
        else if (monsterAi.monsterType == 2)
        {
            totalMonsterList.Add(monsterAi);
            currentStrongSpawn++;
        }
    }

    public void MonsterScriptSet(bool scriptState, bool guardianState)
    {
        foreach (MonsterAi monster in totalMonsterList)
        {
            monster.MonsterScriptSetServerRpc(scriptState);
        }
        if (guardianState)
        {
            foreach (GuardianAi guardian in guardianList)
            {
                guardian.GetComponent<MonsterAi>().MonsterScriptSetServerRpc(scriptState);
            }
        }
    }

    public void MonsterDieChcek(GameObject monster, int type, bool waveState)
    {
        if (type == 3)  // 가디언
        {
            guardianList.Remove(monster.GetComponent<GuardianAi>());
            return;
        }

        if (waveState)
        {
            waveMonsterList.Remove(monster.GetComponent<MonsterAi>());
        }
        else
        {
            totalMonsterList.Remove(monster.GetComponent<MonsterAi>());
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
    }

    public void TakeDamage(float damage, GameObject attackObj)
    {
        CheckRagePattern(attackObj);

        if (!dieCheck)
            TakeDamageServerRpc(damage);
    }

    void CheckRagePattern(GameObject attackObj)
    {
        float expectedPhase = (maxHp - hp) / (maxHp / maxRagePhase);

        if (expectedPhase >= 1 && ragePhase < expectedPhase)
        {
            ragePhase++;
            bool levelPhase = LevelPhaseCheck(ragePhase);
            if(levelPhase)
                StartCoroutine(ExtraMonsterSpawnCoroutine(ragePhase, attackObj));
            else
                TriggerRage(ragePhase, attackObj);
        }
    }

    bool LevelPhaseCheck(int ragePhase)
    {
        bool pashe = false;

        if(ragePhase == 1)
        {
            pashe = spawnerLevel > 2;
        }   
        else if (ragePhase == 2)
        {
            pashe = spawnerLevel > 6;
        }
        else if (ragePhase == 3)
        {
            pashe = true;
        }
        else if (ragePhase == 4)
        {
            pashe = spawnerLevel > 4;
        }
        else if (ragePhase == 5)
        {
            pashe = true;
        }

        return pashe;
    }

    void TriggerRage(int phase, GameObject attackObj)
    {
        GuardianCall(phase, attackObj);
        MonsterCall(phase, attackObj);
    }


    [ServerRpc]
    void TakeDamageServerRpc(float damage)
    {
        TakeDamageClientRpc(damage);
    }

    [ClientRpc]
    void TakeDamageClientRpc(float damage)
    {
        if (!unitCanvas.activeSelf)
            unitCanvas.SetActive(true);
        float reducedDamage = Mathf.Max(damage - defense, 5);
        hp -= reducedDamage;
        if (hp < 0f)
            hp = 0f;
        onHpChangedCallback?.Invoke();
        hpBar.fillAmount = hp / maxHp;
        if (hp <= 0f && !dieCheck)
        {
            hp = 0f;
            dieCheck = true;
            if (IsServer)
                DieFuncServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    protected void DieFuncServerRpc()
    {
        DieFuncClientRpc();
    }

    [ClientRpc]
    protected void DieFuncClientRpc()
    {
        unitSprite.color = new Color(1f, 1f, 1f, 0f);
        unitCanvas.SetActive(false);
        capsuleCollider2D.enabled = false;

        if (InfoUI.instance.spawner == this)
            InfoUI.instance.SetDefault();

        GameObject InfoObj = GetComponentInChildren<InfoInteract>().gameObject;
        InfoObj.SetActive(false);

        MapGenerator.instance.ClearCorruption(transform.position, spawnerLevel);
        icon.enabled = false;

        if (!IsServer)
            return;
        
        monsterSpawnerManager.AreaGroupRemove(this, spawnerLevel, isInHostMap);
        Overall.instance.OverallCount(0);

        int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(ItemList.instance.itemDic["VoidShard"]);
        GeminiNetworkManager.instance.ItemSpawnServerRpc(itemIndex, 1, transform.position);

        spawnerSearchColl.DieFunc();
        spawnerAwakeColl.DieFunc();
    }

    public void DieFuncLoad()
    {
        unitSprite.color = new Color(1f, 1f, 1f, 0f);
        unitCanvas.SetActive(false);
        capsuleCollider2D.enabled = false;
        MapGenerator.instance.ClearCorruption(transform.position, spawnerLevel);
        icon.enabled = false;
        spawnerSearchColl.DieFunc();
        spawnerAwakeColl.DieFunc();
    }

    public void SearchObj(bool find)
    {
        if (!dieCheck)
            MonsterScriptSet(find, true);
        nearUserObjExist = find;
        if (nearUserObjExist)
        {
            StartCoroutine(MonsterSpawnStartCoroutine());
        }
    }

    //void MonsterSpawnStart()
    //{
    //    int spawnCount = spawnNum;
    //    if (spawnNum + totalMonsterList.Count > totalSpawnNum)
    //    {
    //        spawnCount = totalSpawnNum - totalMonsterList.Count;
    //    }

    //    for (int i = 0; i < spawnCount; i++)
    //    {
    //        MonsterSpawn();
    //        spawnNum--;
    //    }
    //}

    public IEnumerator MonsterSpawnStartCoroutine()
    {
        int spawnCount = spawnNum;

        if (spawnNum + totalMonsterList.Count > totalSpawnNum)
        {
            spawnCount = totalSpawnNum - totalMonsterList.Count;
        }

        for (int i = 0; i < spawnCount; i++)
        {
            MonsterSpawn();
            spawnNum--;

            yield return null; // 매 프레임 한 마리씩 생성
        }
    }

    void GuardianCall(int phase, GameObject attackObj)
    {
        foreach (GuardianAi guardian in guardianList)
        {
            guardian.SpawnerCallCheck(attackObj);
            if (phase == maxRagePhase - 1)
            {
                guardian.LastPhase();
            }
        }
    }

    void MonsterCall(int phase, GameObject attackObj)
    {
        foreach (MonsterAi monster in totalMonsterList)
        {
            monster.SpawnerCallCheck(attackObj);
            if (phase == maxRagePhase - 1)
            {
                monster.LastPhase();
            }
        }
    }

    public void WaveStart()
    {
        StartCoroutine(WaveStartCoroutine());
    }

    private IEnumerator WaveStartCoroutine()
    {
        if (spawnNum > 0)
        {
            yield return StartCoroutine(MonsterSpawnStartCoroutine()); // 코루틴이면 이렇게
        }

        yield return StartCoroutine(ExtraMonsterSpawnCoroutine(Mathf.CeilToInt(spawnerLevel / 2) + 1));

        // 아래는 몬스터가 전부 생성된 후 실행됨
        MonsterScriptSet(true, false);

        List<GameObject> monsters = new List<GameObject>();
        foreach (MonsterAi monster in totalMonsterList)
        {
            monster.WaveStart(wavePos);
            monsters.Add(monster.gameObject);
        }

        monsterSpawnerManager.WaveAddMonster(monsters);
        waveMonsterList.AddRange(totalMonsterList);

        totalMonsterList.Clear();
        currentWeakSpawn = 0;
        currentNormalSpawn = 0;
        currentStrongSpawn = 0;
    }

    public void GameStartSet(SpawnerSaveData spawnerSaveData, AreaLevelData levelData, Vector3 _basePos, bool isHostMap, int groupIndex)
    {
        hp = spawnerSaveData.hp;
        maxHp = structureData.MaxHp[spawnerSaveData.level - 1];
        defense = structureData.Defense[levelData.sppawnerLevel - 1];

        if (hp < maxHp)
        {
            hpBar.fillAmount = hp / maxHp;
            unitCanvas.SetActive(true);
        }
        spawnerLevel = spawnerSaveData.level;
        spawnNum = spawnerSaveData.spawnNum;
        nearUserObjExist = spawnerSaveData.nearUserObjExist;
        nearEnergyObjExist = spawnerSaveData.nearEnergyObjExist;
        ragePhase = spawnerSaveData.ragePhase;
        isInHostMap = isHostMap;
        spawnerGroupIndex = groupIndex;
        safeCount = spawnerSaveData.safeCount;
        violentDay = spawnerSaveData.violentDay;
        spawnerLevelData = levelData;
        spawnerLevel = levelData.sppawnerLevel;
        maxWeakSpawn = levelData.maxWeakSpawn;
        maxNormalSpawn = levelData.maxNormalSpawn;
        maxStrongSpawn = levelData.maxStrongSpawn;
        maxGuardianSpawn = levelData.maxGuardianSpawn;
        totalSpawnNum = maxWeakSpawn + maxNormalSpawn + maxStrongSpawn;
        wavePos = _basePos;
    }

    public GameObject WaveMonsterSpawn(int monserType, int monsterIndex, bool isInHostMap, bool isWaveColonyCallCheck)
    {
        GameObject newMonster = null;
        if (monserType == 0)
        {
            newMonster = Instantiate(weakMonster[monsterIndex]);
            waveMonsterList.Add(newMonster.GetComponent<MonsterAi>());
        }
        else if (monserType == 1)
        {
            newMonster = Instantiate(normalMonster[monsterIndex]);
            waveMonsterList.Add(newMonster.GetComponent<MonsterAi>());
        }
        else if (monserType == 2)
        {
            newMonster = Instantiate(strongMonster[monsterIndex]);
            waveMonsterList.Add(newMonster.GetComponent<MonsterAi>());
        }

        NetworkObject networkObject = newMonster.GetComponent<NetworkObject>();
        if (!networkObject.IsSpawned) networkObject.Spawn(true);

        newMonster.transform.SetParent(this.transform, false);

        MonsterAi monsterAi = newMonster.GetComponent<MonsterAi>();
        monsterAi.MonsterSpawnerSet(this, monserType);
        monsterAi.AStarSet(isInHostMap);

        if (IsServer)
        {
            monsterAi.MonsterScriptSetServerRpc(true);
            monsterSpawnerManager.WaveAddMonster(newMonster);
        }

        return newMonster;
    }

    void StructuresEnergyCheck()
    {
        foreach (var data in energyUseStrs)
        {
            if (data.Key.isOperate)
            {
                energyAggroValue += data.Value;
            }
        }

        float maxAggroValue = energyAggroMaxValue[spawnerLevel - 1];

        if (energyAggroValue > maxAggroValue)
        {
            restTime = true;
            attackLevelTier = (int)(energyAggroTimer / (energyAggroInterval[spawnerLevel - 1] / restAggroIntervals[spawnerLevel - 1].Length));
            energyAggroValue = 0;
            checkAggroTimer = 0;
            energyAggroTimer = 0;
            Debug.Log("lv : " +attackLevelTier);
            EnergyUseStrAttack(attackLevelTier);
        }
    }

    public void EnergyUseStrAttack(int attackLevel)
    {
        StartCoroutine(EnergyUseStrAttackCoroutine(attackLevel));
        //if (spawnNum > 0)
        //{
        //    MonsterSpawnStart();
        //}

        //Debug.Log("EnergyUseStrAttack");
        //float attackPercentage = GetSpawnPercentage(attackLevel);
        //int monstersToAttackCount = Mathf.CeilToInt(totalMonsterList.Count * attackPercentage);
        //List<GameObject> monsters = new List<GameObject>();
        //List<Structure> structures = new List<Structure>(energyUseStrs.Keys);

        //// 총 가중치 합을 계산합니다.
        //float totalWeight = 0f;
        //foreach (var entry in energyUseStrs)
        //{
        //    totalWeight += entry.Value;
        //}

        //if (totalWeight == 0f)
        //{
        //    Debug.LogWarning("No structures with positive energy usage for attack.");
        //    return;
        //}

        //for (int i = 0; i < monstersToAttackCount; i++)
        //{
        //    MonsterAi monsterAi = totalMonsterList[i];
        //    monsterAi.MonsterScriptSetServerRpc(true);
        //    // 가중치를 기반으로 랜덤하게 구조물을 선택합니다.
        //    float randomWeight = Random.Range(0f, totalWeight);
        //    float cumulativeWeight = 0f;
        //    Structure selectedStructure = null;

        //    foreach (var entry in energyUseStrs)
        //    {
        //        cumulativeWeight += entry.Value;

        //        if (randomWeight <= cumulativeWeight)
        //        {
        //            selectedStructure = entry.Key;
        //            break;
        //        }
        //    }
        //    Debug.Log(selectedStructure);
        //    // 선택된 구조물로 몬스터를 공격하게 합니다.
        //    if (selectedStructure != null)
        //    {
        //        monsterAi.ColonyAttackStart(selectedStructure.transform.position);
        //    }

        //    monsters.Add(monsterAi.gameObject);
        //    waveMonsterList.Add(monsterAi);

        //    // 몬스터 타입에 따라 카운트를 감소시킵니다.
        //    if (monsterAi.monsterType == 0)
        //    {
        //        currentWeakSpawn--;
        //    }
        //    else if (monsterAi.monsterType == 1)
        //    {
        //        currentNormalSpawn--;
        //    }
        //    else if (monsterAi.monsterType == 2)
        //    {
        //        currentStrongSpawn--;
        //    }
        //}

        //foreach (var monster in monsters)
        //{
        //    totalMonsterList.Remove(monster.GetComponent<MonsterAi>());
        //}

        ////battleBGM.ColonyCallAddMonster(monsters, isInHostMap);
        //WarningWindowSetServerRpc();
    }

    private IEnumerator EnergyUseStrAttackCoroutine(int attackLevel)
    {
        if (spawnNum > 0)
        {
            yield return StartCoroutine(MonsterSpawnStartCoroutine()); // 코루틴이면 이렇게
        }

        Debug.Log("EnergyUseStrAttack");
        float attackPercentage = GetSpawnPercentage(attackLevel);
        int monstersToAttackCount = Mathf.CeilToInt(totalMonsterList.Count * attackPercentage);
        List<GameObject> monsters = new List<GameObject>();
        List<Structure> structures = new List<Structure>(energyUseStrs.Keys);

        // 총 가중치 합을 계산합니다.
        float totalWeight = 0f;
        foreach (var entry in energyUseStrs)
        {
            totalWeight += entry.Value;
        }

        for (int i = 0; i < monstersToAttackCount; i++)
        {
            MonsterAi monsterAi = totalMonsterList[i];
            monsterAi.MonsterScriptSetServerRpc(true);
            // 가중치를 기반으로 랜덤하게 구조물을 선택합니다.
            float randomWeight = Random.Range(0f, totalWeight);
            float cumulativeWeight = 0f;
            Structure selectedStructure = null;

            foreach (var entry in energyUseStrs)
            {
                cumulativeWeight += entry.Value;

                if (randomWeight <= cumulativeWeight)
                {
                    selectedStructure = entry.Key;
                    break;
                }
            }
            Debug.Log(selectedStructure);
            // 선택된 구조물로 몬스터를 공격하게 합니다.
            if (selectedStructure != null)
            {
                monsterAi.ColonyAttackStart(selectedStructure.transform.position);
            }

            monsters.Add(monsterAi.gameObject);
            waveMonsterList.Add(monsterAi);

            // 몬스터 타입에 따라 카운트를 감소시킵니다.
            if (monsterAi.monsterType == 0)
            {
                currentWeakSpawn--;
            }
            else if (monsterAi.monsterType == 1)
            {
                currentNormalSpawn--;
            }
            else if (monsterAi.monsterType == 2)
            {
                currentStrongSpawn--;
            }
        }

        foreach (var monster in monsters)
        {
            totalMonsterList.Remove(monster.GetComponent<MonsterAi>());
        }

        //battleBGM.ColonyCallAddMonster(monsters, isInHostMap);
        WarningWindowSetServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    void WarningWindowSetServerRpc()
    {
        WarningWindowSetClientRpc();
    }

    [ClientRpc]
    void WarningWindowSetClientRpc()
    {
        WarningWindow.instance.WarningTextSet("Attack detected on", isInHostMap);
    }

    private float GetSpawnPercentage(int tier)
    {
        switch (tier)
        {
            case 0:
                return 1.0f;
            case 1:
                return 0.80f;
            case 2:
                return 0.60f;
            case 3:
                return 0.40f;
            case 4:
                return 0.20f;
            default:
                return 0f;
        }
    }

    public SpawnerSaveData SaveData()
    {
        SpawnerSaveData data = new SpawnerSaveData();

        data.hp = hp;
        data.level = spawnerLevel;
        data.wavePos = Vector3Extensions.FromVector3(wavePos);
        data.spawnerPos = Vector3Extensions.FromVector3(transform.position);
        data.spawnNum = spawnNum;
        data.dieCheck = dieCheck;
        data.spawnerGroupIndex = spawnerGroupIndex;
        data.safeCount = safeCount;
        data.violentDay = violentDay;
        data.violentCollSize = spawnerSearchColl.violentCollSize;
        data.nearUserObjExist = nearUserObjExist;
        data.nearEnergyObjExist = nearEnergyObjExist;
        data.ragePhase = ragePhase;

        foreach (MonsterAi monster in totalMonsterList)
        {
            data.monsterList.Add(monster.SaveData());
        }
        foreach (MonsterAi monster in waveMonsterList)
        {
            data.monsterList.Add(monster.SaveData());
        }
        foreach (MonsterAi monster in guardianList)
        {
            data.monsterList.Add(monster.SaveData());
        }

        return data;
    }

    public void SetCorruption()
    {
        if (!dieCheck)
            MapGenerator.instance.SetCorruption(transform.position, spawnerLevel);
    }

    public void SearchCollExtend()
    {
        spawnerSearchColl.SearchCollExtend();
    }

    public void SearchCollFullExtend()
    {
        spawnerSearchColl.SearchCollFullExtend();
    }

    public float EnergyUseCheck()
    {
        float aggroValue = 0;
        foreach (var data in energyUseStrs)
        {
            if (data.Key.isOperate)
            {
                aggroValue += data.Value;
            }
        }
        return aggroValue;
    }

    public void SearchCollReturn()
    {
        spawnerSearchColl.SearchCollReturn();
        violentDay = false;
    }

    public void ViolentDaySet()
    {
        violentDay = true;
        safeCount = safeAmount;
        spawnerSearchColl.ViolentCollSizeReduction();
    }

    public void SpawnerLevelUp()
    {
        if (spawnerLevel < SpawnerSetManager.instance.arealevelData.Length)
        {
            spawnerLevel++;
            monsterSpawnerManager.AreaGroupLevelUp(this, spawnerLevel - 1, spawnerLevel, isInHostMap);
            spawnerLevelData = SpawnerSetManager.instance.arealevelData[spawnerLevel - 1];
            hp = structureData.MaxHp[spawnerLevel - 1];
            maxHp = structureData.MaxHp[spawnerLevel - 1];
            defense = structureData.Defense[spawnerLevel - 1];

            maxWeakSpawn = spawnerLevelData.maxWeakSpawn;
            maxNormalSpawn = spawnerLevelData.maxNormalSpawn;
            maxStrongSpawn = spawnerLevelData.maxStrongSpawn;
            maxGuardianSpawn = spawnerLevelData.maxGuardianSpawn;
            totalSpawnNum = maxWeakSpawn + maxNormalSpawn + maxStrongSpawn;
            SpriteSet();
        }

        if (guardianList.Count < maxGuardianSpawn)
        {
            int guardianSpawnAmount = maxGuardianSpawn - guardianList.Count;
            for (int i = 0; i < guardianSpawnAmount; i++)
            {
                SpawnMonster(3, 0, isInHostMap);
            }
        }
    }

    public void SafeCountDown()
    {
        safeCount--;
        if (safeCount < 0)
        {
            safeCount = 0;
        }
    }


    void SpriteSet()
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();

        if (spawnerLevel >= 7)
        {
            sprite.sprite = monsterSpawnerManager.spawnerSprite[2];
            capsuleCollider2D.size = new Vector2(5f, 1.8f);
        }
        else if (spawnerLevel <= 3)
        {
            sprite.sprite = monsterSpawnerManager.spawnerSprite[0];
            capsuleCollider2D.size = new Vector2(2.6f, 0.9f);
        }
        else
        {
            sprite.sprite = monsterSpawnerManager.spawnerSprite[1];
            capsuleCollider2D.size = new Vector2(3.5f, 1f);
        }
    }
}
