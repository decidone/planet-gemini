using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// UTF-8 설정
public class UnitDrag : DragFunc
{
    //int unitLayer;
    //int monsterLayer;
    //int spawnerLayer;

    public delegate void AddUnitDelegate(GameObject obj);
    public static event AddUnitDelegate addUnit;
    
    public delegate void RemoveUnitDelegate();
    public static event RemoveUnitDelegate removeUnit;

    public delegate void TargetDelegate(Vector3 targetPos, bool isAttack, bool playerUnitPortalIn);
    public static event TargetDelegate targetSet;    
    
    public delegate void PatrolDelegate(Vector3 patrolPos);
    public static event PatrolDelegate patrolSet;

    public delegate void GroupCenterDelegate();
    public static event GroupCenterDelegate groupCenterSet;    
    
    public delegate void UnitHoldDelegate();
    public static event UnitHoldDelegate unitHoldSet;

    public delegate void MonsterTargetDelegate(GameObject obj);
    public static event MonsterTargetDelegate monsterTargetSet;

    private Vector2 targetPosition;

    bool unitCtrlKeyPressed = false;
    bool isPKeyPressed = false;
    bool isAKeyPressed = false;
    bool isUnitRemove = false;

    public bool playerAttackClick;

    InputManager inputManager;
    UnitRemovePopup unitRemovePopup;
    List<UnitAi> removeUnitList = new List<UnitAi>();

    public static UnitDrag instance;
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    protected override void Start()
    {
        base.Start();
        unitRemovePopup = UnitRemovePopup.instance;
    }

    void OnEnable()
    {
        inputManager = InputManager.instance;
        inputManager.controls.Unit.Attack.performed += Attack;
        inputManager.controls.Unit.Patrol.performed += Patrol;
        inputManager.controls.Unit.Hold.performed += Hold;
    }
    void OnDisable()
    {
        inputManager.controls.Unit.Attack.performed -= Attack;
        inputManager.controls.Unit.Patrol.performed -= Patrol;
        inputManager.controls.Unit.Hold.performed -= Hold;
    }

    public void Attack()
    {
        if (selectedObjects.Length > 0)
        {
            if (UpgradeRemoveBtn.instance.currentBtn == UpgradeRemoveBtn.SelectedButton.None)
                MouseSkin.instance.UnitCursorCursorSet(true);
            isAKeyPressed = true;
            unitCtrlKeyPressed = true;
        }
    }

    void Attack(InputAction.CallbackContext ctx)
    {
        Attack();
    }

    public void Patrol()
    {
        if (selectedObjects.Length > 0)
        {
            if (UpgradeRemoveBtn.instance.currentBtn == UpgradeRemoveBtn.SelectedButton.None)
                MouseSkin.instance.UnitCursorCursorSet(false);
            isPKeyPressed = true;
            unitCtrlKeyPressed = true;
        }
    }

    void Patrol(InputAction.CallbackContext ctx)
    {
        Patrol();
    }

    public void Hold()
    {
        if (selectedObjects.Length > 0)
        {
            unitHoldSet?.Invoke();
        }
    }

    void Hold(InputAction.CallbackContext ctx)
    {
        Hold();
    }

    public override void LeftMouseUp(Vector2 startPos, Vector2 endPos)
    {
        if (isUnitRemove)
        {
            removeUnitList.Clear();

            if (startPos != endPos)
            {
                Collider2D[] colliders = Physics2D.OverlapAreaAll(startPos, endPos, 1 << interactLayer);
                List<UnitAi> selectedObjectsList = new List<UnitAi>();

                foreach (Collider2D collider in colliders)
                {
                    if (collider.GetComponentInParent<UnitAi>() == null)
                        continue;
                    if (collider.GetComponentInParent<Portal>() || collider.GetComponentInParent<ScienceBuilding>())
                        continue;

                    selectedObjectsList.Add(collider.GetComponentInParent<UnitAi>());
                }

                if (selectedObjectsList.Count > 0)
                {
                    foreach (UnitAi obj in selectedObjectsList)
                    {
                        removeUnitList.Add(obj);
                    }
                }
            }
            else
            {
                RaycastHit2D hit = Physics2D.Raycast(endPos, Vector2.zero, 0f, 1 << interactLayer);
                if (hit)
                {
                    if (!hit.collider.GetComponentInParent<UnitAi>())
                        return;
                    UnitAi unitAi = hit.collider.GetComponentInParent<UnitAi>();
                    removeUnitList.Add(unitAi);
                }
                else
                {
                    selectedObjects = new GameObject[0];
                }
            }

            if (removeUnitList.Count > 0)
            {
                Dictionary<(string,int), int> removeUnitIndexCount = new Dictionary<(string, int), int>();
                int sellPrice = 0;
                for (int i = 0; i < removeUnitList.Count; i++)
                {
                    string unitIndex = removeUnitList[i].unitName;
                    if (!removeUnitIndexCount.ContainsKey((unitIndex, removeUnitList[i].unitLevel)))
                    {
                        removeUnitIndexCount.Add((unitIndex, removeUnitList[i].unitLevel), 0);
                    }
                    removeUnitIndexCount[(unitIndex, removeUnitList[i].unitLevel)]++;
                    sellPrice += removeUnitList[i].unitCommonData.sellPrice;
                }
                unitRemovePopup.OpenPopup(removeUnitIndexCount, sellPrice);
            }
        }
        else if (!playerAttackClick)
        {
            if (!unitCtrlKeyPressed)
            {
                if (startPos != endPos)
                    GroupSelectedObjects(startPos, endPos);
                else
                {
                    RaycastHit2D hit = Physics2D.Raycast(endPos, Vector2.zero, 0f, 1 << interactLayer);
                    if (hit)
                    {
                        SelectedObjects(hit);
                    }
                    else
                    {
                        selectedObjects = new GameObject[0];
                        removeUnit?.Invoke();
                    }
                }
            }
            else if (isPKeyPressed)
            {
                groupCenterSet?.Invoke();
                patrolSet?.Invoke(endPos);
                UnitMovePos.instance.AnimStart(endPos);
            }
            else if (isAKeyPressed)
            {
                RaycastHit2D hit = Physics2D.Raycast(endPos, Vector2.zero, 0f, 1 << interactLayer);
                if (!hit)
                {
                    SetTargetPosition(true, endPos);
                }
                else if (hit.collider.GetComponentInParent<MonsterAi>())
                {
                    monsterTargetSet?.Invoke(hit.collider.GetComponentInParent<MonsterAi>().gameObject);
                }
                else if (hit.collider.GetComponentInParent<MonsterSpawner>())
                {
                    monsterTargetSet?.Invoke(hit.collider.GetComponentInParent<MonsterSpawner>().gameObject);
                }

                UnitMovePos.instance.AnimStart(endPos);
            }
        }
        ReSetBool();
    }

    public void LeftMouseDoubleClick(Vector2 startPos, Vector2 endPos)
    {
        if (isUnitRemove)
            return;

        if (!unitCtrlKeyPressed)
        {
            RaycastHit2D hit = Physics2D.Raycast(endPos, Vector2.zero, 0f, 1 << interactLayer);
            if (hit)
                SelectedSameUnit(hit);
            else
            {
                selectedObjects = new GameObject[0];
                removeUnit?.Invoke();
                ReSetBool();
            }
        }
        ReSetBool();
    }

    public override void RightMouseUp(Vector2 startPos, Vector2 endPos)
    {
        if (!playerAttackClick)
        {
            SetTargetPosition(false, endPos);
            UnitMovePos.instance.AnimStart(endPos);
            ReSetBool();
        }
    }

    protected override void GroupSelectedObjects(Vector2 startPosition, Vector2 endPosition)
    {
        Collider2D[] colliders = Physics2D.OverlapAreaAll(startPosition, endPosition, 1 << interactLayer);
        List<GameObject> selectedObjectsList = new List<GameObject>();

        foreach (Collider2D collider in colliders)
        {
            if (collider.GetComponentInParent<UnitAi>() == null || collider.GetComponentInParent<TankCtrl>())
                continue;
            if (collider.GetComponentInParent<Portal>() || collider.GetComponentInParent<ScienceBuilding>())
                continue;

            selectedObjectsList.Add(collider.GetComponentInParent<UnitAi>().gameObject);
        }
        selectedObjects = selectedObjectsList.ToArray();

        removeUnit?.Invoke();

        if (selectedObjects.Length > 0)
        {
            foreach (GameObject obj in selectedObjects)
            {
                addUnit?.Invoke(obj);
            }
            BasicUIBtns.instance.SwapFunc(false);
        }

        if (selectedObjects.Length == 1)
            InfoUI.instance.SetUnitInfo(selectedObjects[0].GetComponentInParent<UnitAi>());
        else
        {
            List<UnitAi> unitAiList = selectedObjectsList.Select(obj => obj.GetComponent<UnitAi>()).ToList();
            InfoUI.instance.SetDefault();
            InfoUI.instance.UnitGroupUISet(unitAiList);
        }
    }

    private void SelectedObjects(RaycastHit2D ray)
    {
        if (!ray.collider.GetComponentInParent<UnitAi>() || ray.collider.GetComponentInParent<TankCtrl>())
            return;
        GameObject gameObject = ray.collider.GetComponentInParent<UnitAi>().gameObject; 
        removeUnit?.Invoke();
        selectedObjects = new GameObject[1];
        selectedObjects[0] = gameObject;

        addUnit?.Invoke(gameObject);
        BasicUIBtns.instance.SwapFunc(false);

        InfoUI.instance.SetUnitInfo(gameObject.GetComponentInParent<UnitAi>());
    }

    private void SelectedSameUnit(RaycastHit2D ray)
    {
        // 클릭 대상이 UnitAi가 아니거나 TankCtrl이면 리턴
        if (!ray.collider.GetComponentInParent<UnitAi>() || ray.collider.GetComponentInParent<TankCtrl>())
            return;

        // 기본 선택 유닛 정보 가져오기
        GameObject gameObject = ray.collider.GetComponentInParent<UnitAi>().gameObject;
        removeUnit?.Invoke();
        gameObject.TryGetComponent(out UnitAi unit);
        int unitIndex = unit.unitIndex;

        // 대상 유닛 리스트
        UnitAi[] unitAis = System.Array.Empty<UnitAi>();
        List<GameObject> selectedObjectsList = new List<GameObject>();

        if (unitIndex == 0)
        {
            unitAis = Object.FindObjectsByType<BounceRobot>(FindObjectsSortMode.None);
        }
        else if (unitIndex == 1)
        {
            unitAis = Object.FindObjectsByType<SentryCopterCtrl>(FindObjectsSortMode.None);
        }
        else if (unitIndex == 2)
        {
            unitAis = Object.FindObjectsByType<SpinRobot>(FindObjectsSortMode.None);
        }
        else if (unitIndex == 3)
        {
            unitAis = Object.FindObjectsByType<CorrosionDrone>(FindObjectsSortMode.None);
        }
        else if (unitIndex == 4)
        {
            unitAis = Object.FindObjectsByType<RepairerDrone>(FindObjectsSortMode.None);
        }
        else if (unitIndex == 5)
        {
            unitAis = Object.FindObjectsByType<TankCtrl>(FindObjectsSortMode.None);
        }

        // 화면 안에 있고, 같은 레벨의 유닛만 선택
        foreach (var obj in unitAis)
        {
            if (obj.TryGetComponent(out UnitAi _unit) && unit.unitLevel == _unit.unitLevel)
            {
                Vector3 viewportPos = Camera.main.WorldToViewportPoint(obj.transform.position);
                if (viewportPos.z > 0 && viewportPos.x >= 0 && viewportPos.x <= 1 &&
                    viewportPos.y >= 0 && viewportPos.y <= 1)
                {
                    selectedObjectsList.Add(obj.gameObject);
                }
            }
        }

        selectedObjects = selectedObjectsList.ToArray();
        removeUnit?.Invoke();

        // 선택된 오브젝트가 있다면 모두 선택 상태로 추가
        if (selectedObjects.Length > 0)
        {
            foreach (GameObject obj in selectedObjects)
            {
                addUnit?.Invoke(obj);
            }
            BasicUIBtns.instance.SwapFunc(false);
        }

        // UI 갱신
        List<UnitAi> unitAiList = selectedObjectsList
            .Select(obj => obj.GetComponent<UnitAi>())
            .ToList();

        InfoUI.instance.SetUnitInfo(unitAiList);
    }

    void SetTargetPosition(bool isAttack, Vector2 targetPos)
    {
        groupCenterSet?.Invoke();
        bool isPlayerUnitPortalIn = false;
        if (!isAttack)
        {
            int x = Mathf.FloorToInt(targetPos.x);
            int y = Mathf.FloorToInt(targetPos.y);

            Map map;
            if (GameManager.instance.isPlayerInHostMap)
                map = GameManager.instance.hostMap;
            else
                map = GameManager.instance.clientMap;

            Cell cell = map.GetCellDataFromPos(x, y);
            if (cell.structure && cell.structure.GetComponent<PortalUnitIn>())
            {
                isPlayerUnitPortalIn = true;
            }
        }

        targetSet?.Invoke(targetPos, isAttack, isPlayerUnitPortalIn);
    }

    void ReSetBool()
    {
        unitCtrlKeyPressed = false;
        isPKeyPressed = false;
        isAKeyPressed = false;
        if(!isUnitRemove)
            MouseSkin.instance.ResetCursor();
    }

    public void UnitRemove()
    {
        isUnitRemove = true;
        MouseSkin.instance.DragCursorSet(true);
    }

    public void UnitRemoveCancel()
    {
        isUnitRemove = false;
        MouseSkin.instance.ResetCursor();
        removeUnitList.Clear();
    }

    public void UnitRemoveFunc()
    {
        List<UnitAi> rmUnit = new List<UnitAi>(removeUnitList);
        int sellPrice = 0;
        for (int i = 0; i < rmUnit.Count; i++)
        {
            if (removeUnitList[i])
            {
                removeUnitList[i].DieFuncServerRpc();
                sellPrice += rmUnit[i].unitCommonData.sellPrice;
            }
        }

        GameManager.instance.AddScrapServerRpc(sellPrice);
    }
}
