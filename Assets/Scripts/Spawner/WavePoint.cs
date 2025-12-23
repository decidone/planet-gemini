using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ToJ;
using QFSW.QC.Actions;

public class WavePoint : MonoBehaviour
{
    GameObject player;
    public GameObject waveObj;
    public Image waveObjGauge;
    public GameObject mapObj;
    private float defaultAngle;
    RectTransform indicatorRect;
    Vector3 offset;
    Vector2 defaultAnchorMin;
    Vector2 defaultAnchorMax;

    bool isMap1WaveStart = false;
    bool isMap2WaveStart = false;

    Vector2 map1WavePos;
    Vector2 map2WavePos;
    (Vector3, bool) loadWaveData;

    [SerializeField]
    GameObject lineObj;
    LineRenderer lineRenderer;
    GameManager gameManager;
    bool mapCameraOpen;


    #region Singleton
    public static WavePoint instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }
    #endregion

    private void Start()
    {
        gameManager = GameManager.instance;
        indicatorRect = waveObj.GetComponent<RectTransform>();
        waveObj.transform.localScale = new Vector3(1, 1, 1);

        Vector2 dir = new Vector2(Screen.width, Screen.height);
        defaultAngle = Vector2.Angle(new Vector2(0, 1), dir);
        offset = new Vector3(0, 1.5f, 0);
        defaultAnchorMin = indicatorRect.anchorMin;
        defaultAnchorMax = indicatorRect.anchorMax;
    }

    void Update()
    {
        if (Time.timeScale == 0)
        {
            return;
        }

        if (player != null)
        {
            if (isMap1WaveStart && !gameManager.isPlayerInMarket && !mapCameraOpen)
            {
                SetIndicator(true);
            }
            else if (isMap2WaveStart && !gameManager.isPlayerInMarket && !mapCameraOpen)
            {
                SetIndicator(false);
            }
            else
            {
                waveObj.SetActive(false);
            }
        }
    }

    public void MapCameraOpen(bool isOpen)
    {
        mapCameraOpen = isOpen;
    }

    public void PlayerSet(GameObject _player)
    {
        player = _player;
    }

    public void WaveStart(Vector3 wavePos, bool isInHostMap)
    {
        if(isInHostMap)
        {
            map1WavePos = wavePos;
            isMap1WaveStart = true;
        }
        else
        {
            map2WavePos = wavePos;
            isMap2WaveStart = true;
        }

        StartCoroutine(MonsterBaseMapCheck.instance.CheckPath(wavePos, isInHostMap));
        StartCoroutine(gameManager.GaugeCountDown());
    }

    public void SetLine(bool isSet, List<Vector3> movePath)
    {
        if (isSet)
        {
            Vector3 startLine = new Vector3(transform.position.x, transform.position.y, -1);

            GameObject currentLine = Instantiate(lineObj, startLine, Quaternion.identity);
            lineRenderer = currentLine.GetComponent<LineRenderer>();
            Vector3[] movePathArr = movePath.ToArray();
            lineRenderer.positionCount = movePathArr.Length;
            lineRenderer.SetPositions(movePathArr);
        }
        else
        {
            if (lineRenderer != null)
            {
                Destroy(lineRenderer.gameObject);
                movePath = null;
            }
        }
    }

    public void LoadWaveStart(Vector3 wavePos, bool isInHostMap)
    {
        loadWaveData = (wavePos, isInHostMap);
        StartCoroutine(AstarScanCheck());
    }

    IEnumerator AstarScanCheck()
    {
        while (!MapGenerator.instance.isCompositeDone)
        {
            yield return null;
        }

        WaveStart(loadWaveData.Item1, loadWaveData.Item2);
    }

    public void WaveEnd(bool isInHostMap)
    {
        if (isInHostMap)
        {
            isMap1WaveStart = false;
        }
        else
        {
            isMap2WaveStart = false;
        }
        waveObj.SetActive(false);
        mapObj.SetActive(false);
        SetLine(false, null);
    }

    public void SetIndicator(bool isInHostMap)
    {
        if (gameManager.isPlayerInHostMap)  // 플레이어가 호스트맵에 있을 때
        {
            if (isInHostMap) //웨이브가 호스트맵에 있을 때 스포너 위치를 타겟
            {
                transform.position = map1WavePos;
            }
            else // 웨이브가 클라이언트맵에 있을 때 포탈 위치를 타겟
            {
                transform.position = gameManager.portal[0].transform.position;
            }
        }
        else  // 플레이어가 클라이언트맵에 있을 때
        {
            if (isInHostMap) //웨이브가 호스트맵에 있을 때 포탈 위치를 타겟
            {
                transform.position = gameManager.portal[1].transform.position;
            }
            else // 웨이브가 클라이언트맵에 있을 때 스포너 위치를 타겟
            {
                transform.position = map2WavePos;
            }
        }

        waveObj.SetActive(true);
        mapObj.SetActive(true);

        if (IsOffScreen())
        {
            float angle = Vector2.Angle(new Vector2(0, 1), transform.position - player.transform.position);
            int sign = player.transform.position.x > transform.position.x ? -1 : 1;
            angle *= sign;

            Vector3 target = Camera.main.WorldToViewportPoint(transform.position);

            float x = target.x - 0.5f;
            float y = target.y - 0.5f;

            if (-defaultAngle <= angle && angle <= defaultAngle)
            {
                float anchorMinMaxY = 0.94f;

                float anchorMinMaxX = x * (anchorMinMaxY - 0.5f) / y + 0.5f;

                if (anchorMinMaxX >= 0.94f) anchorMinMaxX = 0.94f;
                else if (anchorMinMaxX <= 0.06f) anchorMinMaxX = 0.06f;

                indicatorRect.anchorMin = new Vector2(anchorMinMaxX, anchorMinMaxY);
                indicatorRect.anchorMax = new Vector2(anchorMinMaxX, anchorMinMaxY);
            }
            else if (defaultAngle <= angle && angle <= 180 - defaultAngle)
            {
                float anchorMinMaxX = 0.94f;

                float anchorMinMaxY = y * (anchorMinMaxX - 0.5f) / x + 0.5f;

                if (anchorMinMaxY >= 0.96f) anchorMinMaxY = 0.96f;
                else if (anchorMinMaxY <= 0.04f) anchorMinMaxY = 0.04f;

                indicatorRect.anchorMin = new Vector2(anchorMinMaxX, anchorMinMaxY);
                indicatorRect.anchorMax = new Vector2(anchorMinMaxX, anchorMinMaxY);
            }
            else if (-180 + defaultAngle <= angle && angle <= -defaultAngle)
            {
                float anchorMinMaxX = 0.06f;

                float anchorMinMaxY = (y * (anchorMinMaxX - 0.5f) / x) + 0.5f;

                if (anchorMinMaxY >= 0.96f) anchorMinMaxY = 0.96f;
                else if (anchorMinMaxY <= 0.04f) anchorMinMaxY = 0.04f;

                indicatorRect.anchorMin = new Vector2(anchorMinMaxX, anchorMinMaxY);
                indicatorRect.anchorMax = new Vector2(anchorMinMaxX, anchorMinMaxY);
            }
            else if (-180 <= angle && angle <= -180 + defaultAngle || 180 - defaultAngle <= angle && angle <= 180)
            {
                float anchorMinMaxY = 0.06f;

                float anchorMinMaxX = x * (anchorMinMaxY - 0.5f) / y + 0.5f;

                if (anchorMinMaxX >= 0.94f) anchorMinMaxX = 0.94f;
                else if (anchorMinMaxX <= 0.06f) anchorMinMaxX = 0.06f;

                indicatorRect.anchorMin = new Vector2(anchorMinMaxX, anchorMinMaxY);
                indicatorRect.anchorMax = new Vector2(anchorMinMaxX, anchorMinMaxY);
            }

            indicatorRect.anchoredPosition = new Vector3(0, 0, 0);
        }
        else
        {
            indicatorRect.anchorMin = defaultAnchorMin;
            indicatorRect.anchorMax = defaultAnchorMax;
            indicatorRect.position = Camera.main.WorldToScreenPoint(transform.position + offset);
        }
    }

    private bool IsOffScreen()
    {
        Vector2 vec = Camera.main.WorldToViewportPoint(transform.position);
        if (vec.x >= 0 && vec.x <= 1 && vec.y >= 0 && vec.y <= 1)
        {
            //waveObj.SetActive(false);
            return false;
        }
        else
        {
            //waveObj.SetActive(true);
            return true;
        }
    }
}
