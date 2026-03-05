using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

// UTF-8 설정
public class UnitDrag : DragFunc
{
    //int unitLayer;
    //int monsterLayer;
    //int spawnerLayer;

    public delegate void AddUnitDelegate(WorldObj obj);
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

    public delegate void MonsterTargetDelegate(WorldObj obj);
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
                    WorldObj obj = collider.GetComponentInParent<WorldObj>();

                    if (!obj || !obj.Has<UnitAi>())
                        continue;
                    //if (collider.GetComponentInParent<Portal>() || collider.GetComponentInParent<ScienceBuilding>())
                    //    continue;

                    selectedObjectsList.Add(obj.Get<UnitAi>());
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
                    WorldObj obj = hit.collider.GetComponentInParent<WorldObj>();

                    if (!obj || !obj.Has<UnitAi>())
                        return;
                    removeUnitList.Add(obj.Get<UnitAi>());
                }
                else
                {
                    selectedObjects = new WorldObj[0];
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
                        selectedObjects = new WorldObj[0];
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
                else
                {
                    WorldObj obj = hit.collider.GetComponentInParent<WorldObj>();
                    if (obj && (obj.Get<MonsterAi>() || obj.Get<MonsterSpawner>()))
                    {
                        monsterTargetSet?.Invoke(obj);
                    }
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
                selectedObjects = new WorldObj[0];
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
        List<WorldObj> selectedObjectsList = new List<WorldObj>();

        foreach (Collider2D collider in colliders)
        {
            WorldObj obj = collider.GetComponentInParent<WorldObj>();

            if (!obj || !obj.Has<UnitAi>() || obj.Has<TankCtrl>())
                continue;
            //if (collider.GetComponentInParent<Portal>() || collider.GetComponentInParent<ScienceBuilding>())
            //    continue;
            selectedObjectsList.Add(obj);
        }
        selectedObjects = selectedObjectsList.ToArray();

        removeUnit?.Invoke();

        if (selectedObjects.Length > 0)
        {
            foreach (WorldObj obj in selectedObjects)
            {
                addUnit?.Invoke(obj);
            }
            BasicUIBtns.instance.SwapFunc(false);
        }

        if (selectedObjects.Length == 1)
            InfoUI.instance.SetUnitInfo(selectedObjects[0].Get<UnitAi>());
        else
        {
            List<UnitAi> unitAiList = selectedObjectsList.Select(obj => obj.Get<UnitAi>()).ToList();
            InfoUI.instance.SetDefault();
            InfoUI.instance.UnitGroupUISet(unitAiList);
        }
    }

    private void SelectedObjects(RaycastHit2D ray)
    {
        WorldObj obj = ray.collider.GetComponentInParent<WorldObj>();

        if (!obj || !obj.Has<UnitAi>())
            return;

        removeUnit?.Invoke();
        selectedObjects = new WorldObj[1];
        selectedObjects[0] = obj;

        addUnit?.Invoke(obj);
        BasicUIBtns.instance.SwapFunc(false);

        InfoUI.instance.SetUnitInfo(obj.Get<UnitAi>());
    }

    private void SelectedSameUnit(RaycastHit2D ray)
    {
        WorldObj obj = ray.collider.GetComponentInParent<WorldObj>();

        // 클릭 대상이 UnitAi가 아니거나 TankCtrl이면 리턴
        if (!obj || !obj.Has<UnitAi>() || obj.Has<TankCtrl>())
            return;

        // 기본 선택 유닛 정보 가져오기
        removeUnit?.Invoke();
        obj.TryGet(out UnitAi unit);
        int unitIndex = unit.unitIndex;

        // 대상 유닛 리스트
        UnitAi[] unitAis = System.Array.Empty<UnitAi>();
        List<WorldObj> selectedObjectsList = new List<WorldObj>();

        if (unitIndex == 0)
        {
            unitAis = FindObjectsByType<BounceRobot>(FindObjectsSortMode.None);
        }
        else if (unitIndex == 1)
        {
            unitAis = FindObjectsByType<SentryCopterCtrl>(FindObjectsSortMode.None);
        }
        else if (unitIndex == 2)
        {
            unitAis = FindObjectsByType<SpinRobot>(FindObjectsSortMode.None);
        }
        else if (unitIndex == 3)
        {
            unitAis = FindObjectsByType<CorrosionDrone>(FindObjectsSortMode.None);
        }
        else if (unitIndex == 4)
        {
            unitAis = FindObjectsByType<RepairerDrone>(FindObjectsSortMode.None);
        }

        // 화면 안에 있고, 같은 레벨의 유닛만 선택
        foreach (var _unit in unitAis)
        {
            if (unit.unitLevel == _unit.unitLevel)
            {
                Vector3 viewportPos = Camera.main.WorldToViewportPoint(_unit.transform.position);
                if (viewportPos.z > 0 && viewportPos.x >= 0 && viewportPos.x <= 1 &&
                    viewportPos.y >= 0 && viewportPos.y <= 1)
                {
                    selectedObjectsList.Add(_unit);
                }
            }
        }

        selectedObjects = selectedObjectsList.ToArray();
        removeUnit?.Invoke();

        // 선택된 오브젝트가 있다면 모두 선택 상태로 추가
        if (selectedObjects.Length > 0)
        {
            foreach (WorldObj _obj in selectedObjects)
            {
                addUnit?.Invoke(_obj);
            }
            BasicUIBtns.instance.SwapFunc(false);
        }

        // UI 갱신
        List<UnitAi> unitAiList = selectedObjectsList
            .Select(obj => obj.Get<UnitAi>())
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
            if (cell.structure && cell.structure.Get<PortalUnitIn>())
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

    public void UnitRemoveGroup(WorldObj obj)
    {
        selectedObjects = selectedObjects.Where(x => x != obj).ToArray();
    }
}
