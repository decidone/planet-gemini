using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

// UTF-8 설정
public class MonsterSpawner : NetworkBehaviour
{
    public enum MonsterType
    {
        Weak = 0,
        Normal = 1,
        Strong = 2,
        Guardian = 3
    }

    struct SpawnRequest
    {
        public MonsterType type;
        public int count;

        public SpawnRequest(MonsterType type, int count)
        {
            this.type = type;
            this.count = count;
        }
    }

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

    int totalSpawnNum;   // 가딘언을 제외한 최대 소환 수    
    public int spawnNum; // 일반 소환해야 하는 몬스터 수

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

    MonsterSpawnerManager monsterSpawnerManager;

    [SerializeField]
    GameObject awakeColl;
    SpawnerAwake spawnerAwakeColl;
    float distanceToPortal; // 포탈까지의 거리

    float detectionRange;   // 포탈 인식 거리
    [HideInInspector]
    public int detectionCount;  // 포탈 인식 카운트

    float distRangeExpansion = 0.15f;    // 인식 거리 확대 0.15이면 15% 확대
    float distRangeReduction = 0.7f;    // 인식 거리 축소 0.6이면 60% 축소
    float maxDistRange = 1.4f;       // 최대 인식 거리 1.4면 140%
    [HideInInspector]
    public bool isReachedPortal;    //포탈까지 도달했는지

    float[] baseDetectRange = new float[8] { 10f, 11f, 12f, 13f, 14f, 15f, 16f, 17f };  // 레벨별 기본 인식 거리

    public bool isInHostMap;
    public Tilemap corruptionTilemap;

    //[HideInInspector]
    public bool violentDay;
    public float violentCollSize;

    int ragePhase = 0;
    int maxRagePhase = 5;
    int[] ragePhaseSpawnCount = new int[5] { 2, 4, 6, 8, 10 };

    int waveLevel;

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
        //spawnerSearchColl = searchColl.GetComponent<SpawnerSearchColl>();
        spawnerAwakeColl = awakeColl.GetComponent<SpawnerAwake>();
    }

    void Start()
    {
        if (!IsServer)
        {
            //searchColl.SetActive(false);
            awakeColl.SetActive(false);
        }
        //spawnerSearchColl.violentCollSize = violentCollSize;
        SpriteSet();

        if (isInHostMap)
        {
            distanceToPortal = Vector3.Distance(transform.position, GameManager.instance.portal[0].transform.position);
        }
        else
        {
            distanceToPortal = Vector3.Distance(transform.position, GameManager.instance.portal[1].transform.position);
        }

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

        if (attackObj)
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
            if (levelPhase)
                StartCoroutine(ExtraMonsterSpawnCoroutine(ragePhase, attackObj));
            else
                TriggerRage(ragePhase, attackObj);
        }
    }

    bool LevelPhaseCheck(int ragePhase)
    {
        bool pashe = false;

        if (ragePhase == 1)
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

        if (violentDay)
        {
            violentDay = false;
            monsterSpawnerManager.WavePointOff();
        }
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

        MapGenerator.instance.ClearCorruption(this, spawnerLevel);
        icon.enabled = false;

        if (!IsServer)
            return;

        monsterSpawnerManager.AreaGroupRemove(this, spawnerLevel, isInHostMap);
        Overall.instance.OverallCount(0);

        int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(ItemList.instance.itemDic["VoidShard"]);
        GeminiNetworkManager.instance.ItemSpawnServerRpc(itemIndex, 1, transform.position);

        //spawnerSearchColl.DieFunc();
        spawnerAwakeColl.DieFunc();
    }

    public void DieFuncLoad()
    {
        unitSprite.color = new Color(1f, 1f, 1f, 0f);
        unitCanvas.SetActive(false);
        capsuleCollider2D.enabled = false;
        //MapGenerator.instance.ClearCorruption(this, spawnerLevel);
        icon.enabled = false;
        //spawnerSearchColl.DieFunc();
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
        detectionRange = spawnerSaveData.detectionRange;
        detectionCount = spawnerSaveData.detectionCount;
        violentDay = spawnerSaveData.violentDay;
        waveLevel = spawnerSaveData.waveLevel;
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
        data.violentDay = violentDay;
        data.detectionRange = detectionRange;
        data.detectionCount = detectionCount;
        data.nearUserObjExist = nearUserObjExist;
        data.nearEnergyObjExist = nearEnergyObjExist;
        data.detectionRange = detectionRange;
        data.detectionCount = detectionCount;
        data.ragePhase = ragePhase;
        data.waveLevel = waveLevel;
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
            MapGenerator.instance.SetCorruption(this, spawnerLevel);
    }

    public void ViolentDaySet(int waveLevelSet)
    {
        violentDay = true;
        waveLevel = waveLevelSet;
        detectionCount = 0;
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

    public void SetTilemap(Tilemap tilemap)
    {
        corruptionTilemap = tilemap;
    }

    public void DetectionRangeExpansion()
    {
        float energyRate = Mathf.RoundToInt(GameManager.instance.EnergyUseAmount() / 3000) / 50;
        float ranCount = Random.Range(-0.05f, 0.05f);
        ranCount = Mathf.Round((ranCount + energyRate) * 100f) / 100f; // 소수점 둘째 자리까지
        float detetionExpansionPersent = 1 + distRangeExpansion + ranCount; // 인식 거리 확대 퍼센트
        if (detetionExpansionPersent > maxDistRange)
        {
            detetionExpansionPersent = maxDistRange;
        }

        detectionRange = (detectionRange + baseDetectRange[spawnerLevel - 1]) * (detetionExpansionPersent);

        if (detectionRange >= distanceToPortal)
        {
            detectionRange = distanceToPortal;
            isReachedPortal = true;
            detectionCount++;
        }
    }

    public void DetectionRangeReduction()
    {
        detectionRange *= (1 - distRangeReduction);

        if (detectionRange < distanceToPortal)
        {
            isReachedPortal = false;
        }
    }

    public void DetectionRangeReset()
    {
        detectionRange = 0;
        isReachedPortal = false;
    }

    public void ExecuteSpawn(int weak, int normal, int strong)
    {
        var requests = new List<SpawnRequest>
        {
            new SpawnRequest(MonsterType.Weak, weak),
            new SpawnRequest(MonsterType.Normal, normal),
            new SpawnRequest(MonsterType.Strong, strong)
        };

        Debug.Log("Wave Spawn Amount : weak " + weak + " : normal " + normal + " : strong " + strong);

        StartCoroutine(WaveMonsterSpawn(requests));
    }

    IEnumerator WaveMonsterSpawn(List<SpawnRequest> requests)
    {
        (float, float) monsterMultiplier = (GameManager.instance.hpMultiplier, GameManager.instance.atkMultiplier);
        List<GameObject> monsters = new();
        List<MonsterAi> monstersAi = new();

        foreach (var req in requests)
        {
            for (int i = 0; i < req.count; i++)
            {
                GameObject monster = RandomWaveMonsterSpawn((int)req.type);
                monsters.Add(monster);

                if (monster.TryGetComponent(out MonsterAi ai))
                {
                    monstersAi.Add(ai);
                    ai.WaveStart(wavePos);
                    ai.WaveStatusSet(monsterMultiplier.Item1, monsterMultiplier.Item2);
                }

                yield return null;
            }
        }

        monsterSpawnerManager.WaveAddMonster(monsters);
        waveMonsterList.AddRange(monstersAi);
    }

    GameObject RandomWaveMonsterSpawn(int monsterType)
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
        GameObject monster = SpawnWaveMonster(monsterType, randomMob, isInHostMap);

        return monster;
    }

    public GameObject SpawnWaveMonster(int monserType, int monsterIndex, bool isInHostMap)
    {
        GameObject newMonster = null;
        if (monserType == 0)
        {
            newMonster = Instantiate(weakMonster[monsterIndex], transform.position, Quaternion.identity, this.transform);
        }
        else if (monserType == 1)
        {
            newMonster = Instantiate(normalMonster[monsterIndex], transform.position, Quaternion.identity, this.transform);
        }
        else if (monserType == 2)
        {
            newMonster = Instantiate(strongMonster[monsterIndex], transform.position, Quaternion.identity, this.transform);
        }
        else if (monserType == 3)
        {
            newMonster = Instantiate(guardian, transform.position, Quaternion.identity, this.transform);
        }

        NetworkObject networkObject = newMonster.GetComponent<NetworkObject>();
        if (!networkObject.IsSpawned) networkObject.Spawn(true);

        newMonster.transform.parent = this.transform;

        MonsterAi monsterAi = newMonster.GetComponent<MonsterAi>();
        monsterAi.MonsterSpawnerSet(this, monserType);
        monsterAi.AStarSet(isInHostMap);
        monsterAi.MonsterScriptSetServerRpc(true);        

        return newMonster;
    }

    private void OnDrawGizmos()
    {
        if (transform == null || GameManager.instance == null) return;

        Vector3 portal;

        if (isInHostMap)
            portal = GameManager.instance.portal[0].transform.position;
        else
            portal = GameManager.instance.portal[1].transform.position;

        // 방향 정규화 후 길이 반영
        Vector3 direction = (portal - transform.position).normalized * detectionRange;

        // 끝점 계산
        Vector3 endPoint = transform.position + direction;

        // 색상 설정 (선택사항)
        if (detectionRange < distanceToPortal)
            Gizmos.color = Color.green;
        else
            Gizmos.color = Color.red;

        // 선 그리기
        Gizmos.DrawLine(transform.position, endPoint);

        // 끝점에 구체 찍기 (시각화용)
        Gizmos.DrawSphere(endPoint, 0.05f);
    }

}
