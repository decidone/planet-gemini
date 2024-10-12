using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildInfoCheck : MonoBehaviour
{
    public RectTransform canvasRectTransform; // 캔버스의 RectTransform
    public RectTransform imageRectTransform;  // 팝업 이미지의 RectTransform
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
        if (Time.timeScale == 0)
        {
            return;
        }

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
                else if (cell.structure != null)
                {
                    if (inputManager.isMapOpened && cell.structure.TryGetComponent(out Structure structure) && !structure.isPreBuilding)
                    {
                        PopUpPosSetStructure(mousePos, structure);
                    }
                }
            }
            else if (cell.structure != null)
            {
                if (inputManager.isMapOpened && cell.structure.TryGetComponent(out Structure structure) && !structure.isPreBuilding)
                {
                    PopUpPosSetStructure(mousePos, structure);
                }
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
        Vector2 mousePos = Input.mousePosition;

        // 캔버스 공간에서의 마우스 좌표로 변환
        Vector2 anchoredPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, mousePos, null, out anchoredPos);

        // 팝업 이미지의 크기 가져오기
        float popupWidth = imageRectTransform.rect.width;
        float popupHeight = imageRectTransform.rect.height;

        // 새 좌표 계산 (마우스 위치에 대해 오프셋 적용)
        // 왼쪽 위에 위치하도록 마우스 좌표에서 팝업 크기만큼 뺌
        Vector2 newPos = new Vector2(anchoredPos.x + popupWidth / 2, anchoredPos.y + popupHeight / 2);
        // 팝업이 화면 밖으로 나가지 않도록 클램핑
        float clampedX = Mathf.Clamp(newPos.x, -canvasRectTransform.rect.width / 2 + popupWidth / 2, canvasRectTransform.rect.width / 2 - popupWidth / 2);
        float clampedY = Mathf.Clamp(newPos.y, -canvasRectTransform.rect.height / 2 + popupHeight / 2, canvasRectTransform.rect.height / 2 - popupHeight / 2);

        // 위치 설정
        buildItemInfoWin.gameObject.transform.localPosition = new Vector2(clampedX, clampedY);

        //RectTransform popUpRect = buildItemInfoWin.GetComponent<RectTransform>();
        //Vector2 newPos = new Vector2(pos.x + popUpRect.rect.width / 2, pos.y - popUpRect.rect.height / 2);
        //buildItemInfoWin.gameObject.transform.position = newPos;
        Dictionary<Item, int> getDic = structure.PopUpItemCheck();
        (bool, bool, EnergyGroup) energyState = structure.PopUpEnergyCheck();

        if ((getDic != null && getDic.Count > 0) || energyState.Item1 || energyState.Item2)
        {
            BuildItemInfoPopUpOn();
            buildItemInfoWin.UiSetting(getDic, energyState.Item1, energyState.Item2, energyState.Item3);
        }
        else
            BuildItemInfoPopUpOff();
    }

    void PopUpPosSetResource(Vector2 pos, Item item)
    {
        Vector2 mousePos = Input.mousePosition;

        // 캔버스 공간에서의 마우스 좌표로 변환
        Vector2 anchoredPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, mousePos, null, out anchoredPos);

        // 팝업 이미지의 크기 가져오기
        float popupWidth = imageRectTransform.rect.width;
        float popupHeight = imageRectTransform.rect.height;

        // 새 좌표 계산 (마우스 위치에 대해 오프셋 적용)
        // 왼쪽 위에 위치하도록 마우스 좌표에서 팝업 크기만큼 뺌
        Vector2 newPos = new Vector2(anchoredPos.x + popupWidth / 2, anchoredPos.y + popupHeight / 2);
        // 팝업이 화면 밖으로 나가지 않도록 클램핑
        float clampedX = Mathf.Clamp(newPos.x, -canvasRectTransform.rect.width / 2 + popupWidth / 2, canvasRectTransform.rect.width / 2 - popupWidth / 2);
        float clampedY = Mathf.Clamp(newPos.y, -canvasRectTransform.rect.height / 2 + popupHeight / 2, canvasRectTransform.rect.height / 2 - popupHeight / 2);

        // 위치 설정
        buildItemInfoWin.gameObject.transform.localPosition = new Vector2(clampedX, clampedY);

        //RectTransform popUpRect = buildItemInfoWin.GetComponent<RectTransform>();
        //Vector2 newPos = new Vector2(pos.x + popUpRect.rect.width / 2, pos.y - popUpRect.rect.height / 2);
        //buildItemInfoWin.gameObject.transform.position = newPos;

        BuildItemInfoPopUpOn();
        buildItemInfoWin.UiSetting(item);
    }
}
