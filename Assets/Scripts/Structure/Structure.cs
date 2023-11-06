using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Pool;
using UnityEngine.Rendering;
using System;

// UTF-8 설정
public class Structure : MonoBehaviour
{
    // 건물 공용 스크립트
    // Update처럼 함수 호출하는 부분은 다 하위 클래스에 넣을 것
    // 연결된 구조물 확인 방법 1. 콜라이더, 2. 맵에서 인접 타일 체크
    [SerializeField]
    public StructureData structureData;
    protected StructureData StructureData { set { structureData = value; } }

    [HideInInspector]
    public bool isFull = false;

    [HideInInspector]
    public int dirNum = 0;
    [HideInInspector]
    public int dirCount = 0;
    public int level = 0;
    public string buildName;

    [HideInInspector]
    public int height;
    [HideInInspector]
    public int width;
    [HideInInspector]
    public bool sizeOneByOne;

    [HideInInspector]
    public bool isPreBuilding = false;
    [HideInInspector]
    public bool isSetBuildingOk = false;    

    protected bool removeState = false;

    [SerializeField]
    protected GameObject unitCanvas;

    [SerializeField]
    protected Image hpBar;
    protected float hp = 200.0f;
    [HideInInspector]
    public bool isRuin = false;

    [HideInInspector]
    public bool isRepair = false;
    [SerializeField]
    protected Image repairBar;
    protected float repairGauge = 0.0f;

    [HideInInspector]
    public List<Item> itemList = new List<Item>();
    [HideInInspector]
    public List<ItemProps> itemObjList = new List<ItemProps>();

    [HideInInspector]
    public bool canBuilding = true;
    protected List<GameObject> buildingPosUnit = new List<GameObject>();

    public GameObject[] nearObj = new GameObject[4];
    public Vector2[] checkPos = new Vector2[4];
    public bool checkObj = true;

    protected Vector2[] startTransform;
    protected Vector3[] directions;
    protected int[] indices;

    protected Inventory playerInven = null;

    protected bool itemGetDelay = false;
    protected bool itemSetDelay = false;

    //[HideInInspector]
    public List<GameObject> inObj = new List<GameObject>();
    //[HideInInspector]
    public List<GameObject> outObj = new List<GameObject>();
    [HideInInspector]
    public List<GameObject> outSameList = new List<GameObject>();

    protected int getItemIndex = 0;
    protected int sendItemIndex = 0;

    protected Coroutine setFacDelayCoroutine; // 실행 중인 코루틴을 저장하는 변수

    [SerializeField]
    protected Sprite[] modelNum;
    protected SpriteRenderer setModel;

    public ItemProps spawnItem;

    //[HideInInspector]
    public List<GameObject> monsterList = new List<GameObject>();

    [HideInInspector]
    public Collider2D col;
    [HideInInspector]
    public RepairTower repairTower;

    public bool isStorageBuild;
    public bool isMainSource;
    public bool isTempBuild = false;

    public virtual bool CheckOutItemNum()  { return new bool(); }

    public void BuildingSetting(int _level, int _height, int _width, int _dirCount)
    {
        isPreBuilding = true;
        ColliderTriggerOnOff(true);
        level = _level - 1;
        height = _height;
        width = _width;
        dirCount = _dirCount;
    }

    protected virtual void SetDirNum()
    {
        setModel.sprite = modelNum[dirNum];
        CheckPos();
    }

    // 건물의 방향 설정
    protected virtual void CheckPos()
    {
        if (width == 1 && height == 1)
        {
            sizeOneByOne = true;
            nearObj = new GameObject[4];
            Vector2[] dirs = { Vector2.up, Vector2.right, Vector2.down, Vector2.left };

            for (int i = 0; i < 4; i++)
            {
                checkPos[i] = dirs[(dirNum + i) % 4];
            }
        }
        else if (width == 2 && height == 1)
        {
            sizeOneByOne = false;
            nearObj = new GameObject[6];
            indices = new int[] { 1, 0, 0, 0, 1, 1 };
            startTransform = new Vector2[] { new Vector2(0.5f, -1f), new Vector2(-0.5f, -1f) };
            directions = new Vector3[] { transform.up, transform.right, -transform.up, -transform.right };

        }
        else if (width == 2 && height == 2)
        {
            sizeOneByOne = false;
            nearObj = new GameObject[8];
            indices = new int[] { 3, 0, 0, 1, 1, 2, 2, 3 };
            startTransform = new Vector2[] { new Vector2(0.5f, 0.5f), new Vector2(0.5f, -0.5f), new Vector2(-0.5f, -0.5f), new Vector2(-0.5f, 0.5f) };
            directions = new Vector3[] { transform.up, transform.right, -transform.up, -transform.right };
        }
    }

    // 1x1 사이즈 근처 오브젝트 찻는 위치(상하좌우) 설정
    protected virtual void CheckNearObj(Vector2 direction, int index, Action<GameObject> callback)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, 1f);

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hitCollider = hits[i].collider;

            if(GetComponent<LogisticsCtrl>() || (GetComponent<Production>() && !GetComponent<FluidFactoryCtrl>()))
            {
                if(hitCollider.GetComponent<FluidFactoryCtrl>() && !hitCollider.GetComponent<Refinery>())
                {
                    continue;
                }
            }
            else if (GetComponent<FluidFactoryCtrl>() && !GetComponent<Refinery>())
            {
                if (hitCollider.GetComponent<LogisticsCtrl>())
                {
                    continue;
                }
            }

            if (hitCollider.CompareTag("Factory") && hitCollider.GetComponent<Structure>().isSetBuildingOk &&
                hits[i].collider.gameObject != this.gameObject)
            {
                nearObj[index] = hits[i].collider.gameObject;
                callback(hitCollider.gameObject);
                break;
            }
        }
    }

    // 2x2 사이즈 근처 오브젝트 찻는 위치(상하좌우) 설정
    protected virtual void CheckNearObj(Vector3 startVec, Vector3 endVec, int index, Action<GameObject> callback) 
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(this.transform.position + startVec, endVec, 1f);

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hitCollider = hits[i].collider;
            if (hitCollider.CompareTag("Factory") && hitCollider.GetComponent<Structure>().isSetBuildingOk &&
                hits[i].collider.gameObject != this.gameObject)
            {
                nearObj[index] = hits[i].collider.gameObject;
                callback(hitCollider.gameObject);
                break;
            }
        }
    }

    public virtual void SetBuild()
    {
        unitCanvas.SetActive(true);
        hpBar.enabled = false;
        repairBar.enabled = true;
        repairGauge = 0;
        repairBar.fillAmount = repairGauge / structureData.MaxBuildingGauge;
        isSetBuildingOk = true;
    }
    // 건물 설치 기능

    public virtual void OnFactoryItem(ItemProps itemProps) 
    {
        itemProps.Pool.Release(itemProps.gameObject);
    }

    public virtual void OnFactoryItem(Item item) { }
    public virtual void ItemNumCheck() { }

    protected virtual IEnumerator UnderBeltConnectCheck(GameObject game)
    {
        yield return new WaitForSeconds(0.1f);

        if (game.TryGetComponent(out GetUnderBeltCtrl getUnder))
        {
            if (!getUnder.outObj.Contains(this.gameObject))
            {
                inObj.Remove(game);
            }
            if (!getUnder.inObj.Contains(this.gameObject))
            {
                outObj.Remove(game);
                outSameList.Remove(game);
            }
        }
        else if (game.TryGetComponent(out SendUnderBeltCtrl sendUnder))
        {
            if (!sendUnder.inObj.Contains(this.gameObject))
            {
                outObj.Remove(game); 
                outSameList.Remove(game);
            }
            if (!sendUnder.outObj.Contains(this.gameObject))
            {
                inObj.Remove(game);
            }
        }
        checkObj = true;
    }

    protected virtual void GetItem()
    {
        itemGetDelay = true;
        if (getItemIndex > inObj.Count)
        {
            getItemIndex = 0;
            return;
        }
        else if (inObj[getItemIndex] == null)
        {
            getItemIndex = 0;
            return;
        }         
        else if (inObj[getItemIndex].TryGetComponent(out BeltCtrl belt) && belt.isItemStop)
        {
            if (belt.itemObjList.Count > 0)
            {
                OnFactoryItem(belt.itemObjList[0]);
                belt.itemObjList[0].transform.position = this.transform.position;
                belt.isItemStop = false;
                belt.itemObjList.RemoveAt(0);
                belt.beltGroupMgr.groupItem.RemoveAt(0);
                belt.ItemNumCheck();

                getItemIndex++;
                if (getItemIndex >= inObj.Count)
                    getItemIndex = 0;
              
                Invoke("DelayGetItem", structureData.SendDelay);
            }
            else if (belt.isItemStop == false)
            {
                getItemIndex++;
                if (getItemIndex >= inObj.Count)
                    getItemIndex = 0;

                DelayGetItem();
                return;
            }
        }
        else
        {
            getItemIndex++;
            if (getItemIndex >= inObj.Count)
                getItemIndex = 0;

            DelayGetItem();
            return;
        }
    }

    protected virtual void SendItem(Item item) 
    {
        if (setFacDelayCoroutine != null)
        {
            return;
        }        
        else if(sendItemIndex > outObj.Count)
        {
            sendItemIndex = 0;
            return;
        }
        else if (outObj[sendItemIndex] == null)
        {
            sendItemIndex = 0;
            return;
        }


        itemSetDelay = true;

        Structure outFactory = outObj[sendItemIndex].GetComponent<Structure>();

        if (outFactory.isFull == false)
        {
            if (outObj[sendItemIndex].TryGetComponent(out BeltCtrl beltCtrl))
            {
                var itemPool = ItemPoolManager.instance.Pool.Get();
                spawnItem = itemPool.GetComponent<ItemProps>();
                if (beltCtrl.OnBeltItem(spawnItem))
                {
                    SpriteRenderer sprite = spawnItem.GetComponent<SpriteRenderer>();
                    spawnItem.col.enabled = false;
                    sprite.sprite = item.icon;
                    sprite.sortingOrder = 2;
                    spawnItem.item = item;
                    spawnItem.amount = 1;
                    spawnItem.transform.position = transform.position;
                    spawnItem.isOnBelt = true;
                    spawnItem.setOnBelt = beltCtrl.GetComponent<BeltCtrl>();

                    if (GetComponent<Production>())
                    {
                        SubFromInventory();
                    }
                    else if (GetComponent<LogisticsCtrl>() && !GetComponent<ItemSpawner>() && !GetComponent<Unloader>())
                    {
                        itemList.RemoveAt(0);
                        ItemNumCheck();
                    }
                }
                else
                {
                    DelaySetItem();
                    return;
                }
            }
            else if (outFactory.isMainSource)
            {
                sendItemIndex++;
                if (sendItemIndex >= outObj.Count)
                    sendItemIndex = 0;

                DelaySetItem();
            }
            else if (!outFactory.isMainSource)
            {
                if (outObj[sendItemIndex].GetComponent<LogisticsCtrl>())                
                    setFacDelayCoroutine = StartCoroutine(SendFacDelayArguments(outObj[sendItemIndex], item));
                else if (outObj[sendItemIndex].TryGetComponent(out Production production) && production.CanTakeItem(item))
                    setFacDelayCoroutine = StartCoroutine(SendFacDelayArguments(outObj[sendItemIndex], item));
            }

            sendItemIndex++;
            if (sendItemIndex >= outObj.Count)
                sendItemIndex = 0;

            Invoke("DelaySetItem", structureData.SendDelay);
        }
        else
        {
            sendItemIndex++;
            if (sendItemIndex >= outObj.Count)
                sendItemIndex = 0;

            DelaySetItem();
        }
    }

    protected IEnumerator SendFacDelayArguments(GameObject game, Item item)
    {
        yield return StartCoroutine(SendFacDelay(game, item));
    }

    protected virtual IEnumerator SendFacDelay(GameObject outFac, Item item)
    {
        var itemPool = ItemPoolManager.instance.Pool.Get();
        spawnItem = itemPool.GetComponent<ItemProps>();
        spawnItem.col.enabled = false;
        SpriteRenderer sprite = spawnItem.GetComponent<SpriteRenderer>();
        sprite.color = new Color(1f, 1f, 1f, 0f);
        CircleCollider2D coll = spawnItem.GetComponent<CircleCollider2D>();
        coll.enabled = false;

        spawnItem.transform.position = this.transform.position;

        var targetPos = outFac.transform.position;
        var startTime = Time.time;
        var distance = Vector3.Distance(spawnItem.transform.position, targetPos);

        while (spawnItem != null && spawnItem.transform.position != targetPos)
        {
            var elapsed = Time.time - startTime;
            var t = Mathf.Clamp01(elapsed / (distance / structureData.SendSpeed[0]));

            spawnItem.transform.position = Vector3.Lerp(spawnItem.transform.position, targetPos, t);

            yield return null;
        }

        if (spawnItem != null && spawnItem.transform.position == targetPos)
        {
            if (CanSendItemCheck())
            {
                if (checkObj && outObj.Count > 0 && outFac != null)
                {
                    if (outFac.TryGetComponent(out Structure outFactory))
                    {
                        outFactory.OnFactoryItem(item);
                    }
                }
                else
                {
                    sprite.color = new Color(1f, 1f, 1f, 1f);
                    coll.enabled = true;
                    spawnItem.Pool.Release(itemPool);
                    spawnItem = null;
                }
                if(GetComponent<LogisticsCtrl>() && !GetComponent<ItemSpawner>())
                {
                    itemList.RemoveAt(0);
                    ItemNumCheck();
                }
                else if (GetComponent<Production>())
                {
                    SubFromInventory();
                }
            }
        }

        if (spawnItem != null)
        {
            sprite.color = new Color(1f, 1f, 1f, 1f);
            coll.enabled = true;
            setFacDelayCoroutine = null;
            spawnItem.Pool.Release(itemPool);
        }
        else
        {
            setFacDelayCoroutine = null;
        }
    }

    bool CanSendItemCheck()
    {
        if (GetComponent<LogisticsCtrl>())
        {
            if (GetComponent<ItemSpawner>())
                return true;
            else if (itemList.Count > 0)
                return true;
        }
        else if(GetComponent<Production>() && CheckOutItemNum())
        {
            return true;
        }

        return false;
    }

    protected virtual void SubFromInventory() { }

    public void TakeDamage(float damage)
    {
        if (!isPreBuilding)
        {
            if (!unitCanvas.activeSelf)
            {
                unitCanvas.SetActive(true);
                hpBar.enabled = true;
            }
        }

        if (hp <= 0f)
            return;

        hp -= damage;
        hpBar.fillAmount = hp / structureData.MaxHp[level];

        if (hp <= 0f)
        {
            hp = 0f;
            DieFunc();
        }
    }

    protected virtual void DieFunc()
    {
        hp = structureData.MaxHp[level];
        repairBar.enabled = true;
        hpBar.enabled = false;
        repairGauge = 0;
        repairBar.fillAmount = repairGauge / structureData.MaxBuildingGauge;
        ColliderTriggerOnOff(true);
        isRuin = true;

        if (!isPreBuilding)
        {
            foreach (GameObject monster in monsterList)
            {
                if (monster.TryGetComponent(out MonsterAi monsterAi))
                {
                    monsterAi.RemoveTarget(this.gameObject);
                }
            }
        }
        else
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(this.transform.position, structureData.ColliderRadius);

            foreach (Collider2D collider in colliders)
            {
                GameObject monster = collider.gameObject;
                if (monster.CompareTag("Monster"))
                {
                    if (!monsterList.Contains(monster))
                    {
                        monsterList.Add(monster);
                    }
                }
            }
            foreach (GameObject monsterObj in monsterList)
            {
                if (monsterObj.TryGetComponent(out MonsterAi monsterAi))
                {
                    monsterAi.RemoveTarget(this.gameObject);
                }
            }
        }
        monsterList.Clear();
    }

    public void HealFunc(float heal)
    {
        if (hp == structureData.MaxHp[level])
        {
            return;
        }
        else if (hp + heal > structureData.MaxHp[level])
        {
            hp = structureData.MaxHp[level];
            if (!isRepair)
                unitCanvas.SetActive(false);
        }
        else
            hp += heal;

        hpBar.fillAmount = hp / structureData.MaxHp[level];
    }

    public void RepairSet(bool repair)
    {
        hp = structureData.MaxHp[level];
        isRepair = repair;
    }

    protected void RepairFunc(bool isBuilding)
    {
        repairGauge += 10.0f * Time.deltaTime;

        if (isBuilding)
        {
            repairBar.fillAmount = repairGauge / structureData.MaxBuildingGauge;
            if (repairGauge >= structureData.MaxBuildingGauge)
            {
                isPreBuilding = false;
                repairGauge = 0.0f;
                repairBar.enabled = false;
                if (hp < structureData.MaxHp[level])
                {
                    unitCanvas.SetActive(true);
                    hpBar.enabled = true;
                }
                else
                {
                    unitCanvas.SetActive(false);
                }
                 
                ColliderTriggerOnOff(false);
            }
        }
        else
        {
            repairBar.fillAmount = repairGauge / structureData.MaxRepairGauge;
            if (repairGauge >= structureData.MaxRepairGauge)
            {
                RepairEnd();
            }
        }
    }

    protected virtual void RepairEnd()
    {
        hpBar.enabled = true;
        hp = structureData.MaxHp[level];
        unitCanvas.SetActive(false);
        hpBar.fillAmount = hp / structureData.MaxHp[level];
        repairBar.enabled = false;
        repairGauge = 0.0f;
        isRuin = false;
        isPreBuilding = false;
        ColliderTriggerOnOff(false);
    }

    public virtual void ColliderTriggerOnOff(bool isOn)
    {
        if (isOn)
            col.isTrigger = true;
        else
            col.isTrigger = false;
    }

    protected void RemoveSameOutList()
    {
        outSameList.Clear();
    }

    protected virtual IEnumerator SetInObjCoroutine(GameObject obj) 
    {
        checkObj = false;
        yield return new WaitForSeconds(0.1f);

        if (obj.GetComponent<Structure>() != null)
        {
            if (obj.TryGetComponent(out BeltCtrl belt))
            {
                if (belt.GetComponentInParent<BeltGroupMgr>().nextObj != this.gameObject)
                {
                    checkObj = true;
                    yield break;
                }
                belt.FactoryPosCheck(GetComponentInParent<Structure>());
            }
            inObj.Add(obj);
            StartCoroutine(UnderBeltConnectCheck(obj));
        }
    }

    protected virtual IEnumerator SetOutObjCoroutine(GameObject obj)
    {
        checkObj = false;
        yield return new WaitForSeconds(0.1f);

        if (obj.GetComponent<Structure>() != null)
        {
            if ((obj.GetComponent<ItemSpawner>() && GetComponent<ItemSpawner>())
                || obj.GetComponent<Unloader>())
            {
                checkObj = true;
                yield break;
            }

            if (obj.TryGetComponent(out BeltCtrl belt))
            {
                if (obj.GetComponentInParent<BeltGroupMgr>().nextObj == this.gameObject)
                {
                    checkObj = true;
                    yield break;
                }
                belt.FactoryPosCheck(GetComponentInParent<Structure>());
            }
            else
            {
                outSameList.Add(obj);
                StartCoroutine(OutCheck(obj));
            }
            outObj.Add(obj);
            StartCoroutine(UnderBeltConnectCheck(obj));
        }
    }

    protected virtual IEnumerator OutCheck(GameObject otherObj)
    {
        yield return new WaitForSeconds(0.1f);

        if (otherObj.TryGetComponent(out Structure otherFacCtrl))
        {
            if (otherObj.GetComponent<Production>())
                yield break;

            if (otherFacCtrl.outSameList.Contains(this.gameObject) && outSameList.Contains(otherObj))
            {
                StopCoroutine("SendFacDelay");
                outObj.Remove(otherObj);
                Invoke("RemoveSameOutList", 0.1f);
                sendItemIndex = 0;
            }
        }
    }

    public virtual void ResetCheckObj(GameObject game)
    {
        checkObj = false;

        if (inObj.Contains(game))
        {
            inObj.Remove(game);
            getItemIndex = 0;
        }
        if (outObj.Contains(game))
        {
            outObj.Remove(game);
            sendItemIndex = 0;
        }

        for (int i = 0; i < nearObj.Length; i++)
        {
            if (nearObj[i] != null && nearObj[i] == game) 
            {
                nearObj[i] = null;
            }
        }

        checkObj = true;
    }

    public virtual void RemoveObj() 
    {
        removeState = true;
        ColliderTriggerOnOff(true); 
        StopAllCoroutines();

        for (int i = 0; i < nearObj.Length; i++)
        {
            if(nearObj[i] != null && nearObj[i].TryGetComponent(out Structure structure))
            {
                structure.ResetCheckObj(this.gameObject);
                if (structure.GetComponentInParent<BeltGroupMgr>())
                {
                    BeltGroupMgr beltGroup = structure.GetComponentInParent<BeltGroupMgr>();
                    beltGroup.nextCheck = true;
                }
            }
        }
        if (repairTower != null) 
            repairTower.RemoveObjectsOutOfRange(this.gameObject);

        if (GetComponent<BeltCtrl>() && GetComponentInParent<BeltManager>() && GetComponentInParent<BeltGroupMgr>())
        {
            BeltManager beltManager = GetComponentInParent<BeltManager>();
            BeltGroupMgr beltGroup = GetComponentInParent<BeltGroupMgr>();
            beltManager.BeltDivide(beltGroup, this.gameObject);
        }
        else if (TryGetComponent(out FluidFactoryCtrl fluid))
        {
            fluid.RemoveMainSource(true);
        }
        else if (TryGetComponent(out TransportBuild trBuild))
        {
            trBuild.RemoveFunc();
        }

        AddInvenItem();

        GameManager gameManager = GameManager.instance;
        int x = Mathf.FloorToInt(transform.position.x);
        int y = Mathf.FloorToInt(transform.position.y);
        if (sizeOneByOne)
        {
            if (gameManager.map.IsOnMap(x, y) && gameManager.map.mapData[x][y].structure == gameObject)
            {
                gameManager.map.mapData[x][y].structure = null;
            }
        }
        else
        {
            if (gameManager.map.IsOnMap(x, y) && gameManager.map.mapData[x][y].structure == gameObject)
            {
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        gameManager.map.mapData[x + j][y + i].structure = null;
                    }
                }
            }
        }

         Destroy(this.gameObject);
    }

    public virtual void TempBuilCooldownSet() { }


    protected virtual void AddInvenItem() { }


    protected void DelaySetItem()
    {
        itemSetDelay = false;
    }

    protected void DelayGetItem()
    {
        itemGetDelay = false;
    }

    public virtual Dictionary<Item, int> PopUpItemCheck() { return null; }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<UnitAi>() || collision.GetComponent<PlayerController>())
        {
            if (isPreBuilding)
            {
                buildingPosUnit.Add(collision.gameObject);

                if (buildingPosUnit.Count > 0)
                {
                    if (!collision.GetComponentInParent<PreBuilding>())
                    {
                        canBuilding = false;
                    }

                    PreBuilding preBuilding = GetComponentInParent<PreBuilding>();
                    if (preBuilding != null)
                    {
                        preBuilding.isBuildingOk = false;
                    }
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<UnitAi>() || collision.GetComponent<PlayerController>())
        {
            if (isPreBuilding)
            {
                buildingPosUnit.Remove(collision.gameObject);
                if (buildingPosUnit.Count > 0)
                    canBuilding = false;
                else
                {
                    canBuilding = true;

                    PreBuilding preBuilding = GetComponentInParent<PreBuilding>();
                    if (preBuilding != null)
                        preBuilding.isBuildingOk = true;
                }
            }
        }
    }
}
