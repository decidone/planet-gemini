using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using Pathfinding;

// UTF-8 설정
public class PreBuilding : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    GameObject gameObj;

    public GameObject beltMgr;

    bool isNeedSetPos = false;
    Vector3 setPos;

    private Tilemap tilemap;

    [HideInInspector]
    public bool isBuildingOk = true;
    int layNumTemp = 0;

    int objHeight = 1;
    int objWidth = 1;
    bool isGetDir = false;
    int dirNum = 0;

    Vector3 startBuildPos;
    Vector3 endBuildPos;

    bool isUnderObj = false;
    [HideInInspector]
    public bool isEnough = true;

    List<GameObject> buildingList = new List<GameObject>();
    Vector3 tempPos;
    List<Vector3> posList = new List<Vector3>();
    bool isMoveX = true; 
    bool tempMoveX;
    bool moveDir;
    bool tempMoveDir;
    bool isPreBeltSend;
    Vector3 mousePos;
    bool isDrag = false;
    Coroutine setBuild;

    bool isTempBuild;
    bool mouseHoldCheck;   //기존 isLeftMouse기능 대체+a 역할이라 실제 hold감지는 InputManager의 hold를 사용

    GameManager gameManager;
    PlayerController playerController;
    InputManager inputManager;

    #region Singleton
    public static PreBuilding instance;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of PreBuilding found!");
            return;
        }

        instance = this;
    }
    #endregion

    void Start()
    {
        mouseHoldCheck = false;
        isTempBuild = false;
        tilemap = GameObject.Find("Tilemap").GetComponent<Tilemap>();

        gameManager = GameManager.instance;
        playerController = gameManager.player.GetComponent<PlayerController>();
        
        inputManager = InputManager.instance;
        inputManager.controls.Building.LeftMouseButtonDown.performed += ctx => LeftMouseButtonDown();
        inputManager.controls.Building.LeftMouseButtonUp.performed += ctx => LeftMouseButtonUp();
        inputManager.controls.Building.RightMouseButtonDown.performed += ctx => CancelBuild();
        inputManager.controls.Building.Rotate.performed += ctx => Rotate();
    }

    void Update()
    {
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = tilemap.WorldToCell(mousePos);
        Vector3 cellCenter = tilemap.GetCellCenterWorld(cellPosition);
        cellCenter.z = transform.position.z;
        transform.position = cellCenter;

        if(spriteRenderer != null)
            BuildingListSetColor();
    }

    void FixedUpdate()
    {
        if(isEnough && mouseHoldCheck && this.gameObject.activeSelf)
        {
            if((!isTempBuild && BuildingInfo.instance.AmountsEnoughCheck()) || (isTempBuild && playerController.TempMinerCountCheck()))
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

    void LeftMouseButtonDown()
    {
        startBuildPos = transform.position;
        endBuildPos = transform.position;
        if (!RaycastUtility.IsPointerOverUI(Input.mousePosition))
            mouseHoldCheck = true;
    }

    void LeftMouseButtonUp()
    {
        if (isEnough && mouseHoldCheck && this.gameObject.activeSelf)
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
                foreach (GameObject obj in buildingList)
                {
                    setBuild = StartCoroutine("SetBuilding", obj);
                    if (obj.TryGetComponent(out UnderBeltCtrl underBelt))
                    {
                        underBelt.buildEnd = true;
                    }
                    else if (obj.TryGetComponent(out UnderPipeBuild underPipe))
                    {
                        underPipe.buildEnd = true;
                    }
                    if (!isUnderObj)
                        MapDataCheck(obj, posList[posIndex]);
                    else if (isUnderObj)
                        MapDataCheck(obj, obj.transform.position);
                    posIndex++;
                }
            }
            else
            {
                foreach (GameObject build in buildingList)
                {
                    Destroy(build);
                }
            }

            buildingList.Clear();
            gameObj.SetActive(true);
            posList.Clear();
            isDrag = false;
        }

        mouseHoldCheck = false;
    }

    void CancelBuild()
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
    }

    void Rotate()
    {
        if (gameObj != null && !inputManager.mouseLeft)
        {
            RotationImg(gameObj);
        }
    }

    void MapDataCheck(GameObject obj, Vector2 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);

        GameObject buildObj = GetBuildObject(obj);

        for (int i = 0; i < objHeight; i++)
        {
            for (int j = 0; j < objWidth; j++)
            {
                gameManager.map.mapData[x + j][y + i].structure = buildObj;
            }
        }
    }

    GameObject GetBuildObject(GameObject obj)
    {
        GameObject buildObj = obj;

        if (obj.TryGetComponent(out BeltGroupMgr beltGroup))
        {
            buildObj = beltGroup.beltList[0].gameObject;
        }
        else if (obj.TryGetComponent(out UnderBeltCtrl underBeltCtrl))
        {
            buildObj = underBeltCtrl.underBelt;
        }
        else if (obj.TryGetComponent(out UnderPipeBuild underPipeBuild))
        {
            buildObj = underPipeBuild.underPipeObj;
        }

        return buildObj;
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
                            if (buildingList[buildingList.Count - 1].TryGetComponent(out UnderBeltCtrl lastBelt))
                            {
                                isPreBeltSend = lastBelt.isSendBelt;

                                if (isRemoe && buildingList[buildingList.Count - 1].transform.position != posList[posList.Count - 1])
                                {
                                    AddBuildingToList(posList[posList.Count - 1]);
                                    isPreBeltSend = !isPreBeltSend;
                                }
                            }
                            else if (buildingList[buildingList.Count - 1].TryGetComponent(out UnderPipeBuild lastPipe))
                            {
                                isPreBeltSend = lastPipe.isSendPipe;

                                if (isRemoe && buildingList[buildingList.Count - 1].transform.position != posList[posList.Count - 1])
                                {
                                    AddBuildingToList(posList[posList.Count - 1]);
                                    isPreBeltSend = !isPreBeltSend;
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
                            if (buildingList[buildingList.Count - 1].TryGetComponent(out UnderBeltCtrl lastBelt))
                            {
                                isPreBeltSend = lastBelt.isSendBelt;

                                if (isRemoe && buildingList[buildingList.Count - 1].transform.position != posList[posList.Count - 1])
                                {
                                    AddBuildingToList(posList[posList.Count - 1]);
                                    isPreBeltSend = !isPreBeltSend;
                                }
                            }
                            else if (buildingList[buildingList.Count - 1].TryGetComponent(out UnderPipeBuild lastPipe))
                            {
                                isPreBeltSend = lastPipe.isSendPipe;

                                if (isRemoe && buildingList[buildingList.Count - 1].transform.position != posList[posList.Count - 1])
                                {
                                    AddBuildingToList(posList[posList.Count - 1]);
                                    isPreBeltSend = !isPreBeltSend;
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
                            if (buildingList[buildingList.Count - 1].TryGetComponent(out UnderBeltCtrl lastBelt))
                            {
                                isPreBeltSend = lastBelt.isSendBelt;

                                if (isRemoe && buildingList[buildingList.Count - 1].transform.position != posList[posList.Count - 1])
                                {
                                    AddBuildingToList(posList[posList.Count - 1]);
                                    isPreBeltSend = !isPreBeltSend;
                                }
                            }
                            else if (buildingList[buildingList.Count - 1].TryGetComponent(out UnderPipeBuild lastPipe))
                            {
                                isPreBeltSend = lastPipe.isSendPipe;

                                if (isRemoe && buildingList[buildingList.Count - 1].transform.position != posList[posList.Count - 1])
                                {
                                    AddBuildingToList(posList[posList.Count - 1]);
                                    isPreBeltSend = !isPreBeltSend;
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
                            if (buildingList[buildingList.Count - 1].TryGetComponent(out UnderBeltCtrl lastBelt))
                            {
                                isPreBeltSend = lastBelt.isSendBelt;
                                if (isRemoe && buildingList[buildingList.Count - 1].transform.position != posList[posList.Count - 1])
                                {
                                    AddBuildingToList(posList[posList.Count - 1]);
                                    isPreBeltSend = !isPreBeltSend;
                                }
                            }
                            else if (buildingList[buildingList.Count - 1].TryGetComponent(out UnderPipeBuild lastPipe))
                            {
                                isPreBeltSend = lastPipe.isSendPipe;
                                if (isRemoe && buildingList[buildingList.Count - 1].transform.position != posList[posList.Count - 1])
                                {
                                    AddBuildingToList(posList[posList.Count - 1]);
                                    isPreBeltSend = !isPreBeltSend;
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
            if (!posList.Contains(position))
            {
                posList.Add(position);
                CreateObj(position);
            }
        }
        else if (isMoveX)
        {
            float currentX = startBuildPos.x;
            float direction = Mathf.Sign(endBuildPos.x - startBuildPos.x);

            if (direction > 0 && currentX < endBuildPos.x)
            {
                moveDir = true;
            }
            else if (direction < 0 && currentX > endBuildPos.x)
            {
                moveDir = false;
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
                    if (!posList.Contains(position))
                    {
                        posList.Add(position);
                        CreateObj(position);
                    }
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
                        if (!posList.Contains(position))
                        {
                            posList.Add(position);
                            CreateObj(position);
                        }
                        currentX += objWidth * direction;
                    }
                    tempMoveDir = moveDir;
                }
                else if (dirNum == 3 && !moveDir)
                {
                    while (Mathf.Approximately(currentX, endBuildPos.x) || (direction < 0 && currentX >= endBuildPos.x))
                    {
                        Vector3 position = new Vector3(currentX, startBuildPos.y, 0);
                        if (!posList.Contains(position))
                        {
                            posList.Add(position);
                            CreateObj(position);
                        }
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
                    if (!posList.Contains(position))
                    {
                        posList.Add(position);
                        CreateObj(position);
                    }
                    currentY += objHeight * direction;
                }
                tempMoveDir = moveDir;
            }
            else if (isUnderObj && isGetDir && (dirNum == 0 || dirNum == 2))
            {
                Vector3 position = new Vector3(startBuildPos.x, startBuildPos.y, 0);
                if (!posList.Contains(position))
                {
                    posList.Add(position);
                    CreateObj(position);
                }
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
            }
            else if (direction < 0 && currentY > endBuildPos.y)
            {
                moveDir = false;
            }            
            
            if(moveDir != tempMoveDir)
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
                    if (!posList.Contains(position))
                    {
                        posList.Add(position);
                        CreateObj(position);
                    }
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
                        if (!posList.Contains(position))
                        {
                            posList.Add(position);
                            CreateObj(position);
                        }
                        currentY += objHeight * direction;
                    }
                    tempMoveDir = moveDir;
                }
                else if (dirNum == 2 && !moveDir)
                {
                    while (Mathf.Approximately(currentY, endBuildPos.y) || (direction < 0 && currentY >= endBuildPos.y))
                    {
                        Vector3 position = new Vector3(startBuildPos.x, currentY, 0);
                        if (!posList.Contains(position))
                        {
                            posList.Add(position);
                            CreateObj(position);
                        }
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
                    if (!posList.Contains(position))
                    {
                        posList.Add(position);
                        CreateObj(position);
                    }
                    currentX += objWidth * direction;
                }
                tempMoveDir = moveDir;
            }
            else if (isUnderObj && isGetDir && (dirNum == 1 || dirNum == 3))
            {
                Vector3 position = new Vector3(startBuildPos.x, startBuildPos.y, 0);
                if (!posList.Contains(position))
                {
                    posList.Add(position);
                    CreateObj(position);
                }
                tempMoveDir = moveDir;
            }
        }
        gameObj.SetActive(false);
    }

    bool GroupBuildCheck(GameObject obj, Vector2 pos)
    {
        GameObject buildObj = GetBuildObject(obj);
        Structure str = buildObj.GetComponent<Structure>();

        if (str.canBuilding && CellCheck(buildObj, pos))
            return true;
        else
            return false;
    }

    IEnumerator SetBuilding(GameObject obj)
    {
        yield return new WaitForSeconds(0.1f);

        if (!isTempBuild)
        {
            if (BuildingInfo.instance.AmountsEnoughCheck())
            {
                SetColor(obj.GetComponentInChildren<SpriteRenderer>(), Color.white, 1f);
                obj.GetComponentInChildren<SpriteRenderer>().sortingOrder = layNumTemp;
                if (obj.TryGetComponent(out Structure structure))
                {
                    if (structure.canBuilding)
                    {
                        structure.SetBuild();
                        structure.ColliderTriggerOnOff(false);
                        obj.AddComponent<DynamicGridObstacle>();
                    }
                }
                else if (obj.TryGetComponent(out BeltGroupMgr belt))
                {
                    if (belt.beltList[0].canBuilding)
                    {
                        obj.transform.parent = beltMgr.transform;
                        belt.isPreBuilding = false;
                        belt.beltList[0].SetBuild();
                        belt.beltList[0].ColliderTriggerOnOff(false);
                        belt.beltList[0].gameObject.AddComponent<DynamicGridObstacle>();
                    }
                }
                else if (obj.TryGetComponent(out UnderBeltCtrl underBelt))
                {
                    if (underBelt.beltScipt.canBuilding)
                    {
                        underBelt.beltScipt.SetBuild();
                        underBelt.ColliderTriggerOnOff(false);
                        underBelt.RemoveObj();
                        underBelt.beltScipt.gameObject.AddComponent<DynamicGridObstacle>();
                    }
                }
                else if (obj.TryGetComponent(out UnderPipeBuild underPipe))
                {
                    if (underPipe.pipeScipt.canBuilding)
                    {                    
                        underPipe.pipeScipt.SetBuild();
                        underPipe.ColliderTriggerOnOff(false);
                        underPipe.RemoveObj();
                        underPipe.pipeScipt.gameObject.AddComponent<DynamicGridObstacle>();
                    }
                }
                BuildingInfo.instance.BuildingEnd();
            }
            else
            {
                Destroy(obj);
            }
            isEnough = BuildingInfo.instance.AmountsEnoughCheck();
            setBuild = null;
        }
        else
        {
            bool canBuild = playerController.TempMinerCountCheck();
            if (canBuild)
            {
                SetColor(obj.GetComponentInChildren<SpriteRenderer>(), Color.white, 1f);
                obj.GetComponentInChildren<SpriteRenderer>().sortingOrder = layNumTemp;
                if (obj.TryGetComponent(out Structure structure))
                {
                    if (structure.canBuilding)
                    {
                        structure.SetBuild();
                        structure.isTempBuild = true;
                        structure.TempBuilCooldownSet();
                        structure.ColliderTriggerOnOff(false);
                        obj.AddComponent<DynamicGridObstacle>();
                    }
                }
                playerController.TempBuildSet();
            }
            else
            {
                Destroy(obj);
            }
            isEnough = canBuild;
            setBuild = null;
        }
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
            else
            {
                UnderBeltCtrl underBelt = gameObj.GetComponent<UnderBeltCtrl>();
                UnderPipeBuild underPipe = gameObj.GetComponent<UnderPipeBuild>();

                if (buildingList.Count == 0)
                {
                    AddBuildingToList(pos);
                    if(underBelt != null)
                    {
                        isPreBeltSend = underBelt.isSendBelt;
                    }
                    else if (underPipe != null)
                    {
                        isPreBeltSend = underPipe.isSendPipe;
                    }
                }
                else if (buildingList.Count > 0)
                {
                    if (!isPreBeltSend)
                    {
                        if (buildingList.Count == 1)
                        {                            
                            AddBuildingToList(pos);
                            isPreBeltSend = !isPreBeltSend;
                        }
                        else if (Vector3.Distance(pos, buildingList[buildingList.Count - 2].transform.position) >= 11)
                        {
                            AddBuildingToList(pos);
                            isPreBeltSend = !isPreBeltSend; 
                        }
                        else if (Vector3.Distance(pos, buildingList[buildingList.Count - 2].transform.position) < 11)
                        {
                            AddBuildingToList(pos);
                            Destroy(buildingList[buildingList.Count - 2]);
                            buildingList.RemoveAt(buildingList.Count - 2);
                        }
                    }
                    else
                    {
                        AddBuildingToList(pos);
                        isPreBeltSend = !isPreBeltSend;
                    }
                }
            }
        }        
    }

    void AddBuildingToList(Vector3 pos)
    {
        GameObject obj = Instantiate(gameObj, pos, Quaternion.identity);
        obj.SetActive(true);
        buildingList.Add(obj);
    }

    public void SetImage(Building build, bool _isTempbuild)
    {
        GameObject game = build.gameObj;
        int level = build.level;
        int height = build.height;
        int width = build.width;
        int dirCount = build.dirCount;
        isTempBuild = _isTempbuild;

        if (gameObj != null)
        {
            Destroy(gameObj);
        }

        gameObj = Instantiate(game);

        isUnderObj = false;
        
        objHeight = height;
        objWidth = width;

        if (gameObj.TryGetComponent(out Structure factory))
        {
            factory.BuildingSetting(level, height, width, dirCount);
            dirNum = factory.dirNum;
        }
        else if (gameObj.TryGetComponent(out BeltGroupMgr belt))
        {
            belt.isPreBuilding = true;
            belt.SetBelt(0, level, height, width, dirCount);
            dirNum = belt.beltList[0].dirNum;
        }
        else if (gameObj.TryGetComponent(out UnderBeltCtrl underBelt))
        {
            underBelt.isPreBuilding = true;
            underBelt.BuildingSetting(level, height, width, dirCount);
            underBelt.SetSendUnderBelt();
            isUnderObj = true;
            dirNum = underBelt.dirNum;
        }
        else if (gameObj.TryGetComponent(out UnderPipeBuild underPipe))
        {
            underPipe.isPreBuilding = true;
            underPipe.SetUnderPipe(level, height, width, dirCount);
            isUnderObj = true;
            isGetDir = true;
            dirNum = underPipe.dirNum;
        }

        if (height == 1 && width == 1)
        {
            isNeedSetPos = false;
            gameObj.transform.position = this.transform.position;
        }
        else if (height == 1 && width == 2)
        {
            setPos = new Vector3(-0.5f, -1);
            isNeedSetPos = true;
            gameObj.transform.position = this.transform.position - setPos;
        }
        else if (height == 2 && width == 2)
        {
            setPos = new Vector3(-0.5f, -0.5f);
            isNeedSetPos = true;
            gameObj.transform.position = this.transform.position - setPos;
        }

        if (dirCount == 4)
        {
            isGetDir = true;
        }
        else
        {
            isGetDir = false;
        }

        gameObj.transform.parent = this.transform;

        if(gameObj.GetComponentInChildren<SpriteRenderer>() != null)
        {
            spriteRenderer = gameObj.GetComponentInChildren<SpriteRenderer>();
            layNumTemp = spriteRenderer.sortingOrder;
        }


        spriteRenderer.sortingOrder = 50;
        if(!gameObj.GetComponent<UnderBeltCtrl>())
            SetColor(spriteRenderer, Color.green, 0.35f);
        else
        {
            gameObj.GetComponent<UnderBeltCtrl>().SetColor(Color.green);
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

                GameObject buildObj = GetBuildObject(obj);
                Structure str = buildObj.GetComponent<Structure>();
                canBuilding = str.canBuilding;

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
            if(gameObj != null)
            {
                if (isBuildingOk && isEnough && GroupBuildCheck(gameObj, mousePos))
                {
                    if (!gameObj.GetComponent<UnderBeltCtrl>())
                        SetColor(spriteRenderer, Color.green, 0.35f);
                    else
                    {
                        gameObj.GetComponent<UnderBeltCtrl>().SetColor(Color.green);
                    }
                }
                else if (!isBuildingOk || !isEnough || !GroupBuildCheck(gameObj, mousePos))
                {
                    if (!gameObj.GetComponent<UnderBeltCtrl>())
                        SetColor(spriteRenderer, Color.red, 0.35f);
                    else
                    {
                        gameObj.GetComponent<UnderBeltCtrl>().SetColor(Color.red);
                    }
                }
            }
        }
    }

    bool CellCheck(GameObject obj, Vector2 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);

        List<int> xList = new List<int>();
        List<int> yList = new List<int>();

        if (objHeight == 1 && objWidth == 1)
        {
            xList.Add(x);
            yList.Add(y);
        }
        else if (objHeight == 1 && objWidth == 2)
        {
            xList.Add(x);
            xList.Add(x + 1);
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
        foreach (int newX in xList)
        {
            foreach (int newY in yList)
            {
                if (!gameManager.map.IsOnMap(newX, newY))
                {
                    continue;
                }

                if (gameManager.map.mapData[newX][newY].structure != null)
                {
                    return false;
                }

                Miner miner = null;
                PumpCtrl pump = null;
                ExtractorCtrl extractor = null;

                if (obj.TryGetComponent(out miner) || obj.TryGetComponent(out pump) || obj.TryGetComponent(out extractor))
                {
                    if ((miner && gameManager.map.mapData[newX][newY].BuildCheck("miner") &&
                        miner.level >= gameManager.map.mapData[newX][newY].resource.level) ||
                        (pump && gameManager.map.mapData[newX][newY].BuildCheck("pump")) ||
                        (extractor && gameManager.map.mapData[newX][newY].BuildCheck("extractor")))
                    {
                        return true;
                    }
                }
                else
                {
                    if (gameManager.map.mapData[newX][newY].buildable.Count == 0 && gameManager.map.mapData[newX][newY].obj == null)
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

        if (canBuild)
            return true;
        else
            return false;
    }

    public void ReSetImage()
    {
        if (gameObj != null)
        {
            Destroy(gameObj);
        }

        if(spriteRenderer)
            spriteRenderer.sprite = null;
        gameObj = null;
        gameObject.SetActive(false);
        playerController.TempBuildUI(false);
    }


    void RotationImg(GameObject obj)
    {
        GameObject buildObj = GetBuildObject(obj);
        Structure str = buildObj.GetComponent<Structure>();

        str.dirNum++;
        if (str.dirNum >= str.dirCount)
            str.dirNum = 0;
        dirNum = str.dirNum;

        if (obj.TryGetComponent(out UnderBeltCtrl underBelt))
        {
            underBelt.dirNum = underBelt.beltScipt.dirNum;
        }
        else if (obj.TryGetComponent(out UnderPipeBuild underPipe))
        {
            underPipe.dirNum = underPipe.pipeScipt.dirNum;
        }
    }
}
