using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BeltPreBuilding : PreBuilding
{
    bool shiftKey;

    #region Singleton
    public static BeltPreBuilding instanceBeltBuilding;

    protected override void Awake()
    {
        if (instanceBeltBuilding != null)
        {
            Destroy(gameObject);
            return;
        }

        instanceBeltBuilding = this;
    }
    #endregion

    protected override void FixedUpdate()
    {
        if (isEnough && mouseHoldCheck && isBuildingOn)
        {
            if (buildingList.Count <= canBuildCount)
            {
                tempPos = transform.position;
                float tempDeltaX = tempPos.x - endBuildPos.x;
                float tempDeltaY = tempPos.y - endBuildPos.y;
                int tempAbsDeltaX = Mathf.Abs((int)tempDeltaX);
                int tempAbsDeltaY = Mathf.Abs((int)tempDeltaY);
                if (tempAbsDeltaX >= 1 || tempAbsDeltaY >= 1)
                {
                    endBuildPos = tempPos;
                    CheckPos(endBuildPos, (int)tempDeltaX, (int)tempDeltaY);
                    isDrag = true;
                }
            }
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        inputManager.controls.Building.ShiftBuildingDown.performed += ShiftKeyDown;
        inputManager.controls.Building.ShiftBuildingUp.performed += ShiftKeyUp;
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        inputManager.controls.Building.ShiftBuildingDown.performed -= ShiftKeyDown;
        inputManager.controls.Building.ShiftBuildingUp.performed -= ShiftKeyUp;
    }

    protected void ShiftKeyDown(InputAction.CallbackContext ctx)
    {
        shiftKey = true;

        if (isBuildingOn)
        {
            if (dirNum ==  0 || dirNum == 2)
            {
                MouseSkin.instance.BuildingCursorSet(3);
            }
            else if (dirNum == 1 || dirNum == 3)
            {
                MouseSkin.instance.BuildingCursorSet(2);
            }
        }

    }

    protected void ShiftKeyUp(InputAction.CallbackContext ctx)
    {
        shiftKey = false;
        if (mouseHoldCheck)
        {
            float tempDeltaX = endBuildPos.x - posList[posList.Count - 1].x;
            float tempDeltaY = endBuildPos.y - posList[posList.Count - 1].y;

            CheckPos(endBuildPos, (int)tempDeltaX, (int)tempDeltaY);
        }
        if (isBuildingOn)
        {
            MouseSkin.instance.BuildingCursorSet(1);
            BasicUIBtns.instance.SetRotateUI(true);
        }
    }

    [Command]
    protected override void LeftMouseButtonUpCommand(InputAction.CallbackContext ctx)
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
                    Vector3[] pos = new Vector3[buildingList.Count];
                    int[] dir = new int[buildingList.Count];
                    for (int i = 0; i < buildingList.Count; i++)
                    {
                        pos[i] = buildingList[i].transform.position;
                        dir[i] = (int)buildingList[i].GetComponent<PreBuildingImg>().animator.GetFloat("DirNum");
                    }
                    BuildingServerRpc(isInHostMap, buildingIndex, pos, dir, isBeltObj, reversSet, gameManager.debug);
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
                PreBuildingImg preBuildingImg = nonNetObj.GetComponent<PreBuildingImg>();

                preBuildingImg.AnimSetFloat("DirNum", dirNum);
                if (isBeltObj)
                    preBuildingImg.AnimSetFloat("Level", level);
     
                posList.Clear();
                isDrag = false;
            }

            mouseHoldCheck = false;
        }
    }

    protected void CheckPos(Vector3 setPos, int moveX, int moveY)
    {
        Vector2 dir = new Vector2(
            moveX == 0 ? 0 : Mathf.Sign(moveX),
            moveY == 0 ? 0 : Mathf.Sign(moveY)
        );

        if (!shiftKey)
        {
            if (posList.Count == 0)
            {
                RotationDirChcek(dir);
                SetPos(startBuildPos);
            }

            if (!posList.Contains(setPos))
            {
                Vector2 newPos;

                int moveXPos = Mathf.Abs(moveX);
                int moveYPos = Mathf.Abs(moveY);

                if (moveXPos > 0 && moveYPos > 0) // 대각선으로 이동
                {
                    int moveXCount = moveXPos;
                    int moveYCount = moveYPos;
                    int setCount;
                    while (moveXCount != 0 && moveYCount != 0)
                    {
                        if (Mathf.Abs(moveXCount) > Mathf.Abs(moveYCount))    // x가 더 큰경우
                        {
                            setCount = Mathf.FloorToInt(moveXCount / 3);
                            RotationDirChcek(new Vector2(dir.x, 0));
                            if (setCount == 0)
                            {
                                newPos = new Vector2(posList[posList.Count - 1].x + dir.x, posList[posList.Count - 1].y);
                                if (posList.Contains(newPos))
                                {
                                    RemoveSet(newPos);
                                }
                                SetPos(newPos);
                                moveXCount--;
                            }
                            else
                            {
                                for (int i = 0; i < setCount; i++)
                                {
                                    newPos = new Vector2(posList[posList.Count - 1].x + dir.x, posList[posList.Count - 1].y);
                                    if (posList.Contains(newPos))
                                    {
                                        RemoveSet(newPos);
                                    }
                                    SetPos(newPos);
                                    moveXCount--;
                                }
                            }
                        }
                        else if (Mathf.Abs(moveXCount) < Mathf.Abs(moveYCount))    // y가 더 큰경우
                        {
                            setCount = Mathf.FloorToInt(moveYCount / 3);
                            RotationDirChcek(new Vector2(0, dir.y));
                            if (setCount == 0)
                            {
                                newPos = new Vector2(posList[posList.Count - 1].x, posList[posList.Count - 1].y + dir.y);
                                if (posList.Contains(newPos))
                                {
                                    RemoveSet(newPos);
                                }
                                SetPos(newPos);
                                moveYCount--;
                            }
                            else
                            {
                                for (int i = 0; i < setCount; i++)
                                {
                                    newPos = new Vector2(posList[posList.Count - 1].x, posList[posList.Count - 1].y + dir.y);
                                    if (posList.Contains(newPos))
                                    {
                                        RemoveSet(newPos);
                                    }
                                    SetPos(newPos);
                                    moveYCount--;
                                }
                            }
                        }
                        else    // 같은 경우
                        {
                            RotationDirChcek(new Vector2(dir.x, 0));
                            newPos = new Vector2(posList[posList.Count - 1].x + dir.x, posList[posList.Count - 1].y);
                            if (posList.Contains(newPos))
                            {
                                RemoveSet(newPos);
                            }
                            SetPos(newPos);

                            RotationDirChcek(new Vector2(0, dir.y));
                            newPos = new Vector2(posList[posList.Count - 1].x, posList[posList.Count - 1].y + dir.y);
                            if (posList.Contains(newPos))
                            {
                                RemoveSet(newPos);
                            }
                            SetPos(newPos);
                            moveXCount--;
                            moveYCount--;
                        }
                    }
                }
                else // 직선으로 이동
                {
                    if (moveXPos > 0)
                    {
                        RotationDirChcek(new Vector2(dir.x, 0));
                        for (int i = 0; i < moveXPos; i++)
                        {
                            newPos = new Vector2(posList[posList.Count - 1].x + dir.x, posList[posList.Count - 1].y);
                            if (posList.Contains(newPos))
                            {
                                RemoveSet(newPos);
                                RotationDirChcek(new Vector2(dir.x, 0));
                            }
                            SetPos(newPos);
                        }
                    }
                    else if (moveYPos > 0)
                    {
                        RotationDirChcek(new Vector2(0, dir.y));
                        for (int j = 0; j < moveYPos; j++)
                        {
                            newPos = new Vector2(posList[posList.Count - 1].x, posList[posList.Count - 1].y + dir.y);
                            if (posList.Contains(newPos))
                            {
                                RemoveSet(newPos);
                                RotationDirChcek(new Vector2(0, dir.y));
                            }
                            SetPos(newPos);
                        }
                    }
                }
            }
            else
            {
                RemoveSet(setPos);
            }
        }
        else
        {
            Vector2 newPos;
            if (posList.Count == 0)
            {
                RotationDirChcek(dir);
                SetPos(startBuildPos);
            }

            if (dirNum == 1 || dirNum == 3) //x 방향 이동 시 y방향 락
            {
                setPos = new Vector3(setPos.x, posList[posList.Count - 1].y);
            }
            else if (dirNum == 0 || dirNum == 2) //y 방향 이동 시 x방향 락
            {
                setPos = new Vector3(posList[posList.Count - 1].x, setPos.y);
            }

            if (!posList.Contains(setPos))
            {
                if (dirNum == 0 || dirNum == 2) // 위 아래로 움직이게
                {
                    int moveYPos = Mathf.Abs(moveY);
                    RotationDirChcek(new Vector2(0, dir.y));
                    for (int j = 0; j < moveYPos; j++)
                    {
                        newPos = new Vector2(posList[posList.Count - 1].x, posList[posList.Count - 1].y + dir.y);
                        if (posList.Contains(newPos))
                        {
                            RemoveSet(newPos);
                            RotationDirChcek(new Vector2(0, dir.y));
                        }
                        SetPos(newPos);
                    }
                }
                else if(dirNum == 1 || dirNum == 3) // 좌 우 움직이게
                {
                    int moveXPos = Mathf.Abs(moveX);
                    RotationDirChcek(new Vector2(dir.x, 0));
                    for (int i = 0; i < moveXPos; i++)
                    {
                        newPos = new Vector2(posList[posList.Count - 1].x + dir.x, posList[posList.Count - 1].y);
                        if (posList.Contains(newPos))
                        {
                            RemoveSet(newPos);
                            RotationDirChcek(new Vector2(dir.x, 0));
                        }
                        SetPos(newPos);
                    }
                }
            }
            else
            {
                RemoveSet(setPos);
            }
        }
    }


    protected void SetPos(Vector3 setPos)
    {
        PosListContainCheck(setPos);
        nonNetObj.SetActive(false);
    }

    void RemoveSet(Vector3 setPos)
    {
        int index = posList.IndexOf(setPos) + 1;

        if (buildingList.Count > index)
        {
            for (int i = 0; i < buildingList.Count; i++)
            {
                if (i >= index)
                {
                    Destroy(buildingList[i]);
                }
            }
            buildingList.RemoveRange(index, buildingList.Count - index);
            posList.RemoveRange(index, posList.Count - index);
        }

        if (buildingList.Count > 1)
        {
            buildingList[buildingList.Count - 1].TryGetComponent(out PreBuildingImg preBuildingImg1);
            buildingList[buildingList.Count - 2].TryGetComponent(out PreBuildingImg preBuildingImg2);

            preBuildingImg1.AnimSetFloat("DirNum", (int)preBuildingImg2.animator.GetFloat("DirNum"));
            preBuildingImg1.AnimSetFloat("ModelNum", 0);
        }
        else
        {
            buildingList[buildingList.Count - 1].TryGetComponent(out PreBuildingImg preBuildingImg1);
            preBuildingImg1.AnimSetFloat("DirNum", dirNum);
        }
    }

    void RotationDirChcek(Vector2 movePos)
    {
        PreBuildingImg preBuildingImg = null;
        float preDir = 0;

        if (buildingList.Count > 0)
        {
            preBuildingImg = buildingList[buildingList.Count - 1].GetComponent<PreBuildingImg>();
            preDir = preBuildingImg.animator.GetFloat("DirNum");
        }

        if (movePos.x == 0 && movePos.y == 1) // 위
        {
            if (!shiftKey)
            {
                dirNum = 0;
            }
            else
            {
                if (preBuildingImg != null && preDir != dirNum)
                {
                    if (preDir == 2)
                    {
                        dirNum = 2;
                    }
                    else
                    {
                        dirNum = 0;
                    }
                }
                else
                {
                    dirNum = 0;
                }
            }
        }
        else if (movePos.x == 1 && movePos.y == 0) // 오른쪽
        {
            if (!shiftKey)
            {
                dirNum = 1;
            }
            else
            {
                if (preBuildingImg != null && preDir != dirNum)
                {
                    if (preDir == 3)
                    {
                        dirNum = 3;
                    }
                    else
                    {
                        dirNum = 1;
                    }
                }
                else
                {
                    dirNum = 1;
                }
            }
        }
        else if (movePos.x == 0 && movePos.y == -1) // 아래
        {
            if (!shiftKey)
            {
                dirNum = 2;
            }
            else
            {
                if (preBuildingImg != null && preDir != dirNum)
                {
                    if (preDir == 0)
                    {
                        dirNum = 0;
                    }
                    else
                    {
                        dirNum = 2;
                    }
                }
                else
                {
                    dirNum = 2;
                }
            }
        }
        else if (movePos.x == -1 && movePos.y == 0) // 왼쪽
        {
            if (!shiftKey)
            {
                dirNum = 3;
            }
            else
            {
                if (preBuildingImg != null && preDir != dirNum)
                {
                    if (preDir == 1)
                    {
                        dirNum = 1;
                    }
                    else
                    {
                        dirNum = 3;
                    }
                }
                else
                {
                    dirNum = 3;
                }
            }
        }
        else
        {
            return;
        }

        if (buildingList.Count == canBuildCount)
        {
            return;
        }

        if (preBuildingImg != null)
        {
            int modelNum = ModelCheck(preDir, dirNum);
            if (buildingList.Count != 1)
            {
                preBuildingImg.AnimSetFloat("DirNum", dirNum);
                preBuildingImg.AnimSetFloat("ModelNum", modelNum);
            }
            else if(buildingList.Count == 1)
            {
                preBuildingImg.AnimSetFloat("DirNum", dirNum);
                preBuildingImg.AnimSetFloat("ModelNum", 0);
            }
        }
    }

    int ModelCheck(float preDir, int dir)
    {
        int modelNum = 0;

        if (preDir == 0) // 이전 벨트 방향 위쪽
        {
            if (dir == 1) // 오른쪽으로 꺽이게
            {
                modelNum = 5;
            }
            else if (dir == 3) // 왼으로 꺽이게
            {
                modelNum = 4;
            }
        }
        else if (preDir == 1) // 이전 벨트 방향 오른쪽
        {
            if (dir == 0) // 위쪽 으로 꺽이게
            {
                modelNum = 4;
            }
            else if (dir == 2) // 아래쪽으로 꺽이게
            {
                modelNum = 5;
            }
        }
        else if (preDir == 2) // 이전 벨트 방향 아래쪽
        {
            if (dir == 1) // 오른쪽으로 꺽이게
            {
                modelNum = 4;
            }
            else if (dir == 3) // 왼으로 꺽이게
            {
                modelNum = 5;
            }
        }
        else if (preDir == 3) // 이전 벨트 방향 왼쪽
        {
            if (dir == 0) // 위쪽 으로 꺽이게
            {
                modelNum = 5;
            }
            else if (dir == 2) // 아래쪽으로 꺽이게
            {
                modelNum = 4;
            }
        }

        return modelNum;
    }
}
