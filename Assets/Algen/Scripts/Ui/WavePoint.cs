using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WavePoint : MonoBehaviour
{
    public Indicator indicator;
    public GameObject myIndicatorObj;
    GameObject indicatorCanvas;

    private bool hasIndicator = false;
    bool isWaveStart = false;

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

    void Start()
    {
        indicatorCanvas = GameManager.instance.inventoryUiCanvas;
    }

    void Update()
    {
        if (isWaveStart)
        {
            indicator.DrawIndicator(gameObject, myIndicatorObj);            
        }
    }

    public void WaveStart(Vector3 wavePos)
    {
        gameObject.transform.position = wavePos;
        isWaveStart = true;
        myIndicatorObj.SetActive(true);
    }

    public void WaveEnd()
    {
        isWaveStart = false;
        myIndicatorObj.SetActive(false);
    }

    private void SetMyIndicator()
    {
        if (IsOffScreen() && !hasIndicator) // 화면 밖인데 내 Indicator가 없으면
        {
            hasIndicator = true;
        }
        else if (!IsOffScreen() && hasIndicator) // 화면 안인데 내 Indicator가 있으면
        {
            hasIndicator = false;
        }
    }

    private bool IsOffScreen()
    {
        Vector2 vec = Camera.main.WorldToViewportPoint(transform.position);
        if (vec.x <= 1 && vec.y <= 1 && vec.x >= 0 && vec.y >= 0) // 화면 안쪽 범위
            return false;
        else
            return true;
    }
}
