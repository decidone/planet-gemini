using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildInfoCheck : MonoBehaviour
{
    public BuildItemInfoWin buildItemInfoWin;
    Structure selectedStr;
    Vector2 mousePos;
    bool isMouseOnStr;
    GameManager gameManager;
    InputManager inputManager;

    void Start()
    {
        buildItemInfoWin = GameManager.instance.inventoryUiCanvas.GetComponent<PopUpManager>().buildItemInfo;
        inputManager = InputManager.instance;
        gameManager = GameManager.instance;
    }

    void Update()
    {
        mousePos = Input.mousePosition;

        if (RaycastUtility.IsPointerOverUI(mousePos))
        {
            if(buildItemInfoWin.gameObject.activeSelf)
                buildItemInfoWin.gameObject.SetActive(false);
            return;
        }

        Vector2 pos = new Vector2();
        int x;
        int y;

        if (!inputManager.isMapOpened)
            pos = Camera.main.ScreenToWorldPoint(mousePos);
        else
            pos = MapCameraController.instance.cam.ScreenToWorldPoint(mousePos);

        x = Mathf.FloorToInt(pos.x);
        y = Mathf.FloorToInt(pos.y);

        RaycastHit2D[] hits = Physics2D.RaycastAll(pos, Vector2.zero);
        if (hits.Length > 0)
        {
            isMouseOnStr = false;
            selectedStr = null;
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider.TryGetComponent(out InfoInteract info))
                {
                    GameObject parent = info.transform.parent.gameObject;
                    if (parent.TryGetComponent(out Structure str))
                    {
                        isMouseOnStr = true;
                        selectedStr = str;
                    }
                }
            }
        }
        else
        {
            selectedStr = null;
            isMouseOnStr = false;
            BuildItemInfoPopUpOff();
        }

        if (gameManager.map.IsOnMap(x, y))
        {
            Cell cell = gameManager.map.GetCellDataFromPos(x, y);
            if (isMouseOnStr)
            {
                if (!inputManager.isMapOpened && !selectedStr.isPreBuilding)
                {
                    PopUpPosSetStructure(mousePos, selectedStr);
                }
            }
            else if (cell.structure != null)
            {
                if (inputManager.isMapOpened && cell.structure.TryGetComponent(out Structure structure) && !structure.isPreBuilding)
                {
                    PopUpPosSetStructure(mousePos, structure);
                }
                //if (!isMouseOnBuild && cell.structure.TryGetComponent(out Structure structure) && !structure.isPreBuilding)
                //{
                //    if (selectBuild != null || selectBuild != structure)
                //    {
                //        isMouseOnBuild = true;
                //        selectBuild = structure;
                //    }
                //}
                //else if (isMouseOnBuild)
                //{
                //    PopUpPosSetStructure(mousePos);
                //}
            }
            else if (cell.resource != null)
            {
                PopUpPosSetResource(mousePos, cell.resource.item);
            }
            else
            {
                selectedStr = null;
                isMouseOnStr = false;
                BuildItemInfoPopUpOff();
            }
        }

        //if (!EventSystem.current.IsPointerOverGameObject())
        //{
        //    RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(mousePos), Vector2.zero);
        //    if (hit.collider != null && !isMouseOnBuild && hit.collider.TryGetComponent(out Structure structure) && !structure.isPreBuilding)
        //    {
        //        if(selectBuild != null || selectBuild != structure)
        //        {
        //            isMouseOnBuild = true;
        //            selectBuild = structure;
        //        }
        //    }
        //    else if (hit.collider != null && isMouseOnBuild)
        //    {
        //        PopUpPosSet(mousePos);
        //    }
        //    else if (hit.collider == null && isMouseOnBuild)
        //    {
        //        isMouseOnBuild = false;
        //        BuildItemInfoPopUpOff();
        //    }
        //}
        //else if (selectBuild != null)
        //{
        //    selectBuild = null;
        //    isMouseOnBuild = false;
        //    BuildItemInfoPopUpOff();
        //}
    }

    void BuildItemInfoPopUpOn()
    {
        buildItemInfoWin.gameObject.SetActive(true);
    }

    void BuildItemInfoPopUpOff()
    {
        buildItemInfoWin.ResetUi();
        buildItemInfoWin.gameObject.SetActive(false);
    }

    void PopUpPosSetStructure(Vector2 pos, Structure structure)
    {
        RectTransform popUpRect = buildItemInfoWin.GetComponent<RectTransform>();
        Vector2 newPos = new Vector2(pos.x + popUpRect.rect.width / 2, pos.y - popUpRect.rect.height / 2);

        buildItemInfoWin.gameObject.transform.position = newPos;
        Dictionary<Item, int> getDic = structure.PopUpItemCheck();
        (bool, bool, EnergyGroup) energyState = structure.PopUpEnergyCheck();

        if ((getDic != null && getDic.Count > 0) || energyState.Item1 || energyState.Item2)
        {
            BuildItemInfoPopUpOn();
            buildItemInfoWin.UiSetting(getDic, energyState.Item1, energyState.Item2, energyState.Item3);
        }
        else
            BuildItemInfoPopUpOff();

        //Dictionary<Item, int> getDic = selectBuild.PopUpItemCheck();
        //(bool, float) energyState = selectBuild.PopUpEnergyCheck();

        //if (selectBuild != null && getDic != null && getDic.Count > 0)
        //{
        //    BuildItemInfoPopUpOn();
        //    buildItemInfoWin.UiSetting(getDic, energyState.Item1, energyState.Item2);
        //}
        //else
        //    BuildItemInfoPopUpOff();
    }

    void PopUpPosSetResource(Vector2 pos, Item item)
    {
        RectTransform popUpRect = buildItemInfoWin.GetComponent<RectTransform>();
        Vector2 newPos = new Vector2(pos.x + popUpRect.rect.width / 2, pos.y - popUpRect.rect.height / 2);

        buildItemInfoWin.gameObject.transform.position = newPos;

        BuildItemInfoPopUpOn();
        buildItemInfoWin.UiSetting(item);
    }
}
