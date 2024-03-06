using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WavePoint : MonoBehaviour
{
    GameObject player;
    public GameObject instanceObj;
    public GameObject mapObj;
    private float defaultAngle;
    bool isWaveStart = false;

    [SerializeField]
    protected GameObject lineObj;
    LineRenderer lineRenderer;
    protected Vector3 basePos;

    #region Singleton
    public static WavePoint instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of GameManager found!");
            return;
        }

        instance = this;
    }
    #endregion

    private void Start()
    {
        instanceObj.transform.localScale = new Vector3(1, 1, 1);

        Vector2 dir = new Vector2(Screen.width, Screen.height);
        defaultAngle = Vector2.Angle(new Vector2(0, 1), dir);
    }


    void Update()
    {
        if (isWaveStart)
        {
            SetIndicator();
        }
    }

    public void PlayerSet(GameObject _player, Vector3 _basePos)
    {
        player = _player;
        basePos = _basePos;
    }

    public void WaveStart(Vector3 wavePos)
    {
        transform.position = wavePos;
        isWaveStart = true;
        SetIndicator();
        instanceObj.SetActive(true);
        mapObj.SetActive(true);

        if(lineRenderer != null)
            Destroy(lineRenderer);

        GameObject currentLine = Instantiate(lineObj, wavePos, Quaternion.identity);
        lineRenderer = currentLine.GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, wavePos);
        lineRenderer.SetPosition(1, basePos);
    }

    public void WaveEnd()
    {
        isWaveStart = false;
        instanceObj.SetActive(false);
        mapObj.SetActive(false);

        Destroy(lineRenderer);
    }

    public void SetIndicator()
    {
        if (!isOffScreen())
            return;

        float angle = Vector2.Angle(new Vector2(0, 1), transform.position - player.transform.position);
        int sign = player.transform.position.x > transform.position.x ? -1 : 1;
        angle *= sign;

        Vector3 target = Camera.main.WorldToViewportPoint(transform.position);

        float x = target.x - 0.5f;
        float y = target.y - 0.5f;

        RectTransform indicatorRect = instanceObj.GetComponent<RectTransform>();

        if (-defaultAngle <= angle && angle <= defaultAngle)
        {
            //anchor minY, maxY 0.96

            float anchorMinMaxY = 0.96f;

            float anchorMinMaxX = x * (anchorMinMaxY - 0.5f) / y + 0.5f;

            if (anchorMinMaxX >= 0.94f) anchorMinMaxX = 0.94f;
            else if (anchorMinMaxX <= 0.06f) anchorMinMaxX = 0.06f;

            indicatorRect.anchorMin = new Vector2(anchorMinMaxX, anchorMinMaxY);
            indicatorRect.anchorMax = new Vector2(anchorMinMaxX, anchorMinMaxY);
        }
        else if (defaultAngle <= angle && angle <= 180 - defaultAngle)
        {
            //anchor minX, maxX 0.94

            float anchorMinMaxX = 0.94f;

            float anchorMinMaxY = y * (anchorMinMaxX - 0.5f) / x + 0.5f;

            if (anchorMinMaxY >= 0.96f) anchorMinMaxY = 0.96f;
            else if (anchorMinMaxY <= 0.04f) anchorMinMaxY = 0.04f;

            indicatorRect.anchorMin = new Vector2(anchorMinMaxX, anchorMinMaxY);
            indicatorRect.anchorMax = new Vector2(anchorMinMaxX, anchorMinMaxY);
        }
        else if (-180 + defaultAngle <= angle && angle <= -defaultAngle)
        {
            //anchor minX, maxX 0.06

            float anchorMinMaxX = 0.06f;

            float anchorMinMaxY = (y * (anchorMinMaxX - 0.5f) / x) + 0.5f;

            if (anchorMinMaxY >= 0.96f) anchorMinMaxY = 0.96f;
            else if (anchorMinMaxY <= 0.04f) anchorMinMaxY = 0.04f;

            indicatorRect.anchorMin = new Vector2(anchorMinMaxX, anchorMinMaxY);
            indicatorRect.anchorMax = new Vector2(anchorMinMaxX, anchorMinMaxY);
        }
        else if (-180 <= angle && angle <= -180 + defaultAngle || 180 - defaultAngle <= angle && angle <= 180)
        {
            //anchor minY, maxY 0.04

            float anchorMinMaxY = 0.04f;

            float anchorMinMaxX = x * (anchorMinMaxY - 0.5f) / y + 0.5f;

            if (anchorMinMaxX >= 0.94f) anchorMinMaxX = 0.94f;
            else if (anchorMinMaxX <= 0.06f) anchorMinMaxX = 0.06f;

            indicatorRect.anchorMin = new Vector2(anchorMinMaxX, anchorMinMaxY);
            indicatorRect.anchorMax = new Vector2(anchorMinMaxX, anchorMinMaxY);
        }

        indicatorRect.anchoredPosition = new Vector3(0, 0, 0);
    }


    private bool isOffScreen()
    {
        Vector2 vec = Camera.main.WorldToViewportPoint(transform.position);
        if (vec.x >= 0 && vec.x <= 1 && vec.y >= 0 && vec.y <= 1)
        {
            instanceObj.SetActive(false);
            return false;
        }
        else
        {
            instanceObj.SetActive(true);
            return true;
        }
    }
}
