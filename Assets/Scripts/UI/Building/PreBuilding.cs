using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using Pathfinding;

public class PreBuilding : MonoBehaviour
{
    public static PreBuilding instance;
    SpriteRenderer spriteRenderer;
    [SerializeField]
    GameObject gameObj = null; 
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
    Vector3 tempPos;
    public bool shiftKeyDown = false;
    bool isUnderObj = false;
    public bool isEnough = true;

    [SerializeField]
    List<GameObject> buildingList = new List<GameObject>();
    [SerializeField]
    List<Vector3> posList = new List<Vector3>();
    bool isMoveX = true; 
    bool tempMoveX;
    bool isPreBeltSend;
    void Awake()
    {
        tilemap = GameObject.Find("Tilemap").GetComponent<Tilemap>();


        if (instance != null)
        {
            Debug.LogWarning("More than one instance of drag slot found!");
            return;
        }

        instance = this;
    }

    void Update()
    {
        InputCheck();
        BuildingListCtrl();

        if (Input.GetKeyDown(KeyCode.R) && gameObj != null && !shiftKeyDown && !Input.GetMouseButton(0))
        {
            RotationImg(gameObj);
        }
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            shiftKeyDown = true;
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            shiftKeyDown = false;
        }

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = tilemap.WorldToCell(mousePosition);
        Vector3 cellCenter = tilemap.GetCellCenterWorld(cellPosition);
        cellCenter.z = transform.position.z;
        transform.position = cellCenter;        

        if(gameObj != null)
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
    }

    protected virtual void InputCheck()
    {
        if (shiftKeyDown && isEnough)
        {
            if (!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(0))
            {
                startBuildPos = transform.position;
                endBuildPos = transform.position; 
            }
            else if (!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButton(0))
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
                }
            }    
            else if (Input.GetMouseButtonUp(0))
            {
                if (buildingList.Count > 0)
                {
                    foreach (GameObject obj in buildingList)
                    {
                        StartCoroutine("SetBuilding", obj);
                    }
                }
                buildingList.Clear();
                startTempPos = startBuildPos;
                gameObj.SetActive(true); 
                posList.Clear();
                //isEnough = BuildingInfo.instance.AmountsEnoughCheck();
            }
        }
        else if (!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(0))
        {
            startBuildPos = transform.position;
            endBuildPos = transform.position;
        }

        else if (Input.GetMouseButtonUp(0) && !shiftKeyDown)
        {
            if (buildingList.Count > 0)
            {
                foreach (GameObject obj in buildingList)
                {
                    StartCoroutine("SetBuilding", obj);
                }
                buildingList.Clear();
                startTempPos = startBuildPos;
                gameObj.SetActive(true);
                posList.Clear();
            }
            else
            {
                if (gameObj != null)
                {
                    if (isBuildingOk && BuildingInfo.instance.AmountsEnoughCheck())
                    {
                        GameObject obj = Instantiate(gameObj);
                        SetColor(obj.GetComponentInChildren<SpriteRenderer>(), Color.white, 1f);
                        obj.GetComponentInChildren<SpriteRenderer>().sortingOrder = layNumTemp;

                        obj.transform.position = gameObj.transform.position;

                        if(obj.TryGetComponent(out Structure factory))
                        {
                            factory.SetBuild();
                            factory.ColliderTriggerOnOff(false);
                            obj.AddComponent<DynamicGridObstacle>();
                        }
                        else if (obj.TryGetComponent(out TowerAi tower))
                        {
                            tower.SetBuild();
                            tower.ColliderTriggerOnOff(false);
                            obj.AddComponent<DynamicGridObstacle>();
                        }
                        else if (obj.TryGetComponent(out BeltGroupMgr belt))
                        {
                            obj.transform.parent = beltMgr.transform;
                            belt.isPreBuilding = false;
                            belt.beltList[0].SetBuild();
                            belt.beltList[0].ColliderTriggerOnOff(false);
                            belt.beltList[0].gameObject.AddComponent<DynamicGridObstacle>();
                        }
                        else if (obj.TryGetComponent(out PipeGroupMgr pipe))
                        {
                            obj.transform.parent = pipeMgr.transform;
                            pipe.isPreBuilding = false;
                            pipe.pipeList[0].SetBuild();
                            pipe.pipeList[0].ColliderTriggerOnOff(false);
                            pipe.pipeList[0].gameObject.AddComponent<DynamicGridObstacle>();
                        }
                        else if (obj.TryGetComponent(out UnderBeltCtrl underBelt))
                        {
                            underBelt.beltScipt.SetBuild();
                            underBelt.ColliderTriggerOnOff(false);
                            underBelt.RemoveObj();
                            underBelt.beltScipt.gameObject.AddComponent<DynamicGridObstacle>();
                        }
                        BuildingInfo.instance.BuildingEnd();
                        isEnough = BuildingInfo.instance.AmountsEnoughCheck();
                    }
                }
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            if (buildingList.Count > 0)
            {
                foreach (GameObject build in buildingList)
                {
                    Destroy(build);
                }
                buildingList.Clear();
            }
            ReSetImage();
            shiftKeyDown = false;
            posList.Clear();
        }
    }


    void CheckPos()
    {
        if(posList.Count > 0)
        {
            if (isMoveX != tempMoveX)
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
                //tempMoveX = isMoveX;
            }

            int posIndex = 0;

            List<GameObject> objectsToRemove = new List<GameObject>(); // 삭제된 오브젝트를 추적할 리스트
            List<Vector3> posToRemove = new List<Vector3>();

            if ((!isGetDir && isMoveX) || (isGetDir && dirNum == 1 || dirNum == 3))
            {
                if (posList.Contains(new Vector3(tempPos.x, startBuildPos.y, 0)))
                {
                    if (posList.Count > 1)
                    {
                        if (startBuildPos.x > posList[1].x)
                        {
                            for (int i = 0; i < posList.Count; i++)
                            {
                                if (i < buildingList.Count && tempPos.x > posList[i].x)
                                {
                                    Destroy(buildingList[i]);
                                    objectsToRemove.Add(buildingList[i]);
                                    posToRemove.Add(posList[i]);
                                }
                                else
                                {
                                    posIndex++;
                                }
                            }

                            buildingList.RemoveAll(obj => objectsToRemove.Contains(obj));
                            posList.RemoveAll(pos => posToRemove.Contains(pos));
                        }
                        else if (startBuildPos.x < posList[1].x)
                        {
                            for (int i = 0; i < posList.Count; i++)
                            {
                                if (i < buildingList.Count && tempPos.x < posList[i].x)
                                {
                                    Destroy(buildingList[i]);
                                    objectsToRemove.Add(buildingList[i]);
                                    posToRemove.Add(posList[i]);
                                }
                                else
                                {
                                    posIndex++;
                                }
                            }

                            buildingList.RemoveAll(obj => objectsToRemove.Contains(obj));
                            posList.RemoveAll(pos => posToRemove.Contains(pos));
                        }
                        //if (startBuildPos.x > posList[1].x)
                        //{
                        //    for (int i = 0; i < posList.Count; i++)
                        //    {
                        //        if (tempPos.x > posList[i].x)
                        //        {
                        //            break;
                        //        }
                        //        posIndex++;
                        //    }
                        //    for (int i = posIndex; i < buildingList.Count; i++)
                        //    {
                        //        Destroy(buildingList[i]);
                        //        objectsToRemove.Add(buildingList[i]);
                        //        posToRemove.Add(posList[i]);
                        //    }
                        //    buildingList.RemoveAll(obj => objectsToRemove.Contains(obj));
                        //    posList.RemoveAll(pos => posToRemove.Contains(pos));
                        //}
                        //else if (startBuildPos.x < posList[1].x)
                        //{
                        //    for (int i = 0; i < posList.Count; i++)
                        //    {
                        //        if (tempPos.x < posList[i].x)
                        //        {
                        //            break;
                        //        }
                        //        posIndex++;
                        //    }
                        //    for (int i = posIndex; i < buildingList.Count; i++)
                        //    {
                        //        Destroy(buildingList[i]);
                        //        objectsToRemove.Add(buildingList[i]);
                        //        posToRemove.Add(posList[i]);
                        //    }
                        //    buildingList.RemoveAll(obj => objectsToRemove.Contains(obj));
                        //    posList.RemoveAll(pos => posToRemove.Contains(pos));
                        //}
                    }
                }
                else
                {
                    SetPos();
                }
                tempMoveX = true;
            }
            else if((!isGetDir && !isMoveX) || (isGetDir && dirNum == 0 || dirNum == 2))
            {
                if (posList.Contains(new Vector3(startBuildPos.x, tempPos.y, 0)))
                {
                    if (posList.Count > 1)
                    {
                        if (startBuildPos.y > posList[1].y)
                        {
                            for (int i = 0; i < posList.Count; i++)
                            {
                                if (i < buildingList.Count && tempPos.y > posList[i].y)
                                {
                                    Destroy(buildingList[i]);
                                    objectsToRemove.Add(buildingList[i]);
                                    posToRemove.Add(posList[i]);
                                }
                                else
                                {
                                    posIndex++;
                                }
                            }

                            buildingList.RemoveAll(obj => objectsToRemove.Contains(obj));
                            posList.RemoveAll(pos => posToRemove.Contains(pos));
                        }
                        else if (startBuildPos.y < posList[1].y)
                        {
                            for (int i = 0; i < posList.Count; i++)
                            {
                                if (i < buildingList.Count && tempPos.y < posList[i].y)
                                {
                                    Destroy(buildingList[i]);
                                    objectsToRemove.Add(buildingList[i]);
                                    posToRemove.Add(posList[i]);
                                }
                                else
                                {
                                    posIndex++;
                                }
                            }

                            buildingList.RemoveAll(obj => objectsToRemove.Contains(obj));
                            posList.RemoveAll(pos => posToRemove.Contains(pos));
                        }
                        //if (startBuildPos.y > posList[1].y)
                        //{
                        //    for (int i = 0; i < posList.Count; i++)
                        //    {
                        //        if (tempPos.y > posList[i].y)
                        //        {
                        //            break;
                        //        }
                        //        posIndex++;
                        //    }
                        //    for (int i = posIndex; i < buildingList.Count; i++)
                        //    {
                        //        Destroy(buildingList[i]);
                        //        objectsToRemove.Add(buildingList[i]);
                        //        posToRemove.Add(posList[i]);
                        //    }
                        //    buildingList.RemoveAll(obj => objectsToRemove.Contains(obj));
                        //    posList.RemoveAll(pos => posToRemove.Contains(pos));
                        //}
                        //else if (startBuildPos.y < posList[1].y)
                        //{
                        //    for (int i = 0; i < posList.Count; i++)
                        //    {
                        //        if (tempPos.y < posList[i].y)
                        //        {
                        //            break;
                        //        }
                        //        posIndex++;
                        //    }
                        //    for (int i = posIndex; i < buildingList.Count; i++)
                        //    {
                        //        Destroy(buildingList[i]);
                        //        objectsToRemove.Add(buildingList[i]);
                        //        posToRemove.Add(posList[i]);
                        //    }
                        //    buildingList.RemoveAll(obj => objectsToRemove.Contains(obj));
                        //    posList.RemoveAll(pos => posToRemove.Contains(pos));
                        //}
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
     
 
        //if (isMoveX)
        //{
        //    float currentX = startBuildPos.x;
        //    float direction = Mathf.Sign(endBuildPos.x - startBuildPos.x);

        //    while (Mathf.Approximately(currentX, endBuildPos.x) || (direction > 0 && currentX <= endBuildPos.x) || (direction < 0 && currentX >= endBuildPos.x))
        //    {

        //        Vector3 position = new Vector3(currentX, startBuildPos.y, 0);
        //        posList.Add(position);
        //        currentX += objWidth * direction;
        //    }
        //}
        //else
        //{
        //    float currentY = startBuildPos.y;
        //    float direction = Mathf.Sign(endBuildPos.y - startBuildPos.y);

        //    while (Mathf.Approximately(currentY, endBuildPos.y) || (direction > 0 && currentY <= endBuildPos.y) || (direction < 0 && currentY >= endBuildPos.y))
        //    {
        //        Vector3 position = new Vector3(startBuildPos.x, currentY, 0);
        //        posList.Add(position);
        //        currentY += objHeight * direction;
        //    }
        //}

    }

    void SetPos()
    {
        //float deltaX = endBuildPos.x - startBuildPos.x;
        //float deltaY = endBuildPos.y - startBuildPos.y;
        //float absDeltaX = Mathf.Abs(deltaX);
        //float absDeltaY = Mathf.Abs(deltaY);

        ////posList.Clear();

        //if (absDeltaX >= absDeltaY)
        //    isMoveX = true;
        //else
        //    isMoveX = false;

        if (isGetDir)
        {
            if (dirNum == 0 || dirNum == 2)
            {
                //if (!isMoveX)
                {
                    float currentY = startBuildPos.y;
                    float direction = Mathf.Sign(endBuildPos.y - startBuildPos.y);

                    if (dirNum == 0)
                    {
                        while (Mathf.Approximately(currentY, endBuildPos.y) || (direction > 0 && currentY < endBuildPos.y))
                        {
                            Vector3 position = new Vector3(startBuildPos.x, currentY, 0);
                            if (!posList.Contains(position))
                            {
                                posList.Add(position);
                                CreateObj(position);
                            }
                            currentY += objHeight * direction;
                        }
                    }
                    else if (dirNum == 2)
                    {
                        while (Mathf.Approximately(currentY, endBuildPos.y) || (direction < 0 && currentY > endBuildPos.y))
                        {
                            Vector3 position = new Vector3(startBuildPos.x, currentY, 0);
                            if (!posList.Contains(position))
                            {
                                posList.Add(position);
                                CreateObj(position);
                            }
                            currentY += objHeight * direction;
                        }
                    }
                }
                //else
                //{
                //    if (!posList.Contains(startBuildPos))
                //    {
                //        posList.Add(startBuildPos);
                //        CreateObj(startBuildPos);
                //    }
                //}
            }
            else if (dirNum == 1 || dirNum == 3)
            {
                //if (isMoveX)
                {
                    float currentX = startBuildPos.x;
                    float direction = Mathf.Sign(endBuildPos.x - startBuildPos.x);

                    if (dirNum == 1)
                    {
                        while (Mathf.Approximately(currentX, endBuildPos.x) || (direction > 0 && currentX < endBuildPos.x))
                        {
                            Vector3 position = new Vector3(currentX, startBuildPos.y, 0);
                            if (!posList.Contains(position))
                            {
                                posList.Add(position);
                                CreateObj(position);
                            }
                            currentX += objWidth * direction;
                        }
                    }
                    else if (dirNum == 3)
                    {
                        while (Mathf.Approximately(currentX, endBuildPos.x) || (direction < 0 && currentX > endBuildPos.x))
                        {
                            Vector3 position = new Vector3(currentX, startBuildPos.y, 0);
                            if (!posList.Contains(position))
                            {
                                posList.Add(position);
                                CreateObj(position);
                            }
                            currentX += objWidth * direction;
                        }
                    }
                }
                //else
                //{
                //    if (!posList.Contains(startBuildPos))
                //    {
                //        posList.Add(startBuildPos);
                //        CreateObj(startBuildPos);
                //    }
                //}
            }
            gameObj.SetActive(false);
        }
        else
        {
            if (isMoveX)
            {
                float currentX = startBuildPos.x;
                float direction = Mathf.Sign(endBuildPos.x - startBuildPos.x);
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
            }
            else
            {
                float currentY = startBuildPos.y;
                float direction = Mathf.Sign(endBuildPos.y - startBuildPos.y);
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
            }
            gameObj.SetActive(false);
            //CreateObj(startBuildPos);
        }
        //if (posList.Count > 0)
        //    CreateObj();
    }

    IEnumerator SetBuilding(GameObject obj)
    {
        yield return new WaitForSeconds(0.3f);

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
            BuildingInfo.instance.BuildingEnd();
        }
        else
        {
            Destroy(obj);
        }
        isEnough = BuildingInfo.instance.AmountsEnoughCheck();
    }

    void CreateObj(Vector3 pos)
    {
        Vector3 lastPos = posList[posList.Count - 1];

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
                if (buildingList.Count == 0)
                {
                    AddBuildingToList(pos);
                    isPreBeltSend = gameObj.GetComponent<UnderBeltCtrl>().isSendBelt;
                }
                else if (buildingList.Count > 0)
                {
                    if (!isPreBeltSend)
                    {
                        AddBuildingToList(pos);
                        isPreBeltSend = !isPreBeltSend;
                    }
                    else
                    {
                        Debug.Log(Vector3.Distance(pos, buildingList[buildingList.Count - 1].transform.position) > 9);
                        if (Vector3.Distance(pos, buildingList[buildingList.Count - 1].transform.position) > 9)
                        {
                            AddBuildingToList(pos);
                            isPreBeltSend = !isPreBeltSend;
                        }
                    }


                    //UnderBeltCtrl underBeltCtrl = gameObj.GetComponent<UnderBeltCtrl>();

                    //if ((underBeltCtrl.dirNum == 0 || underBeltCtrl.dirNum == 2) ||
                    //    (underBeltCtrl.dirNum == 1 || underBeltCtrl.dirNum == 3))
                    //{
                    //    if (posList.Count <= 12)
                    //    {
                    //        AddBuildingToList(pos);
                    //        if(buildingList.Count > 2 && posList.Count != 12)
                    //        {
                    //            Destroy(buildingList[buildingList.Count - 2]);
                    //            buildingList.RemoveAt(buildingList.Count - 2);
                    //        }
                    //        else
                    //        {
                    //            isPreBeltSend = !isPreBeltSend;
                    //        }
                    //    }
                    //    else if (posList.Count > 12)
                    //    {
                    //        if(!isPreBeltSend)
                    //        {
                    //            AddBuildingToList(pos); 
                    //            isPreBeltSend = !isPreBeltSend;
                    //        }
                    //        else
                    //        {
                    //            Debug.Log(Vector3.Distance(pos, buildingList[buildingList.Count - 1].transform.position) > 9);
                    //            if(Vector3.Distance(pos, buildingList[buildingList.Count - 1].transform.position) > 9)
                    //            {
                    //                AddBuildingToList(pos);
                    //                isPreBeltSend = !isPreBeltSend;
                    //            }
                    //        }
                    //    }
                        //else if (posList.Count > 12)
                        //{
                        //    if (pos == lastPos || setNum == 1 || setNum % 13 == 0)
                        //    {
                        //        AddBuildingToList(pos);
                        //        if (buildingList.Count > 2 && setNum != 0)
                        //        {
                        //            Destroy(buildingList[buildingList.Count - 2]);
                        //            buildingList.RemoveAt(buildingList.Count - 2);
                        //        }
                        //        if (setNum % 13 == 0)
                        //        {
                        //            setNum = 0;
                        //        }
                        //    }
                        //}
                        //Debug.Log(setNum);
                    //}
                    //else
                    //{
                    //    AddBuildingToList(pos);
                    //}
                }
            }
        }        
    }

    //void CreateObj()
    //{
    //    //bool isPreBeltSend = true;
    //    int setNum = 0;
    //    int buildIndex = 0;
    //    Vector3 lastPos = posList[posList.Count - 1];

    //    foreach (Vector3 pos in posList)
    //    {
    //        if (isNeedSetPos)
    //        {
    //            AddBuildingToList(pos - setPos);
    //        }
    //        else
    //        {
    //            if (!isUnderObj)
    //            {
    //                AddBuildingToList(pos);
    //            }
    //            else
    //            {
    //                setNum++;
    //                if (buildIndex == 0)
    //                //if (buildingList.Count == 0)
    //                {
    //                    AddBuildingToList(pos);
    //                }
    //                else if (buildIndex > 0)
    //                //else if (buildingList.Count > 0)
    //                {
    //                    UnderBeltCtrl underBeltCtrl = gameObj.GetComponent<UnderBeltCtrl>();
    //                    //isPreBeltSend = gameObj.GetComponent<UnderBeltCtrl>().isSendBelt;

    //                    if ((!isMoveX && (underBeltCtrl.dirNum == 0 || underBeltCtrl.dirNum == 2)) ||
    //                        (isMoveX && (underBeltCtrl.dirNum == 1 || underBeltCtrl.dirNum == 3)))
    //                    {
    //                        if (posList.Count <= 11 && setNum == posList.Count)
    //                        {
    //                            AddBuildingToList(pos);
    //                        }
    //                        else if (posList.Count > 11)
    //                        {
    //                            if (pos == lastPos || setNum == 1 || setNum % 11 == 0)
    //                            {
    //                                AddBuildingToList(pos);
    //                                if (setNum % 11 == 0)
    //                                {
    //                                    setNum = 0;
    //                                }
    //                            }
    //                        }
    //                    }
    //                    else
    //                    {
    //                        AddBuildingToList(pos);
    //                    }
    //                }
    //                buildIndex++;
    //            }
    //        }
    //    }
    //}

    void AddBuildingToList(Vector3 pos)
    {
        GameObject obj = Instantiate(gameObj, pos, Quaternion.identity);
        obj.SetActive(true);
        buildingList.Add(obj);
    }

    public void SetImage(GameObject game, int level, int height, int width)
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
            factory.level = level;
            if (factory.dirCount == 4)
            {
                isGetDir = true;
                dirNum = factory.dirNum;
            }
            else
            {
                isGetDir = false;
                dirNum = 0;
            }
        }
        else if (gameObj.TryGetComponent(out TowerAi tower))
        {
            tower.isPreBuilding = true;
            //tower.DisableColliders();
            tower.ColliderTriggerOnOff(true);
            tower.level = level;
            isGetDir = false;
            dirNum = 0;
        }
        else if (gameObj.TryGetComponent(out BeltGroupMgr belt))
        {
            belt.isPreBuilding = true;
            belt.SetBelt(0);
            belt.beltList[0].isPreBuilding = true;
            belt.beltList[0].ColliderTriggerOnOff(true);
            //belt.beltList[0].DisableColliders();
            belt.beltList[0].level = level;
            isGetDir = true;
            dirNum = belt.beltList[0].dirNum;            
        }
        //else if (gameObj.TryGetComponent(out PipeGroupMgr pipe))
        //{
        //    pipe.isPreBuilding = true;
        //    pipe.SetPipe(0);
        //    pipe.pipeList[0].isPreBuilding = true;
        //    pipe.pipeList[0].ColliderTriggerOnOff(true);
        //    //pipe.pipeList[0].DisableColliders();
        //    isUnderObj = true;
        //    isGetDir = true;
        //    dirNum = pipe.pipeList[0].dirNum;            
        //}
        else if (gameObj.TryGetComponent(out UnderBeltCtrl underBelt))
        {
            underBelt.isPreBuilding = true;
            underBelt.SetSendUnderBelt();
            //underBelt.SetLevel(level);
            isUnderObj = true;
            isGetDir = true;
            dirNum = underBelt.dirNum;            
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

        gameObj.transform.parent = this.transform;

        spriteRenderer = gameObj.GetComponentInChildren<SpriteRenderer>();
        layNumTemp = spriteRenderer.sortingOrder;

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
    }
}
