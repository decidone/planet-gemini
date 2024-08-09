using System.Collections;
using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Pathfinding;

// UTF-8 설정
public enum AIState   // 몬스터 상태 관리
{
    AI_Idle,
    AI_Move,
    AI_Patrol,
    AI_SpawnerCall,
    AI_NormalTrace,
    AI_ReturnPos,
    AI_Attack,
    AI_Die
}

public enum AttackState
{
    Waiting,
    AttackStart,
    Attacking,
    AttackDelay,
    AttackEnd,
}

public class UnitCommonAi : NetworkBehaviour
{
    [SerializeField]
    public UnitCommonData unitCommonData;
    protected UnitCommonData UnitCommonData { set { unitCommonData = value; } }

    protected Transform tr;
    protected Rigidbody2D rg;
    protected Seeker seeker;
    protected Coroutine checkPathCoroutine;             // 실행 중인 코루틴을 저장하는 변수
    protected int currentWaypointIndex;                 // 현재 이동 중인 경로 점 인덱스
    protected Vector3 targetPosition;
    protected Vector2 lastMoveDirection = Vector2.zero; // 마지막으로 이동한 방향
    protected Vector3 direction = Vector3.zero;
    protected Vector3 targetVec = Vector3.zero;
    protected List<Vector3> movePath = new List<Vector3>();

    protected float searchInterval;
    protected float searchTimer;
    public GameObject aggroTarget;
    protected float tarDisCheckTime;
    protected float tarDisCheckInterval;                // 0.3초 간격으로 몬스터 거리 체크
    protected float targetDist;
    public List<GameObject> targetList = new List<GameObject>();

    protected Vector3 patrolPos;

    [SerializeField]
    protected Animator animator;

    protected CapsuleCollider2D capsule2D;

    public string unitName;

    public SpriteRenderer unitSprite;
    public GameObject unitCanvas;
    public Image hpBar;
    public float hp;
    public float maxHp;
    public bool isInHostMap;

    public int unitIndex;

    public bool isFlip;
    protected bool isDelayAfterAttackCoroutine = false;

    public AIState aIState;
    public AttackState attackState;

    protected bool dieCheck = false;

    public SoundManager soundManager;
    protected BattleBGMCtrl battleBGM;

    protected NetworkObjectPool networkObjectPool;

    public delegate void OnHpChanged();
    public OnHpChanged onHpChangedCallback;

    private void Awake()
    {
        tr = GetComponent<Transform>();
        rg = GetComponent<Rigidbody2D>();
        capsule2D = GetComponent<CapsuleCollider2D>();
        seeker = GetComponent<Seeker>();
        animator = GetComponent<Animator>();

        hp = unitCommonData.MaxHp;
        maxHp = unitCommonData.MaxHp;
        hpBar.fillAmount = hp / maxHp;
        unitCanvas.SetActive(false);

        isFlip = unitSprite.flipX;
        searchInterval = 0.3f;
        tarDisCheckInterval = 0.3f;
        patrolPos = Vector3.zero;
        unitName = unitCommonData.UnitName;
        //hp = 100.0f;
        aIState = AIState.AI_Idle;
        attackState = AttackState.Waiting;
    }

    protected virtual void Start()
    {
        soundManager = SoundManager.instance;
        battleBGM = BattleBGMCtrl.instance;
        networkObjectPool = NetworkObjectPool.Singleton;
    }

    protected virtual void FixedUpdate()
    {
        if (!IsServer)
            return;

        if (aIState != AIState.AI_Die)
            UnitAiCtrl();
    }

    protected virtual void Update()
    {
        if (!IsServer)
            return;

        if (aIState != AIState.AI_Die)
        {
            searchTimer += Time.deltaTime;

            if (searchTimer >= searchInterval)
            {
                SearchObjectsInRange();
                searchTimer = 0f; // 탐색 후 타이머 초기화
            }

            if (targetList.Count > 0)
            {
                tarDisCheckTime += Time.deltaTime;
                if (tarDisCheckTime > tarDisCheckInterval)
                {
                    tarDisCheckTime = 0f;
                    AttackTargetCheck();
                    RemoveObjectsOutOfRange();
                }
                AttackTargetDisCheck();
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
        ClientConnectSyncClientRpc(hp);
    }
    
    [ClientRpc]
    public virtual void ClientConnectSyncClientRpc(float syncHp)
    {
        hp = syncHp;

        if (hp < maxHp)
        {
            hpBar.fillAmount = hp / maxHp;
            unitCanvas.SetActive(true);
        }
    }

    protected virtual void UnitAiCtrl() { }
    protected virtual void SearchObjectsInRange() { }
    protected virtual void AttackTargetCheck() { }

    protected void RemoveObjectsOutOfRange()
    {
        for (int i = targetList.Count - 1; i >= 0; i--)
        {
            if (!targetList[i])
                targetList.RemoveAt(i);
            else
            {
                GameObject target = targetList[i];
                if (Vector2.Distance(tr.position, target.transform.position) > unitCommonData.ColliderRadius)
                {
                    targetList.RemoveAt(i);
                }
            }
        }
    }

    protected virtual void AttackTargetDisCheck()
    {
        if (aggroTarget)
        {
            targetVec = (new Vector3(aggroTarget.transform.position.x, aggroTarget.transform.position.y, 0) - tr.position).normalized;
            targetDist = Vector3.Distance(tr.position, aggroTarget.transform.position);
        }
    }

    protected virtual IEnumerator CheckPath(Vector3 targetPos, string moveFunc) { yield return null; }
    protected virtual void AnimSetFloat(Vector3 direction, bool isNotLast) { }
    protected virtual void NormalTrace() { }

    protected void AttackCheck()
    {
        if (targetDist == 0)
            return;
        else if (targetDist > unitCommonData.AttackDist)  // 공격 범위 밖으로 나갈 때
        {
            animator.SetBool("isAttack", false);
            aIState = AIState.AI_NormalTrace;
            attackState = AttackState.Waiting;
        }
        else if (targetDist <= unitCommonData.AttackDist)  // 공격 범위 내로 들어왔을 때        
        {
            aIState = AIState.AI_Attack;
            attackState = AttackState.AttackStart;
        }
    }

    protected void Attack()
    {
        AnimSetFloat(targetVec, true);

        if (!isDelayAfterAttackCoroutine)
        {
            attackState = AttackState.AttackDelay;
            StartCoroutine(DelayAfterAttack(unitCommonData.AttDelayTime)); // 1.5초 후 딜레이 적용
        }
    }
    protected IEnumerator DelayAfterAttack(float delayTime)
    {
        isDelayAfterAttackCoroutine = true;
        AttackStart();
        //SwBodyType(false);

        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 1.0f)
        {
            yield return null;
        }

        yield return new WaitForSeconds(delayTime);

        isDelayAfterAttackCoroutine = false;
    }

    protected virtual void AttackStart() { }

    protected virtual void AttackEnd()
    {
        animator.SetBool("isAttack", false);
        animator.SetBool("isMove", false);
        if(IsServer)
            AnimSetFloat(targetVec, false);
        attackState = AttackState.Waiting;        
    }

    public virtual void TakeDamage(float damage)
    {
        if (!dieCheck)
        {
            TakeDamageServerRpc(damage);
        }
    }

    [ServerRpc]
    public virtual void TakeDamageServerRpc(float damage)
    {
        TakeDamageClientRpc(damage);
    }

    [ClientRpc]
    public virtual void TakeDamageClientRpc(float damage)
    {
        //if (hp <= 0f)
        //    return;
        if (!unitCanvas.activeSelf)
            unitCanvas.SetActive(true);

        float reducedDamage = Mathf.Max(damage - unitCommonData.Defense, 5);

        hp -= reducedDamage;
        if (hp < 0f)
            hp = 0f;
        onHpChangedCallback?.Invoke();
        hpBar.fillAmount = hp / maxHp;

        if (IsServer && hp <= 0f && !dieCheck)
        {
            aIState = AIState.AI_Die;
            hp = 0f;
            dieCheck = true;
            DieFuncServerRpc();
        }
    }

    [ServerRpc]
    protected virtual void DieFuncServerRpc()
    {
        DieFuncClientRpc();
    }

    [ClientRpc]
    protected virtual void DieFuncClientRpc()
    {
        unitSprite.color = new Color(1f, 1f, 1f, 0f);
        unitCanvas.SetActive(false);

        capsule2D.enabled = false;
    }

    public virtual void RemoveTarget(GameObject target) 
    {            
        if (targetList.Contains(target))
        {
            targetList.Remove(target);
        }
        if (targetList.Count == 0)
        {
            aggroTarget = null;
        }
    }

    public void AStarSet(bool isHostMap)
    {
        GraphMask mask;
        if (isHostMap)
            mask = GraphMask.FromGraphName("Map1");
        else
            mask = GraphMask.FromGraphName("Map2");

        isInHostMap = isHostMap;
        seeker.graphMask = mask;
    }

    public virtual void GameStartSet(UnitSaveData unitSaveData)
    {
        unitIndex = unitSaveData.unitIndex;
        hp = unitSaveData.hp;

        if (hp < maxHp) 
        {
            hpBar.fillAmount = hp / maxHp;
            unitCanvas.SetActive(true);
        }
    }

    public virtual UnitSaveData SaveData()
    {
        UnitSaveData data = new UnitSaveData();

        data.hp = hp;
        data.pos = Vector3Extensions.FromVector3(transform.position);

        return data;
    }
}
