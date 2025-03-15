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
    int maxExtraSpawn;
    int maxGuardianSpawn;

    int totalSpawnNum;  // 가딘언을 제외한 최대 소환 수
    public int extraSpawnNum;  // 임시 저장 소환 수

    // 현재 소환 개수 정보
    public int currentWeakSpawn;
    public int currentNormalSpawn;
    public int currentStrongSpawn;

    public List<MonsterAi> totalMonsterList = new List<MonsterAi>(); // 가딘언을 제외한 몬스터 리스트
    public List<GuardianAi> guardianList = new List<GuardianAi>();
    public List<MonsterAi> waveMonsterList = new List<MonsterAi>();
    // 가디언은 초기에 배치하고 그 이후로는 관리 안함

    public int sppawnerLevel;
    AreaLevelData sppawnerLevelData;
    string biome;
    int spawnerGroupIndex;

    float spawnInterval;
    float spawnTimer;

    public bool nearUserObjExist;
    public bool nearEnergyObjExist;

    public SpriteRenderer unitSprite;
    public GameObject unitCanvas;
    public Image hpBar;
    public float hp;
    public float maxHp;
    public bool dieCheck = false;
    protected CapsuleCollider2D capsuleCollider2D;
    public bool extraSpawn;
    bool takeDamageCheck;
    float guardianCallInterval;
    float guardianCallTimer;

    Vector3 wavePos;
    float waveInterval = 40;
    public float waveTimer;
    bool waveState;

    //BattleBGMCtrl battleBGM;
    MonsterSpawnerManager monsterSpawnerManager;

    [SerializeField]
    GameObject searchColl;
    [SerializeField]
    GameObject awakeColl;
    public SpawnerSearchColl spawnerSearchColl;
    SpawnerAwake spawnerAwakeColl;
    float[] energyAggroMaxValue = new float[8] {3000, 6000, 8000, 10000, 12000, 12000, 12000, 12000 }; // 어그로 최대 임계치
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
    bool gameLodeSet = false;

    [HideInInspector]
    public bool violentDay;
    public float violentCollSize;
    [SerializeField]
    int safeAmount;
    [HideInInspector]
    public int safeCount; 

    public delegate void OnHpChanged();
    public OnHpChanged onHpChangedCallback;

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
        spawnInterval = 20;
        guardianCallInterval = 2;
        maxExtraSpawn = 10;
        extraSpawn = false;
        if (!IsServer)
        {
            searchColl.SetActive(false);
            awakeColl.SetActive(false);
        }
        if (!gameLodeSet)
            InitializeMonsterSpawn();
        spawnerSearchColl.violentCollSize = violentCollSize;
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();

        if (sppawnerLevel >= 7)
        {
            sprite.sprite = monsterSpawnerManager.spawnerSprite[2];
            capsuleCollider2D.size = new Vector2(5f, 1.8f);
        }
        else if (sppawnerLevel <= 3)
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
                MonsterSpawn();
                spawnTimer = 0;
            }
        }
        else if (maxExtraSpawn > extraSpawnNum)
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnInterval)
            {
                ExtraSpawnCount();
                spawnTimer = 0;
            }
        }

        if (!extraSpawn && nearUserObjExist && totalMonsterList.Count < totalSpawnNum / 2 && extraSpawnNum > 0)
        {
            ExtraMonsterSpawn();
        }
        
        if (!violentDay && nearEnergyObjExist && energyUseStrs.Count > 0)
        {
            if (restTime)
            {
                restAggroTimer += Time.deltaTime;
                Debug.Log(attackLevelTier);
                if (restAggroTimer >= restAggroIntervals[sppawnerLevel - 1][attackLevelTier])
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

                if (energyAggroTimer >= energyAggroInterval[sppawnerLevel - 1])
                {
                    energyAggroValue = 0;
                    energyAggroTimer = 0;
                }
            }
        }

        if (takeDamageCheck)
        {
            guardianCallTimer += Time.deltaTime;
            if (guardianCallTimer >= guardianCallInterval)
            {
                takeDamageCheck = false;
            }
        }

        //if (waveState)
        //{
        //    waveTimer += Time.deltaTime;
        //    if (waveTimer >= waveInterval)
        //    {
        //        WaveStart();
        //    }
        //}
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
        sppawnerLevelData = levelData;
        sppawnerLevel = levelData.sppawnerLevel;
        biome = _biome;
        isInHostMap = isHostMap;
        spawnerGroupIndex = groupIndex;
        hp = structureData.MaxHp[levelData.sppawnerLevel - 1];
        maxHp = structureData.MaxHp[levelData.sppawnerLevel - 1];

        maxWeakSpawn = levelData.maxWeakSpawn;
        maxNormalSpawn = levelData.maxNormalSpawn;
        maxStrongSpawn = levelData.maxStrongSpawn;
        maxGuardianSpawn = levelData.maxGuardianSpawn;
        totalSpawnNum = maxWeakSpawn + maxNormalSpawn + maxStrongSpawn;

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
            SpawnMonster(3, 0, isInHostMap);
        }
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

    public GameObject SpawnMonster(int monserType, int monsterIndex, bool isInHostMap)
    {
        GameObject newMonster = null;
        if (monserType == 0)
        {
            newMonster = Instantiate(weakMonster[monsterIndex]);
            totalMonsterList.Add(newMonster.GetComponent<MonsterAi>());
            currentWeakSpawn++;
        }
        else if (monserType == 1)
        {
            newMonster = Instantiate(normalMonster[monsterIndex]);
            totalMonsterList.Add(newMonster.GetComponent<MonsterAi>());
            currentNormalSpawn++;
        }
        else if (monserType == 2)
        {
            newMonster = Instantiate(strongMonster[monsterIndex]);
            totalMonsterList.Add(newMonster.GetComponent<MonsterAi>());
            currentStrongSpawn++;
        }
        else if (monserType == 3)
        {
            newMonster = Instantiate(guardian);
            guardianList.Add(newMonster.GetComponent<GuardianAi>());
        }

        NetworkObject networkObject = newMonster.GetComponent<NetworkObject>();
        if (!networkObject.IsSpawned) networkObject.Spawn(true);

        newMonster.transform.SetParent(this.transform, false);
        //newMonster.transform.position = transform.position;

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
        if (!takeDamageCheck)
        {
            GuardianCall(attackObj);
            MonsterCall();
        }

        if (!dieCheck)
            TakeDamageServerRpc(damage);
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

        hp -= damage;
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

        MapGenerator.instance.ClearCorruption(transform.position, sppawnerLevel);
        icon.enabled = false;

        if (!IsServer)
            return;
        
        monsterSpawnerManager.AreaGroupRemove(this, sppawnerLevel, isInHostMap);
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
        MapGenerator.instance.ClearCorruption(transform.position, sppawnerLevel);
        icon.enabled = false;
        spawnerSearchColl.DieFunc();
        spawnerAwakeColl.DieFunc();
    }

    public void SearchObj(bool find)
    {
        if (!dieCheck)
            MonsterScriptSet(find, true);
        nearUserObjExist = find;
    }

    void GuardianCall(GameObject attackObj)
    {
        takeDamageCheck = true;

        foreach (GuardianAi guardian in guardianList)
        {
            guardian.SpawnerCallCheck(attackObj);
        }
    }

    void MonsterCall()
    {
        foreach (MonsterAi monster in totalMonsterList)
        {
            monster.SpawnerCallCheck();
        }
    }

    //public void WaveTeleport(Vector3 pos)
    //{
    //    waveState = true;
    //    waveTimer = 0;
    //    MonsterScriptSet(true, false);
    //    foreach (MonsterAi monster in totalMonsterList)
    //    {
    //        monster.WaveTeleport(pos, wavePos);
    //    }
    //}

    public void WaveStart()
    {
        waveState = false;
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
        if (hp < maxHp)
        {
            hpBar.fillAmount = hp / maxHp;
            unitCanvas.SetActive(true);
        }
        sppawnerLevel = spawnerSaveData.level;
        extraSpawnNum = spawnerSaveData.extraSpawnNum;
        nearUserObjExist = spawnerSaveData.nearUserObjExist;
        nearEnergyObjExist = spawnerSaveData.nearEnergyObjExist;
        isInHostMap = isHostMap;
        spawnerGroupIndex = groupIndex;
        safeCount = spawnerSaveData.safeCount;
        violentDay = spawnerSaveData.violentDay;
        sppawnerLevelData = levelData;
        sppawnerLevel = levelData.sppawnerLevel;
        maxWeakSpawn = levelData.maxWeakSpawn;
        maxNormalSpawn = levelData.maxNormalSpawn;
        maxStrongSpawn = levelData.maxStrongSpawn;
        maxGuardianSpawn = levelData.maxGuardianSpawn;
        totalSpawnNum = maxWeakSpawn + maxNormalSpawn + maxStrongSpawn;
        wavePos = _basePos;
        gameLodeSet = true;
    }

    public void GameStartWaveSet(float waveTimerSet)
    {
        waveState = true;
        waveTimer = waveTimerSet;
    }

    //public GameObject WaveWaitingMonsterSpawn(int monserType, int monsterIndex, bool isInHostMap)
    //{
    //    GameObject newMonster = null;
    //    if (monserType == 0)
    //    {
    //        newMonster = Instantiate(weakMonster[monsterIndex]);
    //        totalMonsterList.Add(newMonster.GetComponent<MonsterAi>());
    //        currentWeakSpawn++;
    //    }
    //    else if (monserType == 1)
    //    {
    //        newMonster = Instantiate(normalMonster[monsterIndex]);
    //        totalMonsterList.Add(newMonster.GetComponent<MonsterAi>());
    //        currentNormalSpawn++;
    //    }
    //    else if (monserType == 2)
    //    {
    //        newMonster = Instantiate(strongMonster[monsterIndex]);
    //        totalMonsterList.Add(newMonster.GetComponent<MonsterAi>());
    //        currentStrongSpawn++;
    //    }
    //    else if (monserType == 3)
    //    {
    //        newMonster = Instantiate(guardian);
    //        guardianList.Add(newMonster.GetComponent<GuardianAi>());
    //    }

    //    NetworkObject networkObject = newMonster.GetComponent<NetworkObject>();
    //    if (!networkObject.IsSpawned) networkObject.Spawn(true);

    //    newMonster.transform.SetParent(this.transform, false);

    //    MonsterAi monsterAi = newMonster.GetComponent<MonsterAi>();
    //    monsterAi.MonsterSpawnerSet(this, monserType);
    //    monsterAi.AStarSet(isInHostMap);

    //    if (IsServer)
    //        monsterAi.MonsterScriptSetServerRpc(true);

    //    return newMonster;
    //}

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

        //if (isWaveColonyCallCheck)
        //{
        //    battleBGM.WaveAddMonster(newMonster);
        //}
        //else
        //{
        //    battleBGM.ColonyCallAddMonster(newMonster, isInHostMap);
        //}

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

        float maxAggroValue = energyAggroMaxValue[sppawnerLevel - 1];

        if (energyAggroValue > maxAggroValue)
        {
            restTime = true;
            attackLevelTier = (int)(energyAggroTimer / (energyAggroInterval[sppawnerLevel - 1] / restAggroIntervals[sppawnerLevel - 1].Length));
            energyAggroValue = 0;
            checkAggroTimer = 0;
            energyAggroTimer = 0;
            Debug.Log("lv : " +attackLevelTier);
            EnergyUseStrAttack(attackLevelTier);
        }
    }

    public void EnergyUseStrAttack(int attackLevel)
    {
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

        if (totalWeight == 0f)
        {
            Debug.LogWarning("No structures with positive energy usage for attack.");
            return;
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

    //public void GlobalWaveState(bool wave)
    //{
    //    globalWave = wave;
    //}

    public SpawnerSaveData SaveData()
    {
        SpawnerSaveData data = new SpawnerSaveData();

        data.hp = hp;
        data.level = sppawnerLevel;
        data.wavePos = Vector3Extensions.FromVector3(wavePos);
        data.spawnerPos = Vector3Extensions.FromVector3(transform.position);
        data.extraSpawnNum = extraSpawnNum;
        data.waveState = waveState;
        data.waveTimer = waveTimer;
        data.dieCheck = dieCheck;
        data.spawnerGroupIndex = spawnerGroupIndex;
        data.safeCount = safeCount;
        data.violentDay = violentDay;
        data.violentCollSize = spawnerSearchColl.violentCollSize;
        data.nearUserObjExist = nearUserObjExist;
        data.nearEnergyObjExist = nearEnergyObjExist;
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
            MapGenerator.instance.SetCorruption(transform.position, sppawnerLevel);
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
    }

    public void SafeCountDown()
    {
        safeCount--;
        if (safeCount < 0)
        {
            safeCount = 0;
        }
    }
}
