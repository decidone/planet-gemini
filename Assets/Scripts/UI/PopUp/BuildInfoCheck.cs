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
    bool isUIOpen;
    Vector2 mousePos;
    GameManager gameManager;
    InputManager inputManager;

    Vector2 newPos;
    Vector2 lastPos;

    float timer = 0f;
    float delay = 0.2f;

    void Start()
    {
        buildItemInfoWin = GameManager.instance.inventoryUiCanvas.GetComponent<PopUpManager>().buildItemInfo;
        inputManager = InputManager.instance;
        gameManager = GameManager.instance;
        lastPos = new Vector2(-1, -1);
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

        newPos = new Vector2(x, y);

        if (newPos != lastPos)
        {
            lastPos = newPos;
            if (gameManager.map.IsOnMap(x, y) &&
                (mousePos.x >= 0 && mousePos.x <= Screen.width && mousePos.y >= 0 && mousePos.y <= Screen.height))
            {
                Cell cell = gameManager.map.GetCellDataFromPos(x, y);

                if (cell.structure)
                {
                    cell.structure.TryGetComponent(out Structure str);
                    PopUpPosSetStructure(str);
                    PopUpPosSet(mousePos);
                    isUIOpen = true;
                    selectedStr = str;
                }
                else if (cell.resource)
                {
                    if (MapGenerator.instance.CheckFogState(pos) == 0)
                    {
                        PopUpPosSetResource(cell.resource.item);
                        PopUpPosSet(mousePos);
                        isUIOpen = true;
                    }
                    selectedStr = null;
                }
                else
                {
                    selectedStr = null;
                    isUIOpen = false;
                    BuildItemInfoPopUpOff();
                }
            }
        }

        if (selectedStr)
        {
            timer += Time.deltaTime;
            if (timer >= delay)
            {
                timer = 0;

                PopUpPosSetStructure(selectedStr);
            }
        }

        if (isUIOpen)
        {
            PopUpPosSet(mousePos);
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

    void PopUpPosSetStructure(Structure structure)
    {
        Dictionary<Item, int> getDic = structure.PopUpItemCheck();
        (bool, bool, bool, EnergyGroup, float) energyState = structure.PopUpEnergyCheck();
        (float, float) storedState = structure.PopUpStoredCheck();

        if ((getDic != null && getDic.Count > 0) || energyState.Item1 || energyState.Item2)
        {
            BuildItemInfoPopUpOn();
            buildItemInfoWin.UiSetting(getDic, energyState.Item1, energyState.Item2, energyState.Item3, energyState.Item4, energyState.Item5, storedState.Item1, storedState.Item2);
        }
        else
            BuildItemInfoPopUpOff();
    }

    void PopUpPosSetResource(Item item)
    {
        BuildItemInfoPopUpOn();
        buildItemInfoWin.UiSetting(item);
    }

    void PopUpPosSet(Vector2 pos)
    {
        // 캔버스 공간에서의 마우스 좌표로 변환
        Vector2 anchoredPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, pos, null, out anchoredPos);

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
    }
}
