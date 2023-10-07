using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawLine : MonoBehaviour
{
    bool isDrawing;
    Vector2 startPosition;

    [SerializeField]
    GameObject lineObj;
    List<GameObject> lineList = new List<GameObject>();
    LineRenderer lineRenderer;

    void Start()
    {
        isDrawing = false;
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            // 시작 위치 설정
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            startPosition = new Vector3(mousePosition.x, mousePosition.y, 4f); // z축 값을 4로 고정
            isDrawing = true;

            // 라인 렌더러 초기화
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, startPosition);
            lineRenderer.SetPosition(1, startPosition);
        }

        // 마우스 이동 중일 때 라인 그리기
        if (isDrawing && Input.GetMouseButton(1))
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 4f; // z축 값을 4로 고정

            // 라인 렌더러의 시작 위치와 마우스 위치를 설정
            lineRenderer.SetPosition(0, startPosition);
            lineRenderer.SetPosition(1, mousePosition);
        }

        // 마우스 왼쪽 버튼 떼면 그리기 중지
        if (Input.GetMouseButtonUp(1))
        {
            isDrawing = false;
        }
        //bool isRightMouseButtonUp = Input.GetMouseButtonUp(1);

        //if (isDrawing)
        //{
        //    Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //    startPosition = new Vector3(mousePosition.x, mousePosition.y, 4f);

        //    lineRenderer.SetPosition(1, mousePosition);

        //    if (isRightMouseButtonUp)
        //    {
        //        EndDraw();
        //    }
        //}
    }

    public void LineDrawStart(Vector2 startPos)
    {
        GameObject currentLine = Instantiate(lineObj, startPos, Quaternion.identity);
        lineRenderer = currentLine.GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, startPosition);

        startPosition = startPos;
        isDrawing = true;
    }

    void EndDraw()
    {
        isDrawing = false;
        lineList.Add(lineRenderer.gameObject);
    }
}
