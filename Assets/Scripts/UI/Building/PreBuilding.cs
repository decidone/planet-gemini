using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using Pathfinding;
using Unity.Netcode;
using UnityEngine.InputSystem;

// UTF-8 설정
public class PreBuilding : NetworkBehaviour
{
    protected SpriteRenderer spriteRenderer;
    protected GameObject nonNetObj;
    public GameObject beltGroupSet;

    public GameObject beltMgr;
    public GameObject beltGroup;

    protected bool isNeedSetPos = false;
    protected Vector3 setPos;

    protected Tilemap tilemap;

    protected int layNumTemp = 0;

    protected int objHeight = 1;
    protected int objWidth = 1;
    protected bool isGetDir = false;
    protected int level;
    protected int dirNum = 0;

    protected Vector3 startBuildPos;
    protected Vector3 endBuildPos;

    public bool isUnderObj = false;
    [HideInInspector]
    public bool isEnough = true;
    protected int canBuildCount;

    protected List<GameObject> buildingList = new List<GameObject>();
    protected Vector3 tempPos;
    public List<Vector3> posList = new List<Vector3>();
    protected bool isMoveX = true;
    protected bool tempMoveX;
    protected bool moveDir;
    protected bool tempMoveDir;
    public bool isPreObjSend;
    protected Vector3 mousePos;
    protected bool isDrag = false;
    public bool canNotDrag;
    protected Coroutine setBuild;

    //bool isTempBuild;
    protected bool mouseHoldCheck;   //기존 isLeftMouse기능 대체+a 역할이라 실제 hold감지는 InputManager의 hold를 사용

    protected GameManager gameManager;
    protected InputManager inputManager;

    [SerializeField]
    protected float maxBuildDist;

    [SerializeField]
    protected bool isBeltObj = false;
    [SerializeField]
    protected bool reversSet = false;
    [SerializeField]
    protected bool isPortalObj = false;
    [SerializeField]
    protected bool isScienceBuilding = false;
    Portal portalScript;
    protected int portalIndex;

    protected BuildingInvenManager buildingInven;

    protected SoundManager soundManager;

    protected BuildingList buildingListSO;
    protected int buildingIndex;

    public bool isBuildingOn = false;
    public GameObject preBuildingNonNet;
    protected List<Sprite> spriteList = new List<Sprite>();
    protected bool isGetAnim;
    protected Building buildData;

    public bool isEnergyUse;
    public bool isEnergyStr;

    public bool isUnderBelt;

    protected bool isInHostMap;

    [SerializeField]
    protected GameObject lineObj;
    [SerializeField]
    protected GameObject fluidLineObj;

    #region Singleton
    public static PreBuilding instance;

    protected virtual void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }
    #endregion

    protected void Start()
    {
        mouseHoldCheck = false;
        tilemap = GameObject.Find("Tilemap").GetComponent<Tilemap>();

        gameManager = GameManager.instance;
        buildingInven = BuildingInvenManager.instance;
        soundManager = SoundManager.instance;
        buildingListSO = BuildingList.instance;
    }
    protected virtual void OnEnable()
    {
        inputManager = InputManager.instance;
        inputManager.controls.Building.LeftMouseButtonDown.performed += LeftMouseButtonDown;
        inputManager.controls.Building.LeftMouseButtonUp.performed += LeftMouseButtonUpCommand;
        inputManager.controls.Building.RightMouseButtonDown.performed += CancelBuild;
        inputManager.controls.Building.Rotate.performed += Rotate;
    }
    protected virtual void OnDisable()
    {
        inputManager.controls.Building.LeftMouseButtonDown.performed -= LeftMouseButtonDown;
        inputManager.controls.Building.LeftMouseButtonUp.performed -= LeftMouseButtonUpCommand;
        inputManager.controls.Building.RightMouseButtonDown.performed -= CancelBuild;
        inputManager.controls.Building.Rotate.performed -= Rotate;
    }

    protected virtual void Update()
    {
        if (Time.timeScale == 0)
        {
            return;
        }

        if (isBuildingOn)
        {
            mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPosition = tilemap.WorldToCell(mousePos);
            Vector3 cellCenter = tilemap.GetCellCenterWorld(cellPosition);
            cellCenter.z = transform.position.z;
            transform.position = cellCenter;
            if (nonNetObj && !(mouseHoldCheck && canNotDrag))
                nonNetObj.transform.position = this.transform.position - setPos;

            if (spriteRenderer != null)
                BuildingListSetColor();
        }
    }

    protected virtual void FixedUpdate()
    {
        if (isEnough && mouseHoldCheck && isBuildingOn)
        {
            if (buildingList.Count <= canBuildCount)
            {
                tempPos = transform.position;
                float tempDeltaX = tempPos.x - endBuildPos.x;
                float tempDeltaY = tempPos.y - endBuildPos.y;
                float tempAbsDeltaX = Mathf.Abs(tempDeltaX);
                float tempAbsDeltaY = Mathf.Abs(tempDeltaY);

                if (tempAbsDeltaX >= 1 || tempAbsDeltaY >= 1)
                {
                    endBuildPos = tempPos;

                    float deltaX = endBuildPos.x - startBuildPos.x;
                    float deltaY = endBuildPos.y - startBuildPos.y;
                    float absDeltaX = Mathf.Abs(deltaX);
                    float absDeltaY = Mathf.Abs(deltaY);

                    if (absDeltaX >= absDeltaY)
                        isMoveX = true;
                    else
                        isMoveX = false;

                    CheckPos();
                    isDrag = true;
                }
            }
        }
    }

    protected void LeftMouseButtonDown(InputAction.CallbackContext ctx)
    {
        if (isBuildingOn)
        {
            startBuildPos = transform.position;
            endBuildPos = transform.position;
            if (!RaycastUtility.IsPointerOverUI(Input.mousePosition))
                mouseHoldCheck = true;
        }
    }

    [Command]
    protected virtual void LeftMouseButtonUpCommand(InputAction.CallbackContext ctx)
    {
        if (isBuildingOn)
        {
            if (isEnough && mouseHoldCheck)
            {
                if (RaycastUtility.IsPointerOverUI(Input.mousePosition))
                {
                    CancelBuild();
                    return;
                }

                if (!isDrag && !RaycastUtility.IsPointerOverUI(Input.mousePosition))
                    CheckPos();

                if (buildingList.Count < 1)
                {
                    CancelBuild();
                    return;
                }

                bool canBuild = false;
                if (!canNotDrag)
                {               
                    int index = 0;
                    foreach (GameObject obj in buildingList)
                    {
                        if (GroupBuildCheck(obj, posList[index]))
                            canBuild = true;
                        else
                        {
                            canBuild = false;
                            break;
                        }
                        index++;
                    }
                }
                else if (CellCheck(buildingList[0], startBuildPos))
                {
                    canBuild = buildingList[0].GetComponent<PreBuildingImg>().CanPlaceBuilding(new Vector2(objWidth, objHeight));
                }

                if (canBuild)
                {
                    Vector3[] pos = new Vector3[buildingList.Count];

                    for (int i = 0; i < buildingList.Count; i++) 
                    {
                        pos[i] = buildingList[i].transform.position;
                    }

                    if (!nonNetObj.GetComponent<UnderObjBuilding>())
                    {
                        BuildingServerRpc(isInHostMap, buildingIndex, pos, dirNum, isBeltObj, reversSet, gameManager.debug);
                    }
                    else
                    {
                        int[] dir = new int[buildingList.Count];
                        //bool underBelt = isUnderBelt;
                        bool[] sideObj = new bool[buildingList.Count];

                        for (int i = 0; i < buildingList.Count; i++)
                        {
                            UnderObjBuilding unObj = buildingList[i].GetComponent<UnderObjBuilding>();
                            dir[i] = unObj.dirNum;
                            sideObj[i] = unObj.isSendObj;
                        }
                        BuildingServerRpc(isInHostMap, buildingIndex, pos, dir, isUnderBelt, sideObj, gameManager.debug);
                    }
                }

                foreach (GameObject build in buildingList)
                {
                    Destroy(build);
                }

                buildingList.Clear();
                nonNetObj.SetActive(true);

                isEnough = BuildingInfo.instance.AmountsEnoughCheck();
                if (!gameManager.debug)
                    canBuildCount = BuildingInfo.instance.CanBuildAmount();
                else
                    canBuildCount = int.MaxValue;

                //if (isPortalObj)
                //    Invoke(nameof(CancelBuild), 0.1f);

                PreBuildingImg preBuildingImg = nonNetObj.GetComponent<PreBuildingImg>();

                if (isGetAnim)
                {
                    if (isGetDir)
                    {
                        preBuildingImg.AnimSetFloat("DirNum", dirNum);
                        if(isBeltObj)
                            preBuildingImg.AnimSetFloat("Level", level);        
                    }
                }
                else
                {
                    if(isUnderObj && !isUnderBelt)
                    {
                        int otherDir = (dirNum + 2) % 4;
                        preBuildingImg.PreSpriteSet(spriteList[otherDir]);
                        dirNum = otherDir;
                    }
                    else
                        preBuildingImg.PreSpriteSet(spriteList[dirNum]);
                }

                if (nonNetObj.TryGetComponent(out UnderObjBuilding under))
                {
                    under.dirNum = dirNum;
                }

                posList.Clear();
                isDrag = false;
            }

            mouseHoldCheck = false;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    protected void BuildingServerRpc(bool isHostMap, int bIndex, Vector3[] setPos, int dir, bool isBelt, bool reversSet, bool debugModeOn)
    {
        int spawnCount = setPos.Length;
        Building building = buildingListSO.FindBuildingData(bIndex);
        BuildingData buildingData = BuildingDataGet.instance.GetBuildingName(building.item.name, building.level);

        Vector3 correctPos = Vector3.zero;
        if (building.height == 2 && building.width == 2)
        {
            correctPos = new Vector3(-0.5f, -0.5f);
        }

        // 아이템이 충분한지 체크
        if (building.type != "Portal")
        {
            if (!debugModeOn)
            {
                bool costEnough = false;
                for (int i = 0; i < buildingData.GetItemCount(); i++)
                {
                    int value;
                    Inventory inven;
                    if (isHostMap)
                        inven = gameManager.hostMapInven;
                    else
                        inven = gameManager.clientMapInven;

                    bool hasItem = inven.totalItems.TryGetValue(ItemList.instance.itemDic[buildingData.items[i]], out value);
                    costEnough = hasItem && value >= buildingData.amounts[i] * spawnCount;

                    if (!costEnough)
                        return;
                }
            }
        }
        else
        {
            if (gameManager.portal[isHostMap ? 0 : 1].PortalObjFind(building.item.name))
                return;
        }
        // 여기서는 셀에 다른 건물이 있는지만 체크
        for (int i = 0; i < spawnCount; i++)
        {
            int x = Mathf.FloorToInt(setPos[i].x + correctPos.x);
            int y = Mathf.FloorToInt(setPos[i].y + correctPos.y);

            List<int> xList = new List<int>();
            List<int> yList = new List<int>();

            if (building.height == 1 && building.width == 1)
            {
                xList.Add(x);
                yList.Add(y);
            }
            else if (building.height == 2 && building.width == 2)
            {
                xList.Add(x);
                xList.Add(x + 1);
                yList.Add(y);
                yList.Add(y + 1);
            }
            foreach (int newX in xList)
            {
                foreach (int newY in yList)
                {
                    Cell cell;
                    if (isHostMap)
                        cell = gameManager.hostMap.GetCellDataFromPos(newX, newY);
                    else
                        cell = gameManager.clientMap.GetCellDataFromPos(newX, newY);

                    if (cell.structure != null)
                    {
                        return;
                    }
                }
            }
        }
        // 위 조건 만족시 설치
        if (isBelt)
        {
            BeltGroupSpawnServerRpc();

            if (reversSet)
            {
                for (int i = spawnCount - 1; i >= 0; i--)
                {
                    SetBuilding(setPos[i], bIndex, isHostMap, building.level - 1, dir, building.height, building.width, false, false, false, false);
                }
            }
            else
            {
                for (int i = 0; i < spawnCount; i++)
                {
                    SetBuilding(setPos[i], bIndex, isHostMap, building.level - 1, dir, building.height, building.width, false, false, false, false);
                }
            }

            BeltGroupSetEndServerRpc();
        }
        else
        {
            for (int i = spawnCount - 1; i >= 0; i--)
            {
                SetBuilding(setPos[i], bIndex, isHostMap, building.level - 1, dir, building.height, building.width, building.type == "Portal", false, false, false);
            }
        }
        if (!debugModeOn)
            PayCost(buildingData, spawnCount);
    }

    [ServerRpc(RequireOwnership = false)]
    protected void BuildingServerRpc(bool isHostMap, int bIndex, Vector3[] setPos, int[] dir, bool isBelt, bool reversSet, bool debugModeOn)
    {
        int spawnCount = setPos.Length;
        Building building = buildingListSO.FindBuildingData(bIndex);
        BuildingData buildingData = BuildingDataGet.instance.GetBuildingName(building.item.name, building.level);

        Vector3 correctPos = Vector3.zero;
        if (building.height == 2 && building.width == 2)
        {
            correctPos = new Vector3(-0.5f, -0.5f);
        }

        // 아이템이 충분한지 체크
        if (building.type != "Portal")
        {
            if (!debugModeOn)
            {
                bool costEnough = false;
                for (int i = 0; i < buildingData.GetItemCount(); i++)
                {
                    int value;
                    Inventory inven;
                    if (isHostMap)
                        inven = gameManager.hostMapInven;
                    else
                        inven = gameManager.clientMapInven;

                    bool hasItem = inven.totalItems.TryGetValue(ItemList.instance.itemDic[buildingData.items[i]], out value);
                    costEnough = hasItem && value >= buildingData.amounts[i] * spawnCount;

                    if (!costEnough)
                        return;
                }
            }
        }
        else
        {
            if (gameManager.portal[isHostMap ? 0 : 1].PortalObjFind(building.item.name))
                return;
        }
        // 여기서는 셀에 다른 건물이 있는지만 체크
        for (int i = 0; i < spawnCount; i++)
        {
            int x = Mathf.FloorToInt(setPos[i].x + correctPos.x);
            int y = Mathf.FloorToInt(setPos[i].y + correctPos.y);

            List<int> xList = new List<int>();
            List<int> yList = new List<int>();

            if (building.height == 1 && building.width == 1)
            {
                xList.Add(x);
                yList.Add(y);
            }
            else if (building.height == 2 && building.width == 2)
            {
                xList.Add(x);
                xList.Add(x + 1);
                yList.Add(y);
                yList.Add(y + 1);
            }
            foreach (int newX in xList)
            {
                foreach (int newY in yList)
                {
                    Cell cell;
                    if (isHostMap)
                        cell = gameManager.hostMap.GetCellDataFromPos(newX, newY);
                    else
                        cell = gameManager.clientMap.GetCellDataFromPos(newX, newY);

                    if (cell.structure != null)
                    {
                        return;
                    }
                }
            }
        }
        // 위 조건 만족시 설치
        if (isBelt)
        {
            BeltGroupSpawnServerRpc();

            if (reversSet)
            {
                for (int i = spawnCount - 1; i >= 0; i--)
                {
                    SetBuilding(setPos[i], bIndex, isHostMap, building.level - 1, dir[i], building.height, building.width, false, false, false, false);
                }
            }
            else
            {
                for (int i = 0; i < spawnCount; i++)
                {
                    SetBuilding(setPos[i], bIndex, isHostMap, building.level - 1, dir[i], building.height, building.width, false, false, false, false);
                }
            }

            BeltGroupSetEndServerRpc();
        }
        else
        {
            for (int i = spawnCount - 1; i >= 0; i--)
            {
                SetBuilding(setPos[i], bIndex, isHostMap, building.level - 1, dir[i], building.height, building.width, building.type == "Portal", false, false, false);
            }
        }
        if (!debugModeOn)
            PayCost(buildingData, spawnCount);
    }

    [ServerRpc(RequireOwnership = false)]
    protected void BuildingServerRpc(bool isHostMap, int bIndex, Vector3[] setPos, int[] dir, bool underBelt, bool[] sideObj, bool debugModeOn) // 지하 오브젝트 같이 한 방향이 아닐때
    {
        int spawnCount = setPos.Length;
        Building building = buildingListSO.FindBuildingData(bIndex);
        BuildingData buildingData = BuildingDataGet.instance.GetBuildingName(building.item.name, building.level);

        Vector3 correctPos = Vector3.zero;
        if (building.height == 2 && building.width == 2)
        {
            correctPos = new Vector3(-0.5f, -0.5f);
        }

        // 아이템이 충분한지 체크
        if (!debugModeOn)
        {
            bool costEnough = false;

            for (int i = 0; i < buildingData.GetItemCount(); i++)
            {
                int value;
                Inventory inven;
                if (isHostMap)
                    inven = gameManager.hostMapInven;
                else
                    inven = gameManager.clientMapInven;

                bool hasItem = inven.totalItems.TryGetValue(ItemList.instance.itemDic[buildingData.items[i]], out value);
                costEnough = hasItem && value >= buildingData.amounts[i] * spawnCount;

                if (!costEnough)
                    return;
            }
        }
        // 여기서는 셀에 다른 건물이 있는지만 체크
        for (int i = 0; i < spawnCount; i++)
        {
            int x = Mathf.FloorToInt(setPos[i].x + correctPos.x);
            int y = Mathf.FloorToInt(setPos[i].y + correctPos.y);

            List<int> xList = new List<int>();
            List<int> yList = new List<int>();

            if (building.height == 1 && building.width == 1)
            {
                xList.Add(x);
                yList.Add(y);
            }
            else if (building.height == 2 && building.width == 2)
            {
                xList.Add(x);
                xList.Add(x + 1);
                yList.Add(y);
                yList.Add(y + 1);
            }
            foreach (int newX in xList)
            {
                foreach (int newY in yList)
                {
                    Cell cell;
                    if (isHostMap)
                        cell = gameManager.hostMap.GetCellDataFromPos(newX, newY);
                    else
                        cell = gameManager.clientMap.GetCellDataFromPos(newX, newY);

                    if (cell.structure != null)
                    {
                        return;
                    }
                }
            }
        }
        // 위 조건 만족시 설치

        for (int i = 0; i < spawnCount; i++)
        {
            SetBuilding(setPos[i], bIndex, isHostMap, building.level - 1, dir[i], building.height, building.width, false, true, underBelt, sideObj[i]);
        }
        if (!debugModeOn)
            PayCost(buildingData, spawnCount);
    }

    protected void PayCost(BuildingData buildingData, int amount)
    {
        if (buildingData == null)
            return;

        for (int i = 0; i < buildingData.GetItemCount(); i++)
        {
            Item item = ItemList.instance.itemDic[buildingData.items[i]];
            int cost = buildingData.amounts[i];
            Overall.instance.OverallConsumption(item, cost * amount);
            GameManager.instance.inventory.Sub(item, cost * amount);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    protected void BeltGroupSpawnServerRpc()
    {
        beltGroupSet = Instantiate(beltGroup);
        beltGroupSet.TryGetComponent(out NetworkObject netObj);
        if (!netObj.IsSpawned) beltGroupSet.GetComponent<NetworkObject>().Spawn(true);
        beltGroupSet.transform.parent = beltMgr.transform;
        BeltGroupSpawnClientRpc(beltGroupSet.GetComponent<NetworkObject>());
    }

    [ClientRpc]
    protected void BeltGroupSpawnClientRpc(NetworkObjectReference networkObjectReference)
    {
        networkObjectReference.TryGet(out NetworkObject networkObject);
        beltGroupSet = networkObject.gameObject;
    }

    [ServerRpc(RequireOwnership = false)]
    protected void BeltGroupSetEndServerRpc()
    {
        BeltGroupSetEndClientRpc();
    }


    [ClientRpc]
    protected void BeltGroupSetEndClientRpc()
    {
        if (!IsServer)
            return;

        if (beltGroupSet.TryGetComponent(out BeltGroupMgr beltGroupMgr))
        {
            beltGroupMgr.SetBeltData();
        }
    }

    public void CancelBuild(InputAction.CallbackContext ctx)
    {
        if(isBuildingOn)
            CancelBuild();
    }

    public void CancelBuild()
    {
        mouseHoldCheck = false;

        if (setBuild == null)
        {
            foreach (GameObject build in buildingList)
            {
                Destroy(build);
            }

            buildingList.Clear();
            ReSetImage();
            posList.Clear();
            isDrag = false;
        }
        buildingInven.PreBuildingCancel();
        isBuildingOn = false;
        isEnergyStr = false;
        isEnergyUse = false;
        isBeltObj = false;
        reversSet = false;
        canNotDrag = false;
        MouseSkin.instance.ResetCursor();
    }

    public void SwapBuilding() // 벨트 프리빌딩 전환용
    {
        mouseHoldCheck = false;

        if (setBuild == null)
        {
            foreach (GameObject build in buildingList)
            {
                Destroy(build);
            }

            buildingList.Clear();
            ReSetImage();
            posList.Clear();
            isDrag = false;
        }
        isBuildingOn = false;
        isEnergyStr = false;
        isEnergyUse = false;
        isBeltObj = false;
        reversSet = false;
        canNotDrag = false;
    }

    protected void Rotate(InputAction.CallbackContext ctx)
    {
        if (nonNetObj != null && !inputManager.mouseLeft)
        {
            RotationImg(nonNetObj);
        }
    }

    protected virtual void CheckPos()
    {
        if (canNotDrag)
        {
            nonNetObj.SetActive(false);
            Vector3 position = new Vector3(startBuildPos.x, startBuildPos.y, 0);
            PosListContainCheck(position);
        }
        else if (posList.Count > 0)
        {
            if (!isGetDir && isMoveX != tempMoveX)
            {
                if (buildingList.Count > 0)
                {
                    foreach (GameObject build in buildingList)
                    {
                        Destroy(build);
                    }
                    buildingList.Clear();
                }
                posList.Clear();
            }

            List<GameObject> objectsToRemove = new List<GameObject>(); // 삭제된 오브젝트를 추적할 리스트
            List<Vector3> posToRemove = new List<Vector3>();
            bool isRemoe = false;

            if ((!isGetDir && isMoveX) || (isGetDir && (dirNum == 1 || dirNum == 3)))
            {
                if (posList.Contains(new Vector3(tempPos.x, startBuildPos.y, 0)))
                {
                    if (posList.Count > 1)
                    {
                        if (startBuildPos.x > posList[1].x)
                        {
                            for (int i = 0; i < posList.Count; i++)
                            {
                                if (tempPos.x > posList[i].x)
                                {
                                    posToRemove.Add(posList[i]);
                                }
                            }
                            for (int b = buildingList.Count - 1; b > 0; b--)
                            {
                                if (tempPos.x > buildingList[b].transform.position.x)
                                {
                                    Destroy(buildingList[b]);
                                    objectsToRemove.Add(buildingList[b]);
                                    isRemoe = true;
                                }
                            }
                            buildingList.RemoveAll(obj => objectsToRemove.Contains(obj));
                            posList.RemoveAll(pos => posToRemove.Contains(pos));

                            if (buildingList[buildingList.Count - 1].TryGetComponent(out UnderObjBuilding lastUnderObj))
                            {
                                isPreObjSend = lastUnderObj.isSendObj;

                                if (isRemoe && buildingList[buildingList.Count - 1].transform.position != posList[posList.Count - 1])
                                {
                                    AddBuildingToList(posList[posList.Count - 1]);
                                    isPreObjSend = !isPreObjSend;
                                }
                            }
                        }

                        else if (startBuildPos.x < posList[1].x)
                        {
                            for (int i = 0; i < posList.Count; i++)
                            {
                                if (tempPos.x < posList[i].x)
                                {
                                    posToRemove.Add(posList[i]);
                                }
                            }
                            for (int b = buildingList.Count - 1; b > 0; b--)
                            {
                                if (tempPos.x < buildingList[b].transform.position.x - 0.5f)
                                {
                                    Destroy(buildingList[b]);
                                    objectsToRemove.Add(buildingList[b]);
                                    isRemoe = true;
                                }
                            }
                            buildingList.RemoveAll(obj => objectsToRemove.Contains(obj));
                            posList.RemoveAll(pos => posToRemove.Contains(pos));

                            if (buildingList[buildingList.Count - 1].TryGetComponent(out UnderObjBuilding lastUnderObj))
                            {
                                isPreObjSend = lastUnderObj.isSendObj;

                                if (isRemoe && buildingList[buildingList.Count - 1].transform.position != posList[posList.Count - 1])
                                {
                                    AddBuildingToList(posList[posList.Count - 1]);
                                    isPreObjSend = !isPreObjSend;
                                }
                            }
                        }
                    }
                }
                else
                {
                    SetPos();
                }
                tempMoveX = true;
            }
            else if ((!isGetDir && !isMoveX) || (isGetDir && (dirNum == 0 || dirNum == 2)))
            {
                if (posList.Contains(new Vector3(startBuildPos.x, tempPos.y, 0)))
                {
                    if (posList.Count > 1)
                    {
                        if (startBuildPos.y > posList[1].y)
                        {
                            for (int i = 0; i < posList.Count; i++)
                            {
                                if (tempPos.y > posList[i].y)
                                {
                                    posToRemove.Add(posList[i]);
                                }
                            }
                            for (int b = buildingList.Count - 1; b > 0; b--)
                            {
                                if (tempPos.y > buildingList[b].transform.position.y)
                                {
                                    Destroy(buildingList[b]);
                                    objectsToRemove.Add(buildingList[b]);
                                    isRemoe = true;
                                }
                            }
                            buildingList.RemoveAll(obj => objectsToRemove.Contains(obj));
                            posList.RemoveAll(pos => posToRemove.Contains(pos));

                            if (buildingList[buildingList.Count - 1].TryGetComponent(out UnderObjBuilding lastUnderObj))
                            {
                                isPreObjSend = lastUnderObj.isSendObj;

                                if (isRemoe && buildingList[buildingList.Count - 1].transform.position != posList[posList.Count - 1])
                                {
                                    AddBuildingToList(posList[posList.Count - 1]);
                                    isPreObjSend = !isPreObjSend;
                                }
                            }
                        }
                        else if (startBuildPos.y < posList[1].y)
                        {
                            for (int i = 0; i < posList.Count; i++)
                            {
                                if (tempPos.y < posList[i].y)
                                {
                                    posToRemove.Add(posList[i]);
                                }
                            }
                            for (int b = buildingList.Count - 1; b > 0; b--)
                            {
                                if (tempPos.y < buildingList[b].transform.position.y - 0.5f)
                                {
                                    Destroy(buildingList[b]);
                                    objectsToRemove.Add(buildingList[b]);
                                    isRemoe = true;
                                }
                            }
                            buildingList.RemoveAll(obj => objectsToRemove.Contains(obj));
                            posList.RemoveAll(pos => posToRemove.Contains(pos));

                            if (buildingList[buildingList.Count - 1].TryGetComponent(out UnderObjBuilding lastUnderObj))
                            {
                                isPreObjSend = lastUnderObj.isSendObj;
                                if (isRemoe && buildingList[buildingList.Count - 1].transform.position != posList[posList.Count - 1])
                                {
                                    AddBuildingToList(posList[posList.Count - 1]);
                                    isPreObjSend = !isPreObjSend;
                                }
                            }
                        }
                    }
                }
                else
                {
                    SetPos();
                }
                tempMoveX = false;
            }
        }
        else
        {
            SetPos();
        }
    }

    protected void SetPos()
    {
        bool notMove = false;

        if (startBuildPos.x == endBuildPos.x && startBuildPos.y == endBuildPos.y)
            notMove = true;

        if (notMove)
        {
            Vector3 position = new Vector3(startBuildPos.x, startBuildPos.y, 0);
            PosListContainCheck(position);
        }
        else if (isMoveX)
        {
            float currentX = startBuildPos.x;
            float direction = Mathf.Sign(endBuildPos.x - startBuildPos.x);

            if (direction > 0 && currentX < endBuildPos.x)
            {
                moveDir = true;
                if (isBeltObj)
                {
                    if (dirNum == 3)
                    {
                        reversSet = true;
                    }
                    else if (dirNum == 1)
                    {
                        reversSet = false;
                    }
                }
            }
            else if (direction < 0 && currentX > endBuildPos.x)
            {
                moveDir = false;
                if (isBeltObj)
                {
                    if (dirNum == 1)
                    {
                        reversSet = true;
                    }
                    else if (dirNum == 3)
                    {
                        reversSet = false;
                    }
                }
            }

            if (moveDir != tempMoveDir)
            {
                if (buildingList.Count > 0)
                {
                    foreach (GameObject build in buildingList)
                    {
                        Destroy(build);
                    }
                    buildingList.Clear();
                }
                posList.Clear();
            }

            if (!isGetDir || (!isUnderObj && isGetDir && (dirNum == 1 || dirNum == 3)))
            {
                while (Mathf.Approximately(currentX, endBuildPos.x) || (direction > 0 && currentX <= endBuildPos.x) || (direction < 0 && currentX >= endBuildPos.x))
                {
                    Vector3 position = new Vector3(currentX, startBuildPos.y, 0);
                    PosListContainCheck(position);
                    currentX += objWidth * direction;
                }
                tempMoveDir = moveDir;
            }
            else if (isUnderObj && isGetDir && (dirNum == 1 || dirNum == 3))
            {
                if (dirNum == 1 && moveDir)
                {
                    while (Mathf.Approximately(currentX, endBuildPos.x) || (direction > 0 && currentX <= endBuildPos.x))
                    {
                        Vector3 position = new Vector3(currentX, startBuildPos.y, 0);
                        PosListContainCheck(position);
                        currentX += objWidth * direction;
                    }
                    tempMoveDir = moveDir;
                }
                else if (dirNum == 3 && !moveDir)
                {
                    while (Mathf.Approximately(currentX, endBuildPos.x) || (direction < 0 && currentX >= endBuildPos.x))
                    {
                        Vector3 position = new Vector3(currentX, startBuildPos.y, 0);
                        PosListContainCheck(position);
                        currentX += objWidth * direction;
                    }
                    tempMoveDir = moveDir;
                }
            }
            else if (!isUnderObj && isGetDir && (dirNum == 0 || dirNum == 2))
            {
                float currentY = startBuildPos.y;
                while (Mathf.Approximately(currentY, endBuildPos.y) || (direction > 0 && currentY <= endBuildPos.y) || (direction < 0 && currentY >= endBuildPos.y))
                {
                    Vector3 position = new Vector3(startBuildPos.x, currentY, 0);
                    PosListContainCheck(position);
                    currentY += objHeight * direction;
                }
                tempMoveDir = moveDir;
            }
            else if (isUnderObj && isGetDir && (dirNum == 0 || dirNum == 2))
            {
                Vector3 position = new Vector3(startBuildPos.x, startBuildPos.y, 0);
                PosListContainCheck(position);
                tempMoveDir = moveDir;
            }
        }
        else if(!isMoveX)
        {
            float currentY = startBuildPos.y;
            float direction = Mathf.Sign(endBuildPos.y - startBuildPos.y);
            
            if (direction > 0 && currentY < endBuildPos.y)
            {
                moveDir = true;
                if (isBeltObj)
                {
                    if (dirNum == 2)
                    {
                        reversSet = true;
                    }
                    else if (dirNum == 0)
                    {
                        reversSet = false;
                    }
                }
            }
            else if (direction < 0 && currentY > endBuildPos.y)
            {
                moveDir = false;
                if (isBeltObj)
                {
                    if (dirNum == 0)
                    {
                        reversSet = true;
                    }
                    else if (dirNum == 2)
                    {
                        reversSet = false;
                    }
                }
            }

            if (moveDir != tempMoveDir)
            {
                if (buildingList.Count > 0)
                {
                    foreach (GameObject build in buildingList)
                    {
                        Destroy(build);
                    }
                    buildingList.Clear();
                }
                posList.Clear();
            }
            
            if (!isGetDir || (!isUnderObj && isGetDir && (dirNum == 0 || dirNum == 2)))
            {
                while (Mathf.Approximately(currentY, endBuildPos.y) || (direction > 0 && currentY <= endBuildPos.y) || (direction < 0 && currentY >= endBuildPos.y))
                {
                    Vector3 position = new Vector3(startBuildPos.x, currentY, 0);
                    PosListContainCheck(position);
                    currentY += objHeight * direction;
                }
                tempMoveDir = moveDir;
            }
            else if (isUnderObj && isGetDir && (dirNum == 0 || dirNum == 2))
            {
                if (dirNum == 0 && moveDir)
                {
                    while (Mathf.Approximately(currentY, endBuildPos.y) || (direction > 0 && currentY <= endBuildPos.y))
                    {
                        Vector3 position = new Vector3(startBuildPos.x, currentY, 0);
                        PosListContainCheck(position);
                        currentY += objHeight * direction;
                    }
                    tempMoveDir = moveDir;
                }
                else if (dirNum == 2 && !moveDir)
                {
                    while (Mathf.Approximately(currentY, endBuildPos.y) || (direction < 0 && currentY >= endBuildPos.y))
                    {
                        Vector3 position = new Vector3(startBuildPos.x, currentY, 0);
                        PosListContainCheck(position);
                        currentY += objHeight * direction;
                    }
                    tempMoveDir = moveDir;
                }
            }
            else if (!isUnderObj && isGetDir && (dirNum == 1 || dirNum == 3))
            {
                float currentX = startBuildPos.x;
                while (Mathf.Approximately(currentX, endBuildPos.x) || (direction > 0 && currentX <= endBuildPos.x) || (direction < 0 && currentX >= endBuildPos.x))
                {
                    Vector3 position = new Vector3(currentX, startBuildPos.y, 0);
                    PosListContainCheck(position);
                    currentX += objWidth * direction;
                }
                tempMoveDir = moveDir;
            }
            else if (isUnderObj && isGetDir && (dirNum == 1 || dirNum == 3))
            {
                Vector3 position = new Vector3(startBuildPos.x, startBuildPos.y, 0);
                PosListContainCheck(position);
                tempMoveDir = moveDir;
            }
        }
        nonNetObj.SetActive(false);
    }

    protected void PosListContainCheck(Vector3 pos)
    {
        if (!posList.Contains(pos))
        {
            posList.Add(pos);
            CreateObj(pos);
        }
    }

    protected bool GroupBuildCheck(GameObject obj, Vector2 pos)
    {
        PreBuildingImg preBuildingImg = obj.GetComponent<PreBuildingImg>();
        if (preBuildingImg.buildingPosUnit.Count == 0 && CellCheck(obj, pos))
            return true;
        else
            return false;
    }

    protected void SetBuilding(Vector3 spawnPos, int buildingIndex, bool isInHostMap, int level, int dirNum, int objHeight, int objWidth, bool isPortalObj, bool isUnderObj, bool isUnderBelt, bool sendObjCheck)
    {
        if (isPortalObj)
        {
            ServerPortalObjBuildConfirmServerRpc(spawnPos, buildingIndex, isInHostMap, level, dirNum, objHeight, objWidth, isInHostMap ? 0 : 1);
        }
        else
        {
            if (!isUnderObj)
            {
                ServerBuildConfirmServerRpc(spawnPos, buildingIndex, isInHostMap, level, dirNum, objHeight, objWidth);
            }
            else
            {
                ServerUnderObjBuildConfirmServerRpc(spawnPos, buildingIndex, isInHostMap, level, dirNum, objHeight, objWidth, sendObjCheck, isUnderBelt);
            }
        }
    }

    //void SetBuilding(GameObject obj, Vector3 spawnPos)
    //{
    //    if (isPortalObj)
    //    {
    //        ServerPortalObjBuildConfirmServerRpc(spawnPos, setPos, buildingIndex, isInHostMap, level, dirNum, objHeight, objWidth, portalIndex);
    //    }
    //    else
    //    {
    //        if (BuildingInfo.instance.AmountsEnoughCheck())
    //        {
    //            if (!isUnderObj)
    //            {
    //                ServerBuildConfirmServerRpc(spawnPos, setPos, buildingIndex, isInHostMap, level, dirNum, objHeight, objWidth);
    //            }
    //            else
    //            {
    //                spawnPos = obj.transform.position;
    //                UnderObjBuilding underObj = obj.GetComponent<UnderObjBuilding>();
    //                bool sendObjCheck = underObj.isSendObj;
    //                int newDir = underObj.dirNum;
    //                ServerUnderObjBuildConfirmServerRpc(spawnPos, setPos, buildingIndex, isInHostMap, level, newDir, objHeight, objWidth, sendObjCheck, isUnderBelt);
    //            }
    //        }
    //    }
    //    setBuild = null;
    //}

    [ServerRpc(RequireOwnership = false)]
    public void ServerBuildConfirmServerRpc(Vector3 spawnPos, int buildingIndex, bool isInHostMap, int level, int dirNum, int objHeight, int objWidth)
    {
        //bool notFound = ReCheck(spawnPos, objHeight, objWidth, buildingIndex, level, isInHostMap);
        //if (!notFound)
        //    return;

        GameObject prefabObj = buildingListSO.FindBuildingListObj(buildingIndex);
        GameObject spawnobj = Instantiate(prefabObj, spawnPos, Quaternion.identity);
        spawnobj.TryGetComponent(out NetworkObject netObj);
        if (!netObj.IsSpawned) spawnobj.GetComponent<NetworkObject>().Spawn(true);

        if (netObj.TryGetComponent(out Structure structure))
        {
            if(netObj.GetComponent<BeltCtrl>())
            {
                BeltGroupMgr beltGroupMgr = beltGroupSet.GetComponent<BeltGroupMgr>();
                beltGroupMgr.SetBelt(spawnobj, level, dirNum, objHeight, objWidth, isInHostMap, this.buildingIndex);
                MapDataCheck(spawnobj, spawnPos);
            }
            else
            {
                structure.SettingClientRpc(level, dirNum, objHeight, objWidth, isInHostMap, this.buildingIndex);
                MapDataCheck(spawnobj, spawnPos);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerUnderObjBuildConfirmServerRpc(Vector3 spawnPos, int buildingIndex, bool isInHostMap, int level, int dirNum, int objHeight, int objWidth, bool isSend, bool isUnderBelt)
    {
        //bool notFound = ReCheck(spawnPos, objHeight, objWidth, buildingIndex, level, isInHostMap);
        //if (!notFound)
        //    return;

        GameObject prefabObj;
        GameObject spawnobj;
         
        if(isUnderBelt && !isSend)
            prefabObj = buildingListSO.FindSideBuildingListObj(buildingIndex);
        else
            prefabObj = buildingListSO.FindBuildingListObj(buildingIndex);

        spawnobj = Instantiate(prefabObj, spawnPos, Quaternion.identity);
        spawnobj.TryGetComponent(out NetworkObject netObj);
        if (!netObj.IsSpawned) spawnobj.GetComponent<NetworkObject>().Spawn(true);
        if (netObj.TryGetComponent(out Structure structure))
        {
            structure.SettingClientRpc(level, dirNum, objHeight, objWidth, isInHostMap, buildingIndex);
        }
        MapDataCheck(spawnobj, spawnPos);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerPortalObjBuildConfirmServerRpc(Vector3 spawnPos, int buildingIndex, bool isInHostMap, int level, int dirNum, int objHeight, int objWidth, int portalIndex)
    {
        //bool notFound = ReCheck(spawnPos, objHeight, objWidth, buildingIndex, level, isInHostMap);
        //if (!notFound)
        //    return;

        GameObject prefabObj = buildingListSO.FindBuildingListObj(buildingIndex);

        //GameObject spawnobj = Instantiate(prefabObj, spawnPos - setPos, Quaternion.identity);
        GameObject spawnobj = Instantiate(prefabObj, spawnPos , Quaternion.identity);
        spawnobj.TryGetComponent(out NetworkObject netObj);
        if (!netObj.IsSpawned) spawnobj.GetComponent<NetworkObject>().Spawn(true);

        if (netObj.TryGetComponent(out PortalObj portal))
        {
            portal.SettingClientRpc(level, dirNum, objHeight, objWidth, isInHostMap, this.buildingIndex);
            gameManager.portal[portalIndex].SetPortalObjEnd(portal.structureData.FactoryName, spawnobj);
            spawnobj.transform.parent = gameManager.portal[portalIndex].transform;
            MapDataCheck(spawnobj, spawnPos);
        }
    }

    //bool ReCheck(Vector3 spawnPos, int objHeight, int objWidth, int itemIndex, int level, bool isHostMap)
    //{
    //    int x = Mathf.FloorToInt(spawnPos.x);
    //    int y = Mathf.FloorToInt(spawnPos.y);

    //    List<int> xList = new List<int>();
    //    List<int> yList = new List<int>();

    //    if (objHeight == 1 && objWidth == 1)
    //    {
    //        xList.Add(x);
    //        yList.Add(y);
    //    }
    //    else if (objHeight == 2 && objWidth == 2)
    //    {
    //        xList.Add(x);
    //        xList.Add(x + 1);
    //        yList.Add(y);
    //        yList.Add(y + 1);
    //    }

    //    foreach (int newX in xList)
    //    {
    //        foreach (int newY in yList)
    //        {
    //            Cell cell;

    //            if (isHostMap)
    //            {
    //                cell = gameManager.hostMap.GetCellDataFromPos(newX, newY);
    //            }
    //            else
    //            {
    //                cell = gameManager.clientMap.GetCellDataFromPos(newX, newY);
    //            }

    //            if (cell.structure)
    //            {
    //                Debug.Log("already");
    //                GameObject prefabObj = buildingListSO.FindBuildingListObj(itemIndex);
    //                string name = prefabObj.GetComponent<Structure>().structureData.FactoryName;
    //                Debug.Log(name + " : " + level);
    //                BuildingData data = BuildingDataGet.instance.GetBuildingName(name, level + 1);
    //                Debug.Log(data.items.Count);

    //                for (int i = 0; i < data.GetItemCount(); i++)
    //                {
    //                    GameManager.instance.inventory.Add(ItemList.instance.itemDic[data.items[i]], data.amounts[i]);
    //                }

    //                return false;
    //            }
    //        }
    //    }

    //    return true;
    //}

    protected void MapDataCheck(GameObject obj, Vector2 pos)
    {
        obj.GetComponent<Structure>().MapDataSaveClientRpc(pos);   
    }

    protected void CreateObj(Vector3 pos)
    {
        if (isNeedSetPos)
        {
            AddBuildingToList(pos - setPos);
        }
        else
        {
            AddBuildingToList(pos);
            //if (!isUnderObj)
            //{
            //    AddBuildingToList(pos);
            //}
            //else if (isUnderObj)
            //{
            //UnderObjBuilding underObj = nonNetObj.GetComponent<UnderObjBuilding>();
            //if (buildingList.Count == 0)
            //{
            //    AddBuildingToList(pos);
            //    if (underObj != null)
            //    {
            //        isPreObjSend = underObj.isSendObj;
            //    }
            //}
            //else if (buildingList.Count > 0)
            //{
            //    if (!isPreObjSend)
            //    {
            //        if (buildingList.Count == 1)
            //        {
            //            AddBuildingToList(pos);
            //            isPreObjSend = !isPreObjSend;
            //        }
            //        else if (Vector3.Distance(pos, buildingList[buildingList.Count - 2].transform.position) >= 11)
            //        {
            //            AddBuildingToList(pos);
            //            isPreObjSend = !isPreObjSend;
            //        }
            //        else if (Vector3.Distance(pos, buildingList[buildingList.Count - 2].transform.position) < 11)
            //        {
            //            Destroy(buildingList[buildingList.Count - 1]);
            //            buildingList.RemoveAt(buildingList.Count - 1);
            //            AddBuildingToList(pos);
            //        }
            //    }
            //    else
            //    {
            //        if (buildingList.Count < canBuildCount)
            //        {
            //            AddBuildingToList(pos);
            //            isPreObjSend = !isPreObjSend;
            //        }
            //    }
            //}
            //}
        }
    }

    protected void AddBuildingToList(Vector3 pos)
    {
        if (buildingList.Count < canBuildCount)
        {
            GameObject obj = Instantiate(nonNetObj, pos, Quaternion.identity);
            obj.TryGetComponent(out PreBuildingImg preBuildingImg);
            obj.SetActive(true);

            if (isUnderObj)
            {
                preBuildingImg.GetComponent<UnderObjBuilding>().Setting(dirNum, isUnderBelt, buildData.sprites);
            }

            if (isGetAnim)
            {
                preBuildingImg.PreAnimatorSet(buildData.animatorController[0]);
                if (isGetDir)
                {
                    preBuildingImg.AnimSetFloat("DirNum", dirNum);
                    if (isBeltObj)
                        preBuildingImg.AnimSetFloat("Level", level);
                }
            }
            else
            {
                spriteList = buildData.sprites;
                preBuildingImg.PreSpriteSet(spriteList[dirNum]);
            }

            buildingList.Add(obj);
        }
    }

    public void SetImage(Building build, int _canBuildAmount, bool _isInHostMap)
    {
        isBuildingOn = true;
        buildData = build;
        buildingIndex = buildingListSO.FindBuildingListIndex(buildData.name);
        level = buildData.level - 1;
        int height = buildData.height;
        int width = buildData.width;
        if (buildData.gameObj.GetComponent<BeltCtrl>())
            isBeltObj = true;
        isPortalObj = false;
        canBuildCount = _canBuildAmount;
        isInHostMap = _isInHostMap;
        if (nonNetObj != null)
        {
            Destroy(nonNetObj);
        }

        nonNetObj = Instantiate(preBuildingNonNet);

        objHeight = height;
        objWidth = width;

        nonNetObj.TryGetComponent(out PreBuildingImg preBuildingImg);
        preBuildingImg.BoxColliderSet(new Vector2(height, width));

        isGetAnim = buildData.isGetAnim;


        if (buildData.isGetDirection)
        {
            isGetDir = true;
            MouseSkin.instance.BuildingCursorSet(1);
            BasicUIBtns.instance.SetRotateUI(true);
        }
        else
        {
            isGetDir = false;
            MouseSkin.instance.BuildingCursorSet(0);
        }

        if (buildData.isUnderObj)
            isUnderObj = true;
        else
            isUnderObj = false;

        canNotDrag = buildData.dragCancel;

        if (isGetAnim)
        {
            preBuildingImg.PreAnimatorSet(buildData.animatorController[0]);
            if (isGetDir)
            {
                preBuildingImg.AnimSetFloat("DirNum", dirNum);
                if (isBeltObj)
                    preBuildingImg.AnimSetFloat("Level", level);
            }
        }
        else
        {
            spriteList = buildData.sprites;
            preBuildingImg.PreSpriteSet(spriteList[0]);
        }

        if (buildData.item.name.Contains("Tower"))
        {
            nonNetObj.transform.localScale = new Vector3(1.25f, 1.25f, 1);
        }
        else
        {
            nonNetObj.transform.localScale = new Vector3(1, 1, 1);
        }

        isUnderBelt = false;

        if (isUnderObj)
        {
            nonNetObj.AddComponent<UnderObjBuilding>();

            if (buildData.item.name == "UnderBelt")
                isUnderBelt = true;

            UnderObjBuilding underObj = nonNetObj.GetComponent<UnderObjBuilding>();
            underObj.Setting(dirNum, isUnderBelt, buildData.sprites);
            if(isUnderBelt)
                underObj.lineObj = lineObj;
            else
                underObj.lineObj = fluidLineObj;
        }

        GameObject prefabObj = buildingListSO.FindBuildingListObj(buildingIndex);
        if (prefabObj.GetComponentInChildren<Structure>())
        {
            isEnergyStr = prefabObj.GetComponentInChildren<Structure>().structureData.IsEnergyStr;
            isEnergyUse = prefabObj.GetComponentInChildren<Structure>().structureData.EnergyUse[level];

            preBuildingImg.PreStrSet(prefabObj.GetComponentInChildren<Structure>());
            preBuildingImg.EnergyUseCheck(isEnergyUse);

            if (isEnergyStr && !prefabObj.GetComponentInChildren<EnergyBattery>())
            {
                if (prefabObj.GetComponentInChildren<EnergyRepeater>() && prefabObj.GetComponentInChildren<EnergyRepeater>().isImprovedRepeater)
                    preBuildingImg.TerritoryViewSet(0);
                else
                    preBuildingImg.TerritoryViewSet(1);
            }
            else if (prefabObj.GetComponentInChildren<Overclock>())
            {
                preBuildingImg.TerritoryViewSet(2);
            }
            else if (prefabObj.GetComponentInChildren<RepairTower>())
            {
                preBuildingImg.TerritoryViewSet(3);
            }
            else if (prefabObj.GetComponentInChildren<SunTower>())
            {
                preBuildingImg.TerritoryViewSet(4);
            }
        }
        else
        {
            isEnergyStr = false;
            isEnergyUse = false;
        }

        if (height == 1 && width == 1)
        {
            setPos = Vector3.zero;
            isNeedSetPos = false;
            nonNetObj.transform.position = this.transform.position;
        }
        else if (height == 2 && width == 2)
        {
            setPos = new Vector3(-0.5f, -0.5f);
            isNeedSetPos = true;
            nonNetObj.transform.position = this.transform.position - setPos;
        }

        if (nonNetObj.GetComponentInChildren<SpriteRenderer>() != null)
        {
            spriteRenderer = nonNetObj.GetComponentInChildren<SpriteRenderer>();
            layNumTemp = spriteRenderer.sortingOrder;
        }

        startBuildPos = transform.position;
        endBuildPos = transform.position;
    }

    public void SetPortalImage(Building build, Portal portal, bool _isInHostMap, bool isPortalObjs)
    {
        isBuildingOn = true;
        buildData = build;
        buildingIndex = buildingListSO.FindBuildingListIndex(buildData.name);
        int height = 2;
        int width = 2;
        isPortalObj = true;
        isScienceBuilding = !isPortalObjs;
        canBuildCount = 1;
        portalScript = portal;
        isInHostMap = _isInHostMap;

        if (gameManager.portal[0] == portal)        
            portalIndex = 0;
        else
            portalIndex = 1;

        if (nonNetObj != null)
        {
            Destroy(nonNetObj);
        }

        nonNetObj = Instantiate(preBuildingNonNet);

        objHeight = height;
        objWidth = width;

        nonNetObj.TryGetComponent(out PreBuildingImg preBuildingImg);
        preBuildingImg.BoxColliderSet(new Vector2(height, width));

        isGetAnim = buildData.isGetAnim;

        GameObject prefabObj = buildingListSO.FindBuildingListObj(buildingIndex);
        preBuildingImg.PreStrSet(prefabObj.GetComponentInChildren<Structure>());
        preBuildingImg.PreAnimatorSet(buildData.animatorController[0]);        

        isGetDir = false;

        MouseSkin.instance.BuildingCursorSet(0);

        isEnergyStr = false;
        isEnergyUse = false;

        canNotDrag = buildData.dragCancel;

        setPos = new Vector3(-0.5f, -0.5f);
        isNeedSetPos = true;
        nonNetObj.transform.position = this.transform.position - setPos;        

        if (nonNetObj.GetComponentInChildren<SpriteRenderer>() != null)
        {
            spriteRenderer = nonNetObj.GetComponentInChildren<SpriteRenderer>();
            layNumTemp = spriteRenderer.sortingOrder;
        }

        startBuildPos = transform.position;
        endBuildPos = transform.position;
    }
    protected void SetColor(SpriteRenderer sprite, Color color, float alpha)
    {
        Color slotColor = sprite.color;
        slotColor = color;
        slotColor.a = alpha;
        sprite.color = slotColor;
    }

    protected void BuildingListSetColor()
    {
        if(buildingList.Count > 0)
        {
            int posIndex = 0;
            foreach (GameObject obj in buildingList)
            {
                bool canBuilding = false;
                Color colorRed = Color.red;
                Color colorGreen = Color.green;
                float alpha = 0.35f;

                PreBuildingImg preBuildingImg = obj.GetComponent<PreBuildingImg>();
                canBuilding = preBuildingImg.buildingPosUnit.Count == 0 ? true : false;
                if (canBuilding && CellCheck(obj, posList[posIndex]))
                {
                    SetColor(obj.GetComponentInChildren<SpriteRenderer>(), colorGreen, alpha);
                }
                else
                {
                    SetColor(obj.GetComponentInChildren<SpriteRenderer>(), colorRed, alpha);
                }
                posIndex++;
            }
        }
        else
        {
            if(nonNetObj != null)
            {
                bool canBuilding = false;
                PreBuildingImg preBuildingImg = nonNetObj.GetComponent<PreBuildingImg>();
                canBuilding = preBuildingImg.buildingPosUnit.Count == 0 ? true : false;
                if (canBuilding && isEnough && GroupBuildCheck(nonNetObj, mousePos))
                {
                    SetColor(spriteRenderer, Color.green, 0.35f);
                }
                else if (!canBuilding || !isEnough || !GroupBuildCheck(nonNetObj, mousePos))
                {
                    SetColor(spriteRenderer, Color.red, 0.35f);
                }
            }
        }
    }

    protected bool CellCheck(GameObject obj, Vector2 pos)
    {
        if (isUnderObj)
            pos = obj.transform.position;

        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);

        List<int> xList = new List<int>();
        List<int> yList = new List<int>();

        if (objHeight == 1 && objWidth == 1)
        {
            xList.Add(x);
            yList.Add(y);
        }
        else if (objHeight == 2 && objWidth == 2)
        {
            xList.Add(x);
            xList.Add(x + 1);
            yList.Add(y);
            yList.Add(y + 1);
        }

        bool canBuild = false;

        if (DistCheck(obj.transform.position))
        {
            if (isPortalObj)
            {
                foreach (int newX in xList)
                {
                    foreach (int newY in yList)
                    {
                        if (gameManager.portal[portalIndex].PortalObjFind(buildData.item.name))
                        {
                            return false;
                        }

                        if (!gameManager.map.IsOnMap(newX, newY))
                        {
                            continue;
                        }

                        Cell cell = gameManager.map.GetCellDataFromPos(newX, newY);
                        if (cell.obj != null)
                        {
                            return false;
                        }

                        if (!isScienceBuilding && !cell.BuildCheck("PortalObj"))
                        {
                            return false;
                        }
                        else if (isScienceBuilding && !cell.BuildCheck("ScienceBuilding"))
                        {
                            return false;
                        }
                        else
                            canBuild = true;
                    }
                }
            }
            else
            {
                foreach (int newX in xList)
                {
                    foreach (int newY in yList)
                    {
                        if (!gameManager.map.IsOnMap(newX, newY))
                        {
                            continue;
                        }

                        Cell cell = gameManager.map.GetCellDataFromPos(newX, newY);
                        if (cell.structure != null || cell.obj != null || (cell.corruptionId > 0))
                        {
                            return false;
                        }

                        Miner miner = null;
                        PumpCtrl pump = null;
                        ExtractorCtrl extractor = null;
                        if (buildData.gameObj.TryGetComponent(out miner) || buildData.gameObj.TryGetComponent(out pump) || buildData.gameObj.TryGetComponent(out extractor))
                        {
                            if ((miner && cell.BuildCheck("miner") &&
                                level + 1 >= cell.resource.level) ||
                                (pump && cell.BuildCheck("pump")) ||
                                (extractor && cell.BuildCheck("extractor")))
                            {
                                if (!canBuild)
                                    canBuild = true;
                            }
                        }
                        else
                        {
                            if (((cell.buildable.Count == 0 || cell.BuildCheck("miner")) && cell.biome.biome != "cliff") && cell.structure == null)
                            {
                                canBuild = true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                }
            }
        }
        return canBuild;
    }

    protected bool DistCheck(Vector3 pos)
    {
        bool canBuild = false;

        Vector3 playerPos = gameManager.playerController.gameObject.transform.position;
        playerPos = new Vector3(playerPos.x, playerPos.y + 1, 0);
        float dist = Vector3.Distance(pos, playerPos);

        if (dist < maxBuildDist)
        {
            canBuild = true;
        }

        return canBuild;
    }

    public void ReSetImage()
    {
        if (nonNetObj != null)
        {
            Destroy(nonNetObj);
        }
        if(spriteRenderer)
            spriteRenderer.sprite = null;
        dirNum = 0;
        nonNetObj = null;
        BasicUIBtns.instance.SetRotateUI(false);
    }


    protected void RotationImg(GameObject obj)
    {
        if (!isGetDir)
            return;

        PreBuildingImg preBuildingImg = obj.GetComponent<PreBuildingImg>();

        dirNum++;
        if (dirNum >= 4)
            dirNum = 0;

        if (isGetAnim)
        {
            if (isGetDir)
            {
                preBuildingImg.AnimSetFloat("DirNum", dirNum);
                if (isBeltObj)
                    preBuildingImg.AnimSetFloat("Level", level);
            }
        }
        else
        {
            preBuildingImg.PreSpriteSet(spriteList[dirNum]);
        }

        if(obj.TryGetComponent(out UnderObjBuilding under))
        {
            under.dirNum = dirNum;
        }
    }
}
