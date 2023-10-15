using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildInfoCheck : MonoBehaviour
{
    public BuildItemInfoWin buildItemInfoWin;
    Structure selectBuild;
    Vector2 mousePos;
    bool isMouseOnBuild = false;

    InputManager inputManager;

    void Start()
    {
        buildItemInfoWin = GameManager.instance.inventoryUiCanvas.GetComponent<PopUpManager>().buildItemInfo;
        inputManager = InputManager.instance;
    }

    void Update()
    {
        //Input State Control
        if (!inputManager.hoverInfo) return;

        mousePos = Input.mousePosition;

        if (!EventSystem.current.IsPointerOverGameObject())
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(mousePos), Vector2.zero);
            if (hit.collider != null && !isMouseOnBuild && hit.collider.TryGetComponent(out Structure structure) && !structure.isPreBuilding)
            {
                if(selectBuild != null || selectBuild != structure)
                {
                    isMouseOnBuild = true;
                    selectBuild = structure;
                }
            }
            else if (hit.collider != null && isMouseOnBuild)
            {
                PopUpPosSet(mousePos);
            }
            else if (hit.collider == null && isMouseOnBuild)
            {
                isMouseOnBuild = false;
                BuildItemInfoPopUpOff();
            }
        }
        else if (selectBuild != null)
        {
            selectBuild = null;
            isMouseOnBuild = false;
            BuildItemInfoPopUpOff();
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

    void PopUpPosSet(Vector2 pos)
    {
        RectTransform popUpRect = buildItemInfoWin.GetComponent<RectTransform>();
        Vector2 newPos = new Vector2(pos.x + popUpRect.rect.width / 2, pos.y - popUpRect.rect.height / 2);

        buildItemInfoWin.gameObject.transform.position = newPos;
        Dictionary<Item, int> getDic = selectBuild.PopUpItemCheck();

        if (selectBuild != null && getDic != null && getDic.Count > 0)
        {
            BuildItemInfoPopUpOn();
            buildItemInfoWin.UiSetting(getDic);
        }
    }
}
