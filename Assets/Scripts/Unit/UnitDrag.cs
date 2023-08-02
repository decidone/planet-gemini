using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitDrag : MonoBehaviour
{
    private Vector2 dragStartPosition;
    [SerializeField]
    private GameObject[] selectedObjects;

    int unitLayer = 0;
    int monsterLayer = 0;

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

    bool unitCtrlKeyPressed = false;
    bool isPKeyPressed = false;
    bool isAKeyPressed = false;

    public GameObject preBuilding = null;

    //bool isHKeyPressed = false;
    void Start()
    {
        unitLayer = LayerMask.NameToLayer("Unit");
        monsterLayer = LayerMask.NameToLayer("Monster");
    }

    void Update()
    {
        if (!preBuilding.activeSelf)
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
                //isHKeyPressed = true;
                //unitCtrlKeyPressed = true;
                unitHoldSet?.Invoke();
            }

            if (Input.GetMouseButtonDown(0))
            {
                dragStartPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }

            if (Input.GetMouseButtonUp(0))
            {
                if(!unitCtrlKeyPressed)
                {
                    Vector2 dragEndPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    if (dragStartPosition != dragEndPosition)
                        GroupSelectedObjects(dragStartPosition, dragEndPosition);
                    else
                    {
                        RaycastHit2D hit = Physics2D.Raycast(dragEndPosition, Vector2.zero, 0f, 1 << unitLayer);
                        if (hit)
                            SelectedObjects(hit);
                        else
                            removeUnit?.Invoke();
                    }
                }
                else if (isPKeyPressed)
                {
                    Vector2 dragEndPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    groupCenterSet?.Invoke(); 
                    patrolSet?.Invoke(dragEndPosition);
                }
                else if (isAKeyPressed)
                {
                    Vector2 dragEndPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    RaycastHit2D hit = Physics2D.Raycast(dragEndPosition, Vector2.zero, 0f, 1 << monsterLayer);
                    if(hit)
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

            if (Input.GetMouseButtonDown(1))
            {
                SetTargetPosition(false);
                ReSetBool();
            }
        }
    }

    private void GroupSelectedObjects(Vector2 startPosition, Vector2 endPosition)
    {
        Collider2D[] colliders = Physics2D.OverlapAreaAll(startPosition, endPosition, 1 << unitLayer);

        List<GameObject> selectedObjectsList = new List<GameObject>();

        foreach (Collider2D collider in colliders)
        {
            selectedObjectsList.Add(collider.gameObject);
        }

        selectedObjects = selectedObjectsList.ToArray();

        removeUnit?.Invoke();

        if (selectedObjects.Length > 0)
        {
            foreach (GameObject obj in selectedObjects)
            {
                addUnit?.Invoke(obj);
            }
        }
    }

    private void SelectedObjects(RaycastHit2D ray)
    {
        removeUnit?.Invoke();
        addUnit?.Invoke(ray.collider.gameObject);
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
        //isHKeyPressed = false;
    }
}
