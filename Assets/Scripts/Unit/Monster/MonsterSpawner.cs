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
    int extraSpawnNum;  // 임시 저장 소환 수

    // 현재 소환 개수 정보
    public int currentWeakSpawn;
    public int currentNormalSpawn;
    public int currentStrongSpawn;

    public List<MonsterAi> totalMonsterList = new List<MonsterAi>(); // 가딘언을 제외한 몬스터 리스트
    public List<GuardianAi> guardianList = new List<GuardianAi>();
    public List<MonsterAi> waveMonsterList = new List<MonsterAi>();
    // 가디언은 초기에 배치하고 그 이후로는 관리 안함

    public int areaLevel;
    AreaLevelData areaLevelData;
    string biome;

    float spawnInterval;
    float spawnTimer;

    public bool nearUserObjExist;

    public SpriteRenderer unitSprite;
    public GameObject unitCanvas;
    public Image hpBar;
    public float hp;
    public float maxHp;
    public bool dieCheck = false;
    protected CapsuleCollider2D capsuleCollider2D;
    bool extraSpawn;
    bool takeDamageCheck;
    float guardianCallInterval;
    float guardianCallTimer;

    Vector3 wavePos;
    float waveInterval = 40;
    public float waveTimer;
    bool waveState = false;

    BattleBGMCtrl battleBGM;
    MonsterSpawnerManager monsterSpawnerManager;

    [SerializeField]
    GameObject searchColl;
    SpawnerSearchColl spawnerSearchColl;
    [SerializeField]
    float energyAggroValue;
    [SerializeField]
    float energyAggroInterval;
    float energyAggroTimer;
    bool globalWave;

    public bool isInHostMap;
    bool gameLodeSet = false;

    public delegate void OnHpChanged();
    public OnHpChanged onHpChangedCallback;

    void Awake()
    {
        monsterSpawn = MonsterSpawnerManager.instance;
        battleBGM = BattleBGMCtrl.instance;
        monsterSpawnerManager = MonsterSpawnerManager.instance;
        weakMonster = GeminiNetworkManager.instance.unitListSO.weakMonsterList;
        normalMonster = GeminiNetworkManager.instance.unitListSO.normalMonsterList;
        strongMonster = GeminiNetworkManager.instance.unitListSO.strongMonsterList;
        guardian = GeminiNetworkManager.instance.unitListSO.guardian[0];
    }

    void Start()
    {
        capsuleCollider2D = GetComponent<CapsuleCollider2D>();
        spawnInterval = 20;
        guardianCallInterval = 2;
        maxExtraSpawn = 10;
        extraSpawn = false;
        if (!IsServer)
            searchColl.SetActive(false);
        if(!gameLodeSet)
            InitializeMonsterSpawn();
        spawnerSearchColl = searchColl.GetComponent<SpawnerSearchColl>();
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (areaLevel == 1)
        {
            sprite.sprite = monsterSpawnerManager.spawnerSprite[0];
            capsuleCollider2D.size = new Vector2(2.6f, 0.9f);
        }
        else if (areaLevel == 2 || areaLevel == 3)
        {
            sprite.sprite = monsterSpawnerManager.spawnerSprite[1];
            capsuleCollider2D.size = new Vector2(3.5f, 1f);
        }
        else if (areaLevel == 4 || areaLevel == 5)
        {
            sprite.sprite = monsterSpawnerManager.spawnerSprite[2];
            capsuleCollider2D.size = new Vector2(5f, 1.8f);
        }
    }

    void Update()
    {
        if (!IsServer || dieCheck)
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

        if (!globalWave && nearUserObjExist && spawnerSearchColl.structures.Count > 0)
        {
            energyAggroTimer += Time.deltaTime;
            if (energyAggroTimer >= energyAggroInterval)
            {
                StructuresEnergyCheck();
                energyAggroTimer = 0;
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

        if (waveState)
        {
            waveTimer += Time.deltaTime;
            if (waveTimer >= waveInterval)
            {
                WaveStart();
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

    public void SpawnerSetting(AreaLevelData levelData, string _biome, Vector3 _basePos, bool isHostMap)
    {
        areaLevelData = levelData;
        areaLevel = levelData.areaLevel;
        biome = _biome;
        isInHostMap = isHostMap;
        hp = structureData.MaxHp[levelData.areaLevel -1];
        maxHp = structureData.MaxHp[levelData.areaLevel -1];

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
            SpawnMonster(3, 0, isInHostMap);
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
        if(!networkObject.IsSpawned) networkObject.Spawn(true);

        newMonster.transform.SetParent(this.transform, false);

        Vector3 setPos = this.transform.position;
        newMonster.transform.position = setPos;

        MonsterAi monsterAi = newMonster.GetComponent<MonsterAi>();
        monsterAi.MonsterSpawnerSet(this, monserType);
        monsterAi.AStarSet(isInHostMap);
        if (!nearUserObjExist && !dieCheck)
        {
            if(IsServer)
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

        if(!dieCheck)
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
        monsterSpawnerManager.AreaGroupRemove(this, areaLevel, isInHostMap);
        capsuleCollider2D.enabled = false;

        if (InfoUI.instance.spawner == this)
            InfoUI.instance.SetDefault();

        GameObject InfoObj = GetComponentInChildren<InfoInteract>().gameObject;
        InfoObj.SetActive(false);

        MapGenerator.instance.ClearCorruption(transform.position, areaLevel);
        icon.enabled = false;

        if (!IsServer)
            return;

        Overall.instance.OverallCount(0);
        spawnerSearchColl.DieFunc();
    }

    public void SearchObj(bool find)
    {
        if(!dieCheck)
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

    public void WaveTeleport(Vector3 pos)
    {
        waveState = true;
        waveTimer = 0;
        MonsterScriptSet(true, false);
        foreach (MonsterAi monster in totalMonsterList)
        {
            monster.WaveTeleport(pos, wavePos);
        }
    }

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

        battleBGM.WaveAddMonster(monsters);

        waveMonsterList.AddRange(totalMonsterList);

        totalMonsterList.Clear();
        currentWeakSpawn = 0;
        currentNormalSpawn = 0;
        currentStrongSpawn = 0;
    }

    public void ColonyCall(EnergyColony colony)
    {
        if (!IsServer)
            return;

        MonsterScriptSet(true, false);
        List<GameObject> monsters = new List<GameObject>();
        foreach (MonsterAi monster in totalMonsterList)
        {
            monster.ColonyAttackStart(colony.transform.position);
            monsters.Add(monster.gameObject);
        }

        battleBGM.ColonyCallAddMonster(monsters, isInHostMap);

        waveMonsterList.AddRange(totalMonsterList);

        totalMonsterList.Clear();
        currentWeakSpawn = 0;
        currentNormalSpawn = 0;
        currentStrongSpawn = 0;

        WarningWindow.instance.WarningTextSet("Attack detected on", isInHostMap);
    }

    public void GameStartSet(SpawnerSaveData spawnerSaveData, AreaLevelData levelData, Vector3 _basePos, bool isHostMap)
    {
        hp = spawnerSaveData.hp;
        areaLevel = spawnerSaveData.level;
        extraSpawnNum = spawnerSaveData.extraSpawnNum;
        isInHostMap = isHostMap;

        areaLevelData = levelData;
        areaLevel = levelData.areaLevel;
        maxWeakSpawn = levelData.maxWeakSpawn;
        maxNormalSpawn = levelData.maxNormalSpawn;
        maxStrongSpawn = levelData.maxStrongSpawn;
        maxGuardianSpawn = levelData.maxGuardianSpawn;

        wavePos = _basePos;
        gameLodeSet = true;
    }

    public void GameStartWaveSet(float waveTimerSet)
    {
        waveState = true;
        waveTimer = waveTimerSet;
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
            monsterAi.MonsterScriptSetServerRpc(true);
  
        if (isWaveColonyCallCheck)
        {
            battleBGM.WaveAddMonster(newMonster);
        }
        else
        {
            battleBGM.ColonyCallAddMonster(newMonster, isInHostMap);
        }

        return newMonster;
    }

    void StructuresEnergyCheck()
    {
        List<Structure> structures = new List<Structure>(spawnerSearchColl.structures);

        foreach (Structure structure in structures)
        {
            if (structure.energyUse && structure.conn && structure.conn.group != null)
            {
                Debug.Log(structure.conn.group.consumption);
                if (structure.conn.group.consumption > energyAggroValue)
                {
                    Debug.Log("WaveStartSet");
                }
            }
            else if (structure.isEnergyStr && structure.GetComponentInChildren<EnergyGroupConnector>())
            {
                EnergyGroupConnector energy = structure.GetComponentInChildren<EnergyGroupConnector>(); 
                if (energy.group != null && energy.group.consumption > energyAggroValue)
                {
                    Debug.Log("WaveStartSet");
                }
            }
        }
    }

    public void GlobalWaveState(bool wave)
    {
        Debug.Log("GlobalWaveState" + wave);
        globalWave = wave;
    }

    public SpawnerSaveData SaveData()
    {
        SpawnerSaveData data = new SpawnerSaveData();

        data.hp = hp;
        data.level = areaLevel;
        data.wavePos = Vector3Extensions.FromVector3(wavePos);
        data.spawnerPos = Vector3Extensions.FromVector3(transform.position);
        data.extraSpawnNum = extraSpawnNum;
        data.waveState = waveState;
        data.waveTimer = waveTimer;
        data.dieCheck = dieCheck;
        Debug.Log(waveTimer + " : " + waveState);
         
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
            MapGenerator.instance.SetCorruption(transform.position, areaLevel);
    }
}
