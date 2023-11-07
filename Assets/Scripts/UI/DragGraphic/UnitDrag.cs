using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// UTF-8 설정
public class UnitDrag : DragFunc
{
    int unitLayer;
    int monsterLayer;

    public delegate void AddUnitDelegate(GameObject obj);
    public static event AddUnitDelegate addUnit;
    
    public delegate void RemoveUnitDelegate();
    public static event RemoveUnitDelegate removeUnit;

    public delegate void TargetDelegate(Vector3 targetPos, bool isAttack);
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

    public bool isSelectingUnits = false;
    bool unitCtrlKeyPressed = false;
    bool isPKeyPressed = false;
    bool isAKeyPressed = false;

    InputManager inputManager;

    protected override void Start()
    {
        unitLayer = LayerMask.NameToLayer("Unit");
        monsterLayer = LayerMask.NameToLayer("Monster");

        inputManager = InputManager.instance;
        inputManager.controls.Unit.Attack.performed += ctx => Attack();
        inputManager.controls.Unit.Patrol.performed += ctx => Patrol();
        inputManager.controls.Unit.Hold.performed += ctx => Hold();
    }

    void Attack()
    {
        if (selectedObjects.Length > 0)
        {
            isAKeyPressed = true;
            unitCtrlKeyPressed = true;
        }
    }

    void Patrol()
    {
        if (selectedObjects.Length > 0)
        {
            isPKeyPressed = true;
            unitCtrlKeyPressed = true;
        }
    }

    void Hold()
    {
        if (selectedObjects.Length > 0)
        {
            unitHoldSet?.Invoke();
        }
    }

    public override void LeftMouseUp(Vector2 startPos, Vector2 endPos)
    {
        if (!unitCtrlKeyPressed)
        {
            if (startPos != endPos)
                GroupSelectedObjects(startPos, endPos, unitLayer);
            else
            {
                RaycastHit2D hit = Physics2D.Raycast(endPos, Vector2.zero, 0f, 1 << unitLayer);
                if (hit)
                    SelectedObjects(hit);
                else
                {
                    selectedObjects = new GameObject[0];
                    isSelectingUnits = false;
                    removeUnit?.Invoke();
                }
            }
        }
        else if (isPKeyPressed)
        {
            groupCenterSet?.Invoke();
            patrolSet?.Invoke(endPos);
        }
        else if (isAKeyPressed)
        {
            RaycastHit2D hit = Physics2D.Raycast(endPos, Vector2.zero, 0f, 1 << monsterLayer);
            if (hit)
            {
                monsterTargetSet?.Invoke(hit.collider.gameObject);
            }
            else
            {
                SetTargetPosition(true, endPos);
            }
        }
        ReSetBool();
    }

    public override void RightMouseUp(Vector2 startPos, Vector2 endPos)
    {
        SetTargetPosition(false, endPos);
        ReSetBool();
    }

    protected override void GroupSelectedObjects(Vector2 startPosition, Vector2 endPosition, int layer)
    {
        base.GroupSelectedObjects(startPosition, endPosition, layer);
        removeUnit?.Invoke();

        if (selectedObjects.Length > 0)
        {
            foreach (GameObject obj in selectedObjects)
            {
                addUnit?.Invoke(obj);
            }
            isSelectingUnits = true;
        }
    }

    private void SelectedObjects(RaycastHit2D ray)
    {
        removeUnit?.Invoke();
        addUnit?.Invoke(ray.collider.gameObject);
        isSelectingUnits = true;
    }

    void SetTargetPosition(bool isAttack, Vector2 targetPos)
    {
        groupCenterSet?.Invoke();
        targetSet?.Invoke(targetPos, isAttack);
    }

    void ReSetBool()
    {
        unitCtrlKeyPressed = false;
        isPKeyPressed = false;
        isAKeyPressed = false;
    }
}
