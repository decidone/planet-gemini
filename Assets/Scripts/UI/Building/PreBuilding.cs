using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using Pathfinding;

enum MouseBtnFunc
{
    None,
    MouseButtonDown,
    MouseButton,
    MouseButtonUp
}

public class PreBuilding : MonoBehaviour
{
    public static PreBuilding instance;
    SpriteRenderer spriteRenderer;
    GameObject gameObj;
    public bool isSelect = false;

    public GameObject beltMgr = null;
    public GameObject pipeMgr = null;

    bool isNeedSetPos = false;
    Vector3 setPos;

    private Tilemap tilemap;

    public bool isBuildingOk = true;
    int layNumTemp = 0;

    int objHeight = 1;
    int objWidth = 1;
    bool isGetDir = false;
    int dirNum = 0;

    Vector3 startBuildPos;
    Vector3 startTempPos;
    Vector3 endBuildPos;

    bool isUnderObj = false;
    public bool isEnough = true;

    List<GameObject> buildingList = new List<GameObject>();    
    Vector3 tempPos;
    List<Vector3> posList = new List<Vector3>();
    bool isMoveX = true; 
    bool tempMoveX;
    bool moveDir;
    bool tempMoveDir;
    bool isPreBeltSend;
    MouseBtnFunc mouseBtnFunc = MouseBtnFunc.None;
    bool isMouseLeft = true;
    public bool isDrag = false;
    Coroutine setBuild;

    void Awake()
    {
        tilemap = GameObject.Find("Tilemap").GetComponent<Tilemap>();

        if (instance != null)
        {
            Debug.LogWarning("More than one instance of PreBuilding found!");
            return;
        }

        instance = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && gameObj != null && !Input.GetMouseButton(0))
        {
            RotationImg(gameObj);
        }

        if (Input.GetMouseButtonDown(0))
        {
            startBuildPos = transform.position;
            endBuildPos = transform.position;
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                isMouseLeft = true;
                mouseBtnFunc = MouseBtnFunc.MouseButtonDown;
            }
        }
        else if (Input.GetMouseButton(0))
        {
            isMouseLeft = true;
            mouseBtnFunc = MouseBtnFunc.MouseButton;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isMouseLeft = true;
            mouseBtnFunc = MouseBtnFunc.MouseButtonUp;
        }
        if (Input.GetMouseButtonDown(1))
        {
            isMouseLeft = false;
            mouseBtnFunc = MouseBtnFunc.MouseButtonUp;
        }

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = tilemap.WorldToCell(mousePosition);
        Vector3 cellCenter = tilemap.GetCellCenterWorld(cellPosition);
        cellCenter.z = transform.position.z;
        transform.position = cellCenter;

        if (gameObj != null)
        {
            if (isBuildingOk && isEnough)
            {
                if (!gameObj.GetComponent<UnderBeltCtrl>())
                    SetColor(spriteRenderer, Color.green, 0.35f);
                else
                {
                    gameObj.GetComponent<UnderBeltCtrl>().SetColor(Color.green);
                }
            }
            else if (!isBuildingOk || !isEnough)
            {
                if (!gameObj.GetComponent<UnderBeltCtrl>())
                    SetColor(spriteRenderer, Color.red, 0.35f);
                else
                {
                    gameObj.GetComponent<UnderBeltCtrl>().SetColor(Color.red);
                }
            }
        }
        InputCheck();
        BuildingListCtrl();
    }

    void FixedUpdate()
    {
        if(EventSystem.current.IsPointerOverGameObject() && mouseBtnFunc == MouseBtnFunc.MouseButton)
        {
            mouseBtnFunc = MouseBtnFunc.None;
        }
        if (!EventSystem.current.IsPointerOverGameObject() && mouseBtnFunc == MouseBtnFunc.MouseButton && isMouseLeft)//Input.GetMouseButton(0))
        {
            if (startTempPos == startBuildPos)
                startBuildPos = transform.position;

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

                //posList.Clear();

                if (absDeltaX >= absDeltaY)
                    isMoveX = true;
                else
                    isMoveX = false;

                CheckPos();
                isDrag = true;
            }
        }
    }

    protected virtual void InputCheck()
    {
        if (!isEnough || mouseBtnFunc != MouseBtnFunc.MouseButtonUp)
            return;

        if (isMouseLeft)
        {
            if (!isDrag && !EventSystem.current.IsPointerOverGameObject())
            {
                CheckPos();
            }

            bool canBuild = false;
            foreach (GameObject obj in buildingList)
            {
                if (GroupBuildCheck(obj))
                    canBuild = true;
                else
                {
                    canBuild = false;
                    break;
                }
            }

            if (canBuild)
            {
                foreach (GameObject obj in buildingList)
                {
                    setBuild = StartCoroutine("SetBuilding", obj);
                    if(obj.TryGetComponent(out UnderBeltCtrl underBelt))
                    {
                        underBelt.buildEnd = true;
                    }
                    else if (obj.TryGetComponent(out UnderPipeBuild underPipe))
                    {
                        underPipe.buildEnd = true;
                    }
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
            startTempPos = startBuildPos;
            gameObj.SetActive(true);
            posList.Clear();
            isDrag = false;
        }
        else
        {
            if(setBuild == null)
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
        mouseBtnFunc = MouseBtnFunc.None;
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
                                if (tempPos.x < buildingList[b].transform.position.x)
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
                                if (tempPos.y < buildingList[b].transform.position.y)
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
        if (isMoveX)
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
        else
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

    bool GroupBuildCheck(GameObject obj)
    {
        if (obj.TryGetComponent(out Structure factory) && factory.canBuilding)
        {
            return true;
        }
        if (obj.TryGetComponent(out TowerAi tower) && tower.canBuilding)
        {
            return true;
        }
        if (obj.TryGetComponent(out BeltGroupMgr belt) && belt.beltList[0].canBuilding)
        {
            return true;
        }
        if (obj.TryGetComponent(out PipeGroupMgr pipe) && pipe.pipeList[0].canBuilding)
        {
            return true;
        }
        if (obj.TryGetComponent(out UnderBeltCtrl underBelt) && underBelt.beltScipt.canBuilding)
        {
            return true;
        }
        if (obj.TryGetComponent(out UnderPipeBuild underPipe) && underPipe.pipeScipt.canBuilding)
        {
            return true;
        }
        return false;
    }

    IEnumerator SetBuilding(GameObject obj)
    {
        yield return new WaitForSeconds(0.1f);

        if (BuildingInfo.instance.AmountsEnoughCheck())
        {
            SetColor(obj.GetComponentInChildren<SpriteRenderer>(), Color.white, 1f);
            obj.GetComponentInChildren<SpriteRenderer>().sortingOrder = layNumTemp;
            if (obj.TryGetComponent(out Structure factory))
            {
                if (factory.canBuilding)
                {
                    factory.SetBuild();
                    factory.ColliderTriggerOnOff(false);
                    obj.AddComponent<DynamicGridObstacle>();
                }
                else
                {
                    Destroy(obj);
                    yield break;
                }
            }
            else if (obj.TryGetComponent(out TowerAi tower))
            {
                if (tower.canBuilding)
                {
                    tower.SetBuild();
                    tower.ColliderTriggerOnOff(false);
                    obj.AddComponent<DynamicGridObstacle>();
                }
                else
                {
                    Destroy(obj);
                    yield break;
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
                else
                {
                    Destroy(obj);
                    yield break;
                }
            }
            else if (obj.TryGetComponent(out PipeGroupMgr pipe))
            {
                if (pipe.pipeList[0].canBuilding)
                {
                    obj.transform.parent = pipeMgr.transform;
                    pipe.isPreBuilding = false;
                    pipe.pipeList[0].SetBuild();
                    pipe.pipeList[0].ColliderTriggerOnOff(false);
                    pipe.pipeList[0].gameObject.AddComponent<DynamicGridObstacle>();
                }
                else
                {
                    Destroy(obj);
                    yield break;
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
                else
                {
                    Destroy(obj);
                    yield break;
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
                else
                {
                    Destroy(obj);
                    yield break;
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

    public void SetImage(GameObject game, int level, int height, int width, int dirCount)
    {
        if (this.transform.childCount > 0)
        {
            GameObject temp = transform.GetChild(0).gameObject;
            Destroy(temp);
        }

        gameObj = Instantiate(game);
        isUnderObj = false;

        if (gameObj.TryGetComponent(out Structure factory))
        {
            factory.isPreBuilding = true;
            factory.ColliderTriggerOnOff(true);
            //factory.DisableColliders();
            factory.level = level -1;
            dirNum = factory.dirNum;
        }
        else if (gameObj.TryGetComponent(out TowerAi tower))
        {
            tower.isPreBuilding = true;
            //tower.DisableColliders();
            tower.ColliderTriggerOnOff(true);
            tower.level = level - 1;
            //isGetDir = false;
            dirNum = 0;
        }
        else if (gameObj.TryGetComponent(out BeltGroupMgr belt))
        {
            belt.isPreBuilding = true;
            belt.SetBelt(0);
            belt.beltList[0].isPreBuilding = true;
            belt.beltList[0].ColliderTriggerOnOff(true);
            //belt.beltList[0].DisableColliders();
            belt.beltList[0].level = level - 1;
            //isGetDir = true;
            dirNum = belt.beltList[0].dirNum;
        }
        else if (gameObj.TryGetComponent(out PipeGroupMgr pipe))
        {
            pipe.isPreBuilding = true;
            pipe.SetPipe(0);
            pipe.pipeList[0].isPreBuilding = true;
            pipe.pipeList[0].ColliderTriggerOnOff(true);
            //pipe.pipeList[0].DisableColliders();
            //isGetDir = false;
            dirNum = pipe.pipeList[0].dirNum;
        }
        else if (gameObj.TryGetComponent(out UnderBeltCtrl underBelt))
        {
            underBelt.isPreBuilding = true;
            underBelt.SetSendUnderBelt();
            //underBelt.SetLevel(level);
            isUnderObj = true;
            //isGetDir = true;
            dirNum = underBelt.dirNum;            
        }
        else if (gameObj.TryGetComponent(out UnderPipeBuild underPipe))
        {
            underPipe.isPreBuilding = true;
            underPipe.SetUnderPipe();
            //underBelt.SetLevel(level);
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

        objHeight = height;
        objWidth = width;

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
        isSelect = true;
    }

    void SetColor(SpriteRenderer sprite, Color color, float alpha)
    {
        Color slotColor = sprite.color;
        slotColor = color;
        slotColor.a = alpha;
        sprite.color = slotColor;
    }

    void BuildingListCtrl()
    {
        if(buildingList.Count > 0)
        {
            foreach (GameObject obj in buildingList)
            {
                bool canBuilding = false;
                Color colorRed = Color.red;
                Color colorGreen = Color.green;
                float alpha = 0.35f;

                if (obj.TryGetComponent(out Structure factory))
                {
                    canBuilding = factory.canBuilding;
                }
                else if (obj.TryGetComponent(out TowerAi tower))
                {
                    canBuilding = tower.canBuilding;
                }
                else if (obj.TryGetComponent(out BeltGroupMgr belt))
                {
                    canBuilding = belt.beltList[0].canBuilding;
                }
                else if (obj.TryGetComponent(out PipeGroupMgr pipe))
                {
                    canBuilding = pipe.pipeList[0].canBuilding;
                }
                else if (obj.TryGetComponent(out UnderBeltCtrl underBelt))
                {
                    canBuilding = underBelt.beltScipt.canBuilding;
                }
                else if (obj.TryGetComponent(out UnderPipeBuild underPipe))
                {
                    canBuilding = underPipe.pipeScipt.canBuilding;
                }

                if (!canBuilding)
                {
                    SetColor(obj.GetComponentInChildren<SpriteRenderer>(), colorRed, alpha);
                }
                else
                {
                    SetColor(obj.GetComponentInChildren<SpriteRenderer>(), colorGreen, alpha);
                }
            }
        }
    }

    public void ReSetImage()
    {
        if (this.transform.childCount > 0)
        {
            GameObject temp = transform.GetChild(0).gameObject;
            Destroy(temp);
        }

        if(spriteRenderer)
            spriteRenderer.sprite = null;
        isSelect = false;
        gameObj = null;
        gameObject.SetActive(false);
    }

    void RotationImg(GameObject obj)
    {
        if (obj.TryGetComponent(out Structure factory))
        {
            factory.dirNum++;
            if(factory.dirNum >= factory.dirCount)
                factory.dirNum = 0;
            dirNum = factory.dirNum;
        }
        else if(obj.TryGetComponent(out BeltGroupMgr belt))
        {
            belt.beltList[0].dirNum++;
            if (belt.beltList[0].dirNum >= belt.beltList[0].dirCount)
                belt.beltList[0].dirNum = 0;
            dirNum = belt.beltList[0].dirNum;
        }
        else if (obj.TryGetComponent(out UnderBeltCtrl underBelt))
        {
            underBelt.beltScipt.dirNum++;
            if (underBelt.beltScipt.dirNum >= underBelt.beltScipt.dirCount)
                underBelt.beltScipt.dirNum = 0;

            underBelt.dirNum = underBelt.beltScipt.dirNum;
            dirNum = underBelt.beltScipt.dirNum;
        }
        else if (obj.TryGetComponent(out UnderPipeBuild underPipe))
        {
            underPipe.pipeScipt.dirNum++;
            if (underPipe.pipeScipt.dirNum >= underPipe.pipeScipt.dirCount)
                underPipe.pipeScipt.dirNum = 0;

            underPipe.dirNum = underPipe.pipeScipt.dirNum;
            dirNum = underPipe.pipeScipt.dirNum;
        }
    }
}
