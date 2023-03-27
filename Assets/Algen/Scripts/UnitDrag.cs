using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitDrag : MonoBehaviour
{
    private Vector2 dragStartPosition;
    [SerializeField]
    private GameObject[] selectedObjects;

    //UnitGroupCtrl unitGroupCtrl;
    //GameObject unitGroup;

    public delegate void AddUnitDelegate(GameObject obj);
    public static event AddUnitDelegate addUnit;
    
    public delegate void RemoveUnitDelegate();
    public static event RemoveUnitDelegate removeUnit;

    public delegate void TargetDelegate(Vector2 targetPos);
    public static event PatrolDelegate targetSet;    
    
    public delegate void PatrolDelegate(Vector2 patrolPos);
    public static event PatrolDelegate patrolSet;

    private Vector2 targetPosition;

    bool unitCtrlKeyPressed = false;
    bool isPKeyPressed = false;
    void Start()
    {
        //unitGroup = GameObject.Find("UnitGroup");
        //unitGroupCtrl = unitGroup.GetComponent<UnitGroupCtrl>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            isPKeyPressed = true;
            unitCtrlKeyPressed = true;
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
                GroupSelectedObjects(dragStartPosition, dragEndPosition);
            }
            else if (isPKeyPressed)
            {
                Vector2 dragEndPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                patrolSet?.Invoke(dragEndPosition);
            }
            ReSetBool();
        }

        if (Input.GetMouseButtonDown(1))
        {
            SetTargetPosition();
            ReSetBool();
        }
    }
    private void GroupSelectedObjects(Vector2 startPosition, Vector2 endPosition)
    {
        float startX = Mathf.Min(startPosition.x, endPosition.x);
        float startY = Mathf.Min(startPosition.y, endPosition.y);
        float endX = Mathf.Max(startPosition.x, endPosition.x);
        float endY = Mathf.Max(startPosition.y, endPosition.y);

        Rect dragRect = new Rect(startX, startY, endX - startX, endY - startY);

        GameObject[] objectsWithTag = GameObject.FindGameObjectsWithTag("Unit");
        List<GameObject> selectedObjectsList = new List<GameObject>();

        foreach (GameObject obj in objectsWithTag)
        {
            if (dragRect.Contains(obj.transform.position))
            {
                selectedObjectsList.Add(obj);
            }
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

    void SetTargetPosition()
    {
        Plane plane = new Plane(Vector3.back, transform.position);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (plane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            targetPosition = new Vector2(hitPoint.x, hitPoint.y);
        }

        targetSet?.Invoke(targetPosition);
    }

    void ReSetBool()
    {
        unitCtrlKeyPressed = false;
        isPKeyPressed = false;
    }
}
