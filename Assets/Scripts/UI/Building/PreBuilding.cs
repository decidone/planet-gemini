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
    SpriteRenderer spriteRenderer;
    GameObject nonNetObj;
    public GameObject beltGroupSet;

    public GameObject beltMgr;
    public GameObject beltGroup;

    bool isNeedSetPos = false;
    Vector3 setPos;

    private Tilemap tilemap;

    [HideInInspector]
    public bool isBuildingOk = true;
    int layNumTemp = 0;

    int objHeight = 1;
    int objWidth = 1;
    bool isGetDir = false;
    int level;
    int dirNum = 0;

    Vector3 startBuildPos;
    Vector3 endBuildPos;

    bool isUnderObj = false;
    [HideInInspector]
    public bool isEnough = true;
    int canBuildCount;

    List<GameObject> buildingList = new List<GameObject>();
    Vector3 tempPos;
    public List<Vector3> posList = new List<Vector3>();
    bool isMoveX = true; 
    bool tempMoveX;
    bool moveDir;
    bool tempMoveDir;
    public bool isPreObjSend;
    Vector3 mousePos;
    bool isDrag = false;
    Coroutine setBuild;

    //bool isTempBuild;
    bool mouseHoldCheck;   //기존 isLeftMouse기능 대체+a 역할이라 실제 hold감지는 InputManager의 hold를 사용

    GameManager gameManager;
    InputManager inputManager;

    [SerializeField]
    float maxBuildDist;

    bool isBeltObj = false;
    bool reversSet = false;
    bool isPortalObj = false;
    bool isScienceBuilding = false;
    Portal portalScript;
    int portalIndex;

    BuildingInvenManager buildingInven;

    SoundManager soundManager;

    BuildingList buildingListSO;
    int buildingIndex;

    public bool isBuildingOn = false;
    public GameObject preBuildingNonNet;
    List<Sprite> spriteList = new List<Sprite>();
    bool isGetAnim;
    Building buildData;

    public bool isEnergyUse;
    public bool isEnergyStr;

    bool isUnderBelt;

    bool isInHostMap;

    #region Singleton
    public static PreBuilding instance;

    void Awake()
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
        mouseHoldCheck = false;
        tilemap = GameObject.Find("Tilemap").GetComponent<Tilemap>();

        gameManager = GameManager.instance;
        buildingInven = BuildingInvenManager.instance;
        soundManager = SoundManager.instance;
        buildingListSO = BuildingList.instance;
    }
    void OnEnable()
    {
        inputManager = InputManager.instance;
        inputManager.controls.Building.LeftMouseButtonDown.performed += LeftMouseButtonDown;
        inputManager.controls.Building.LeftMouseButtonUp.performed += LeftMouseButtonUpCommand;
        inputManager.controls.Building.RightMouseButtonDown.performed += CancelBuild;
        inputManager.controls.Building.Rotate.performed += Rotate;
    }
    void OnDisable()
    {
        inputManager.controls.Building.LeftMouseButtonDown.performed -= LeftMouseButtonDown;
        inputManager.controls.Building.LeftMouseButtonUp.performed -= LeftMouseButtonUpCommand;
        inputManager.controls.Building.RightMouseButtonDown.performed -= CancelBuild;
        inputManager.controls.Building.Rotate.performed -= Rotate;
    }

    void Update()
    {
        if (isBuildingOn)
        {
            mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPosition = tilemap.WorldToCell(mousePos);
            Vector3 cellCenter = tilemap.GetCellCenterWorld(cellPosition);
            cellCenter.z = transform.position.z;
            transform.position = cellCenter;
            if (nonNetObj)
                nonNetObj.transform.position = this.transform.position - setPos;

            if (spriteRenderer != null)
                BuildingListSetColor();
        }
    }

    void FixedUpdate()
    {
        if(isEnough && mouseHoldCheck && isBuildingOn)
        {
            if(BuildingInfo.instance.AmountsEnoughCheck())
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

    void LeftMouseButtonDown(InputAction.CallbackContext ctx)
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
    void LeftMouseButtonUpCommand(InputAction.CallbackContext ctx)
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

                bool canBuild = false;
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

                if (canBuild)
                {
                    int posIndex = 0;

                    if (isBeltObj)
                    {
                        BeltGroupSpawnServerRpc();

                        if (reversSet)
                        {
                            posIndex = posList.Count - 1;
                            for (int i = buildingList.Count - 1; i >= 0; i--)
                            {
                                GameObject obj = buildingList[i];
                                SetBuilding(obj, posList[posIndex]);
                                posIndex--;
                            }
                        }
                        else
                        {
                            for (int i = 0; i < buildingList.Count; i++)
                            {
                                GameObject obj = buildingList[i];
                                SetBuilding(obj, posList[posIndex]);
                                posIndex++;
                            }
                        }

                        BeltGroupSetEndServerRpc();
                    }
                    else
                    {
                        foreach (GameObject obj in buildingList)
                        {
                            SetBuilding(obj, posList[posIndex]); 
                            posIndex++;
                        }
                    }
           
                    if (isPortalObj)
                    {
                        //portalScript.SetPortalObjEnd(buildData.name, buildingList[0]);
                        isEnough = false;
                        canBuildCount = 0;
                    }
                    else
                    {
                        BuildingInfo.instance.BuildingEnd(buildingList.Count);
                        isEnough = BuildingInfo.instance.AmountsEnoughCheck();
                        canBuildCount = BuildingInfo.instance.CanBuildAmount();
                    }
                    foreach (GameObject obj in buildingList)
                    {
                        Destroy(obj);
                    }
                    soundManager.PlayUISFX("BuildingOk");
                }
                else
                {
                    soundManager.PlayUISFX("BuildingCancel");
                    foreach (GameObject build in buildingList)
                    {
                        Destroy(build);
                    }
                }

                buildingList.Clear();
                nonNetObj.SetActive(true);

                PreBuildingImg preBuildingImg = nonNetObj.GetComponent<PreBuildingImg>();

                if (isGetAnim)
                {
                    if (isGetDir)
                        preBuildingImg.AnimSetFloat("DirNum", dirNum);
                }
                else
                {
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
    void BeltGroupSpawnServerRpc()
    {
        beltGroupSet = Instantiate(beltGroup);
        beltGroupSet.TryGetComponent(out NetworkObject netObj);
        if (!netObj.IsSpawned) beltGroupSet.GetComponent<NetworkObject>().Spawn(true);
        beltGroupSet.transform.parent = beltMgr.transform;
        BeltGroupSpawnClientRpc(NetworkObjManager.instance.FindNetObjID(beltGroupSet));
    }

    [ClientRpc]
    void BeltGroupSpawnClientRpc(ulong Id)
    {
        beltGroupSet = NetworkObjManager.instance.FindNetworkObj(Id).gameObject;
    }

    [ServerRpc(RequireOwnership = false)]
    void BeltGroupSetEndServerRpc()
    {
        BeltGroupSetEndClientRpc();
    }


    [ClientRpc]
    void BeltGroupSetEndClientRpc()
    {
        if (!IsServer)
            return;

        if (beltGroupSet.TryGetComponent(out BeltGroupMgr beltGroupMgr))
        {
            beltGroupMgr.SetBeltData();
            beltGroupMgr.isSetBuildingOk = true;
        }
    }

    public void CancelBuild(InputAction.CallbackContext ctx)
    {
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
    }

    void Rotate(InputAction.CallbackContext ctx)
    {
        if (nonNetObj != null && !inputManager.mouseLeft)
        {
            RotationImg(nonNetObj);
        }
    }

    void CheckPos()
    {
        if(posList.Count > 0)
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
            else if((!isGetDir && !isMoveX) || (isGetDir && (dirNum == 0 || dirNum == 2)))
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
                                if(tempPos.y > buildingList[b].transform.position.y)
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

    void SetPos()
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

    void PosListContainCheck(Vector3 pos)
    {
        if (!posList.Contains(pos))
        {
            posList.Add(pos);
            CreateObj(pos);
        }
    }

    bool GroupBuildCheck(GameObject obj, Vector2 pos)
    {
        PreBuildingImg preBuildingImg = obj.GetComponent<PreBuildingImg>();

        if (preBuildingImg.canBuilding && CellCheck(obj, pos))
            return true;
        else
            return false;
    }

    void SetBuilding(GameObject obj, Vector3 spawnPos)
    {
        if (isPortalObj)
        {
            ServerPortalObjBuildConfirmServerRpc(spawnPos, setPos, buildingIndex, isInHostMap, level, dirNum, objHeight, objWidth, portalIndex, buildingIndex);
        }
        else
        {
            if (BuildingInfo.instance.AmountsEnoughCheck())
            {
                if (!isUnderObj)
                {
                    ServerBuildConfirmServerRpc(spawnPos, setPos, buildingIndex, isInHostMap, level, dirNum, objHeight, objWidth, buildingIndex);
                }
                else
                {
                    spawnPos = obj.transform.position;
                    UnderObjBuilding underObj = obj.GetComponent<UnderObjBuilding>();
                    bool sendObjCheck = underObj.isSendObj;
                    int newDir = underObj.dirNum;
                    ServerUnderObjBuildConfirmServerRpc(spawnPos, setPos, buildingIndex, isInHostMap, level, newDir, objHeight, objWidth, sendObjCheck, isUnderBelt, buildingIndex);
                }
            }
        }
        setBuild = null;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerBuildConfirmServerRpc(Vector3 spawnPos, Vector3 setPos, int itemIndex, bool isInHostMap, int level, int dirNum, int objHeight, int objWidth, int buildingIndex)
    {
        GameObject prefabObj = buildingListSO.FindBuildingListObj(itemIndex);

        GameObject spawnobj = Instantiate(prefabObj, spawnPos - setPos, Quaternion.identity);
        spawnobj.TryGetComponent(out NetworkObject netObj);
        if (!netObj.IsSpawned) spawnobj.GetComponent<NetworkObject>().Spawn(true);

        if (netObj.TryGetComponent(out Structure structure))
        {
            if(netObj.GetComponent<BeltCtrl>())
            {
                BeltGroupMgr beltGroupMgr = beltGroupSet.GetComponent<BeltGroupMgr>();
                beltGroupMgr.SetBelt(spawnobj, level, dirNum, objHeight, objWidth, isInHostMap, buildingIndex);
                MapDataCheck(spawnobj, spawnPos);
            }
            else
            {
                structure.SettingClientRpc(level, dirNum, objHeight, objWidth, isInHostMap, buildingIndex);
                MapDataCheck(spawnobj, spawnPos);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerUnderObjBuildConfirmServerRpc(Vector3 spawnPos, Vector3 setPos, int itemIndex, bool isInHostMap, int level, int dirNum, int objHeight, int objWidth, bool isSend, bool isUnderBelt, int buildingIndex)
    {
        GameObject prefabObj;
        GameObject spawnobj;
         
        if(isUnderBelt && !isSend)
            prefabObj = buildingListSO.FindSideBuildingListObj(itemIndex);
        else
            prefabObj = buildingListSO.FindBuildingListObj(itemIndex);

        spawnobj = Instantiate(prefabObj, spawnPos - setPos, Quaternion.identity);
        spawnobj.TryGetComponent(out NetworkObject netObj);
        if (!netObj.IsSpawned) spawnobj.GetComponent<NetworkObject>().Spawn(true);
        if (netObj.TryGetComponent(out Structure structure))
        {
            structure.SettingClientRpc(level, dirNum, objHeight, objWidth, isInHostMap, buildingIndex);
        }
        MapDataCheck(spawnobj, spawnPos);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerPortalObjBuildConfirmServerRpc(Vector3 spawnPos, Vector3 setPos, int itemIndex, bool isInHostMap, int level, int dirNum, int objHeight, int objWidth, int portalIndex, int buildingIndex)
    {
        GameObject prefabObj = buildingListSO.FindBuildingListObj(itemIndex);

        GameObject spawnobj = Instantiate(prefabObj, spawnPos - setPos, Quaternion.identity);
        spawnobj.TryGetComponent(out NetworkObject netObj);
        if (!netObj.IsSpawned) spawnobj.GetComponent<NetworkObject>().Spawn(true);

        if (netObj.TryGetComponent(out PortalObj portal))
        {
            portal.SettingClientRpc(level, dirNum, objHeight, objWidth, isInHostMap, buildingIndex);
            gameManager.portal[portalIndex].SetPortalObjEnd(portal.structureData.FactoryName, spawnobj);
            spawnobj.transform.parent = gameManager.portal[portalIndex].transform;
            MapDataCheck(spawnobj, spawnPos);
        }
    }

    void MapDataCheck(GameObject obj, Vector2 pos)
    {
        obj.GetComponent<Structure>().MapDataSaveClientRpc(pos);   
    }

    void CreateObj(Vector3 pos)
    {
        if (isNeedSetPos)
        {
            AddBuildingToList(pos - setPos);
        }
        else
        {
            if (!isUnderObj)
            {
                AddBuildingToList(pos);
            }
            else if (isUnderObj)
            {
                UnderObjBuilding underObj = nonNetObj.GetComponent<UnderObjBuilding>();
                if (buildingList.Count == 0)
                {
                    AddBuildingToList(pos);
                    if (underObj != null)
                    {
                        isPreObjSend = underObj.isSendObj;
                    }
                }
                else if (buildingList.Count > 0)
                {
                    if (!isPreObjSend)
                    {
                        if (buildingList.Count == 1)
                        {
                            AddBuildingToList(pos);
                            isPreObjSend = !isPreObjSend;
                        }
                        else if (Vector3.Distance(pos, buildingList[buildingList.Count - 2].transform.position) >= 11)
                        {
                            AddBuildingToList(pos);
                            isPreObjSend = !isPreObjSend;
                        }
                        else if (Vector3.Distance(pos, buildingList[buildingList.Count - 2].transform.position) < 11)
                        {
                            Destroy(buildingList[buildingList.Count - 1]);
                            buildingList.RemoveAt(buildingList.Count - 1);
                            AddBuildingToList(pos);
                        }
                    }
                    else
                    {
                        if (buildingList.Count < canBuildCount)
                        {
                            AddBuildingToList(pos);
                            isPreObjSend = !isPreObjSend;
                        }
                    }
                }
            }
        }
    }

    void AddBuildingToList(Vector3 pos)
    {
        if(buildingList.Count < canBuildCount)
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
                    preBuildingImg.AnimSetFloat("DirNum", dirNum);
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

        isGetAnim = buildData.isGetAnim;

        if (isGetAnim)
        {
            preBuildingImg.PreAnimatorSet(buildData.animatorController[0]);
        }
        else
        {
            spriteList = buildData.sprites;
            preBuildingImg.PreSpriteSet(spriteList[0]);
        }

        if (buildData.isGetDirection)
            isGetDir = true;
        else
            isGetDir = false;

        if (buildData.isUnderObj)
            isUnderObj = true;
        else
            isUnderObj = false;

        if (buildData.item.name.Contains("Tower"))
        {
            nonNetObj.transform.localScale = new Vector3(1.25f, 1.25f, 1);
        }
        else
        {
            nonNetObj.transform.localScale = new Vector3(1, 1, 1);
        }

        if (isUnderObj)
        {
            nonNetObj.AddComponent<UnderObjBuilding>();
            if (buildData.item.name == "UnderBelt")
                isUnderBelt = true;
            else
                isUnderBelt = false;

            nonNetObj.GetComponent<UnderObjBuilding>().Setting(dirNum, isUnderBelt, buildData.sprites);
        }

        GameObject prefabObj = buildingListSO.FindBuildingListObj(buildingIndex);
        if (prefabObj.GetComponentInChildren<Structure>())
        {
            isEnergyStr = prefabObj.GetComponentInChildren<Structure>().structureData.IsEnergyStr;
            isEnergyUse = prefabObj.GetComponentInChildren<Structure>().structureData.EnergyUse[level];

            preBuildingImg.EnergyUseCheck(isEnergyUse);

            if (isEnergyStr && !prefabObj.GetComponentInChildren<EnergyBattery>())
            {
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

        isGetAnim = buildData.isGetAnim;

        preBuildingImg.PreAnimatorSet(buildData.animatorController[0]);        

        isGetDir = false;
        isUnderObj = false;

        isEnergyStr = false;
        isEnergyUse = false;        

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
    void SetColor(SpriteRenderer sprite, Color color, float alpha)
    {
        Color slotColor = sprite.color;
        slotColor = color;
        slotColor.a = alpha;
        sprite.color = slotColor;
    }

    void BuildingListSetColor()
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
                canBuilding = preBuildingImg.canBuilding;

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
                if (isBuildingOk && isEnough && GroupBuildCheck(nonNetObj, mousePos))                
                    SetColor(spriteRenderer, Color.green, 0.35f);                
                else if (!isBuildingOk || !isEnough || !GroupBuildCheck(nonNetObj, mousePos))
                    SetColor(spriteRenderer, Color.red, 0.35f);                
            }
        }
    }

    bool CellCheck(GameObject obj, Vector2 pos)
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
                        if (cell.structure != null || cell.obj != null || cell.isCorrupted)
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

    bool DistCheck(Vector3 pos)
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
    }


    void RotationImg(GameObject obj)
    {
        if (!isGetDir)
            return;

        PreBuildingImg preBuildingImg = obj.GetComponent<PreBuildingImg>();

        dirNum++;
        if (dirNum >= 4)
            dirNum = 0;

        if (isGetAnim)
        {
            if(isGetDir)
                preBuildingImg.AnimSetFloat("DirNum", dirNum);
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
