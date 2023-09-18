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

    void Start()
    {
        unitLayer = LayerMask.NameToLayer("Unit");
        monsterLayer = LayerMask.NameToLayer("Monster");
    }

    void Update()
    {
        if (selectedObjects.Length > 0)
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                isPKeyPressed = true;
                unitCtrlKeyPressed = true;
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                isAKeyPressed = true;
                unitCtrlKeyPressed = true;
            }
            else if (Input.GetKeyDown(KeyCode.H))
            {
                unitHoldSet?.Invoke();
            }
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
                SetTargetPosition(true);
            }
        }
        ReSetBool();
    }

    public void RightMouseUp()
    {
        SetTargetPosition(false);
        ReSetBool();
    }

    protected override List<GameObject> GroupSelectedObjects(Vector2 startPosition, Vector2 endPosition, int layer)
    {
        List<GameObject> List = base.GroupSelectedObjects(startPosition, endPosition, layer);
        selectedObjects = List.ToArray();

        removeUnit?.Invoke();

        if (selectedObjects.Length > 0)
        {
            foreach (GameObject obj in selectedObjects)
            {
                addUnit?.Invoke(obj);
            }
            isSelectingUnits = true;
        }

        return null;
    }

    private void SelectedObjects(RaycastHit2D ray)
    {
        removeUnit?.Invoke();
        addUnit?.Invoke(ray.collider.gameObject);
        isSelectingUnits = true;
    }

    void SetTargetPosition(bool isAttack)
    {
        Plane plane = new Plane(Vector3.back, transform.position);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (plane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            targetPosition = new Vector2(hitPoint.x, hitPoint.y);
        }
        groupCenterSet?.Invoke();

        targetSet?.Invoke(targetPosition, isAttack);
    }

    void ReSetBool()
    {
        unitCtrlKeyPressed = false;
        isPKeyPressed = false;
        isAKeyPressed = false;
    }
}
