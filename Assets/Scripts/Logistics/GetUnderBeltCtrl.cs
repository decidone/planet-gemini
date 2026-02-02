using System.Collections;
using UnityEngine;
using System;
using Unity.Netcode;

// UTF-8 설정
public class GetUnderBeltCtrl : LogisticsCtrl
{
    PreBuilding preBuilding;
    bool preBuildingCheck;
    [SerializeField]
    protected GameObject lineObj;
    public LineRenderer lineRenderer;
    protected Vector3 startLine;
    protected Vector3 endLine;

    void Start()
    {
        gameManager = GameManager.instance;
        preBuilding = PreBuilding.instance;

        StrBuilt();
    }

    protected override void Update()
    {
        base.Update();
        if (!removeState)
        {
            //SetDirNum();
            //if (isSetBuildingOk)
            //{
            //    for (int i = 0; i < nearObj.Length; i++)
            //    {
            //        if (nearObj[i] == null)
            //        {
            //            if (i == 0)
            //                CheckNearObj(checkPos[0], 0, obj => StartCoroutine(SetOutObjCoroutine(obj)));
            //            else if (i == 2) 
            //                CheckNearObj(checkPos[2], 2, obj => StartCoroutine(SetInObjCoroutine(obj)));
            //        }
            //    }
            //}

            if (IsServer && !isPreBuilding)
            {
                if (itemList.Count > 0 && outObj.Count > 0 && !itemSetDelay)
                {
                    int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(itemList[0]);
                    SendItem(itemIndex);
                    //SendItem(itemList[0]);
                }
            }

            if (gameManager.focusedStructure == null)
            {
                if (preBuilding.isBuildingOn && preBuilding.isUnderObj && preBuilding.isUnderBelt)
                {
                    if (!preBuildingCheck)
                    {
                        StartRenderer();
                    }
                }
                else
                {
                    if (preBuildingCheck)
                    {
                        EndRenderer();
                    }
                }
            }
        }
    }

    public void StartRenderer()
    {
        if (inObj.Count > 0)
        {
            startLine = new Vector3(inObj[0].transform.position.x, inObj[0].transform.position.y, -1);
            GameObject currentLine = Instantiate(lineObj, startLine, Quaternion.identity);
            lineRenderer = currentLine.GetComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, startLine);
            endLine = new Vector3(transform.position.x, transform.position.y, -1);
            lineRenderer.SetPosition(1, endLine);
            preBuildingCheck = true;
        }
    }

    public void EndRenderer()
    {
        if (lineRenderer != null)
        {
            Destroy(lineRenderer.gameObject);
            lineRenderer = null;
        }
        preBuildingCheck = false;
    }

    [ClientRpc]
    public override void UpgradeFuncClientRpc()
    {
        //base.UpgradeFuncClientRpc();
        UpgradeFunc();

        setModel.sprite = modelNum[dirNum + (level * 4)];
    }

    public override void StrBuilt()
    {
        base.StrBuilt();

        float dist = 10;

        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, checkPos[0], dist);

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hitCollider = hits[i].collider;
            if (hitCollider.CompareTag("Factory") && hitCollider.gameObject != this.gameObject)
            {
                if (hitCollider.TryGetComponent(out GetUnderBeltCtrl getUnderBelt) && getUnderBelt.dirNum == dirNum)
                {
                    getUnderBelt.NearStrBuilt();
                    return;
                }
            }
        }
    }

    public override void NearStrBuilt()
    {
        // 건물을 지었을 때나 근처에 새로운 건물이 지어졌을 때 동작
        // 변경사항이 생기면 DelayNearStrBuiltCoroutine()에도 반영해야 함
        if (IsServer)
        {
            CheckPos();
            for (int i = 0; i < nearObj.Length; i++)
            {
                if (!nearObj[i])
                {
                    if (i == 0)
                        CheckNearObj(checkPos[0], 0, obj => StartCoroutine(SetOutObjCoroutine(obj)));
                    else if (i == 2)
                        CheckNearObj(checkPos[2], 2, obj => StartCoroutine(SetInObjCoroutine(obj)));
                }
            }
            setModel.sprite = modelNum[dirNum + (level * 4)];
        }
        else
        {
            DelayNearStrBuilt();
        }
    }

    public override void DelayNearStrBuilt()
    {
        // 동시 건설, 클라이언트 동기화 등의 이유로 딜레이를 주고 NearStrBuilt()를 실행할 때 사용
        StartCoroutine(DelayNearStrBuiltCoroutine());
    }

    protected override IEnumerator DelayNearStrBuiltCoroutine()
    {
        // 동시 건설이나 그룹핑을 따로 예외처리 하는 경우가 아니면 NearStrBuilt()를 그대로 사용
        yield return new WaitForEndOfFrame();

        CheckPos();
        for (int i = 0; i < nearObj.Length; i++)
        {
            if (!nearObj[i])
            {
                if (i == 0)
                    CheckNearObj(checkPos[0], 0, obj => StartCoroutine(SetOutObjCoroutine(obj)));
                else if (i == 2)
                    CheckNearObj(checkPos[2], 2, obj => StartCoroutine(SetInObjCoroutine(obj)));
            }
        }
        setModel.sprite = modelNum[dirNum + (level * 4)];
    }

    protected override void CheckNearObj(Vector2 direction, int index, Action<GameObject> callback)
    {
        float dist = 0;

        if (index == 2)
            dist = 10;
        else
            dist = 1;

        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, dist);

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hitCollider = hits[i].collider;
            if (hitCollider.TryGetComponent(out Structure str) && str.destroyStart)
            {
                continue; // 구조물이 파괴 중이면 무시
            }

            if (index != 2)
            {
                if (hitCollider.CompareTag("Factory") && hits[i].collider.gameObject != this.gameObject)
                {
                    nearObj[index] = hits[i].collider.gameObject;
                    callback(hitCollider.gameObject);
                    break;
                }
            }
            else
            {
                if (hitCollider.CompareTag("Factory") && hitCollider.GetComponent<GetUnderBeltCtrl>() != this)
                {
                    if (hitCollider.TryGetComponent(out GetUnderBeltCtrl othGet) && othGet.dirNum == dirNum)                    
                    {
                        break;
                    }
                    else if(hitCollider.TryGetComponent(out SendUnderBeltCtrl sendUnderBelt))
                    {
                        if (sendUnderBelt.dirNum == dirNum)
                        {
                            nearObj[index] = hits[i].collider.gameObject;
                            callback(hitCollider.gameObject);
                            break;
                        }
                    }
                }
            }
        }
    }
    protected override IEnumerator SetOutObjCoroutine(GameObject obj)
    {
        yield return new WaitForSeconds(0.1f);
        
        if (obj.GetComponent<WallCtrl>())
            yield break;

        if (obj.GetComponent<Structure>() != null)
        {
            if ((obj.GetComponent<ItemSpawner>() && GetComponent<ItemSpawner>())
                || obj.GetComponent<Unloader>())
            {
                yield break;
            }

            if (obj.TryGetComponent(out BeltCtrl belt))
            {
                if (obj.GetComponentInParent<BeltGroupMgr>().nextObj == this.gameObject)
                {
                    yield break;
                }
                if (!outObj.Contains(obj))
                    outObj.Add(obj);
                belt.FactoryPosCheck(GetComponentInParent<Structure>());
            }
            else
            {
                outSameList.Add(obj);
                StartCoroutine(OutCheck(obj));
                StartCoroutine(UnderBeltConnectCheck(obj));
            }
            //if (!outObj.Contains(obj))
            //    outObj.Add(obj);
        }
        //checkObj = true;
    }

    [ClientRpc]
    public override void SettingClientRpc(int _level, int _beltDir, int objHeight, int objWidth, bool isHostMap, int index)
    {
        level = _level;
        dirNum = _beltDir;
        height = objHeight;
        width = objWidth;
        buildingIndex = index;
        isInHostMap = isHostMap;
        settingEndCheck = true;
        SetBuild();
        ColliderTriggerOnOff(true);

        if (col != null)
        {
            // 3. A* 그래프 업데이트 (해당 영역을 길막으로 인식시킴)
            Bounds b = col.bounds;
            AstarPath.active.UpdateGraphs(b);
        }
        //gameObject.AddComponent<DynamicGridObstacle>();
        myVision.SetActive(true);
        DataSet();

        if (energyUse)
        {
            GameObject TriggerObj = new GameObject("Trigger");
            CircleCollider2D coll = TriggerObj.AddComponent<CircleCollider2D>();
            coll.isTrigger = true;
            TriggerObj.transform.position = Vector3.zero;
            StartCoroutine(Move(TriggerObj));
        }
        soundManager.PlaySFX(gameObject, "structureSFX", "BuildingSound");
    }

    protected override IEnumerator SetInObjCoroutine(GameObject obj)
    {
        yield return new WaitForSeconds(0.1f);

        SendUnderBeltCtrl sendUnderbelt = obj.GetComponent<SendUnderBeltCtrl>();

        if (sendUnderbelt.dirNum == dirNum)
        {
            inObj.Add(obj);
            sendUnderbelt.SetOutObj(this.gameObject);
        }
    }

    public void ResetInObj()
    {
        nearObj[2] = null;
        EndRenderer();
        inObj.Clear();
    }

    public override StructureSaveData SaveData()
    {
        StructureSaveData data = base.SaveData();
        data.sideObj = true;
        return data;
    }

    public override void ColliderTriggerOnOff(bool isOn)
    {
        col.isTrigger = true;
    }

    public override void Focused()
    {
        StartRenderer();
    }

    public override void DisableFocused()
    {
        EndRenderer();
    }
}
