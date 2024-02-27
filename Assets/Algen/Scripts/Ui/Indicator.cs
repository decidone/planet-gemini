using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Indicator : MonoBehaviour
{
    GameManager gameManager;
    GameObject player; // player 오브젝트

    private Vector2 playerVec; // Player 벡터
    private Vector2 playerScreenVec; // Screen 상의 Player 벡터

    private float angleRU; // 우측상단 대각선 각도

    private float screenHalfHeight = 0.5f; // 화면 높이 절반
    private float screenHalfWidth = 0.5f; // 화면 폭 절반

    #region Singleton
    public static Indicator instance;

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

    public void PlayerSet(GameObject playerSet)
    {
        player = playerSet;
        playerScreenVec = Camera.main.WorldToScreenPoint(player.transform.position);
        playerVec = Camera.main.WorldToViewportPoint(player.transform.position); // 0f ~ 1f

        Vector2 vecRU = new Vector2(Screen.width, Screen.height) - playerScreenVec;
        vecRU = vecRU.normalized;
        angleRU = Vector2.Angle(vecRU, Vector2.up);
    }

    public void DrawIndicator(GameObject obj, GameObject indicatorObj)
    {
        Image indicator = indicatorObj.GetComponent<Image>();

        Vector2 objScreenVec = Camera.main.WorldToScreenPoint(obj.transform.position);
        Vector2 objVec = Camera.main.WorldToViewportPoint(obj.transform.position); // 0f ~ 1f

        Vector2 targetVec = objScreenVec - playerScreenVec;
        targetVec = targetVec.normalized;

        float targetAngle = Vector2.Angle(targetVec, Vector2.up); // 0 ~ 180
        int sign = Vector3.Cross(targetVec, Vector2.up).z < 0 ? -1 : 1;
        targetAngle *= sign; // -180 ~ 180

        float xPrime = objVec.x - playerVec.x;
        float yPrime = objVec.y - playerVec.y;

        float anchorMinX;
        float anchorMinY;
        float anchorMaxX;
        float anchorMaxY;

        if (-angleRU < targetAngle && angleRU >= targetAngle) // UP 쪽에 있을 때
        {
            anchorMinY = 0.94f;
            anchorMaxY = 0.94f;
            // y anchor 지정

            float posX = (Mathf.Abs(xPrime) * screenHalfHeight) / yPrime;

            if (xPrime > 0) // Right
            {
                anchorMinX = screenHalfWidth + posX;
                anchorMaxX = screenHalfWidth + posX;

                if (anchorMinX > 0.965f) anchorMinX = 0.965f;
                if (anchorMaxX > 0.965f) anchorMaxX = 0.965f;
                // 이미지가 넘어가는 걸 방지
            }
            else // Left
            {
                anchorMinX = screenHalfWidth - posX;
                anchorMaxX = screenHalfWidth - posX;

                if (anchorMinX < 0.035f) anchorMinX = 0.035f;
                if (anchorMaxX < 0.035f) anchorMaxX = 0.035f;
                // 이미지가 넘어가는 걸 방지
            }

            indicator.rectTransform.anchorMin = new Vector2(anchorMinX, anchorMinY);
            indicator.rectTransform.anchorMax = new Vector2(anchorMaxX, anchorMaxY);
            // indicator의 anchor 지정
        }
        else if (angleRU < targetAngle && 180 - angleRU >= targetAngle) // RIGHT 쪽에 있을 떄
        {
            anchorMinX = 0.965f;
            anchorMaxX = 0.965f;
            // x anchor 지정

            float posY = (screenHalfWidth * Mathf.Abs(yPrime)) / xPrime;

            if (yPrime > 0) // Up
            {
                anchorMinY = screenHalfHeight + posY;
                anchorMaxY = screenHalfHeight + posY;

                if (anchorMinY > 0.94f) anchorMinY = 0.94f;
                if (anchorMaxY > 0.94f) anchorMaxY = 0.94f;
                // 이미지가 넘어가는 걸 방지
            }
            else // Down
            {
                anchorMinY = screenHalfHeight - posY;
                anchorMaxY = screenHalfHeight - posY;

                if (anchorMinY < 0.04f) anchorMinY = 0.04f;
                if (anchorMaxY < 0.04f) anchorMaxY = 0.04f;
                // 이미지가 넘어가는 걸 방지
            }

            indicator.rectTransform.anchorMin = new Vector2(anchorMinX, anchorMinY);
            indicator.rectTransform.anchorMax = new Vector2(anchorMaxX, anchorMaxY);
            // indicator의 anchor 지정
        }
        else if ((180 - angleRU < targetAngle && 180 > targetAngle)
            || (-180 <= targetAngle && angleRU - 180 >= targetAngle)) // DOWN 쪽에 있을 때
        {
            anchorMinY = 0.04f;
            anchorMaxY = 0.04f;

            float posX = (Mathf.Abs(xPrime) * screenHalfHeight) / -yPrime;

            if (xPrime > 0) // Right
            {
                anchorMinX = screenHalfWidth + posX;
                anchorMaxX = screenHalfWidth + posX;

                if (anchorMinX > 0.965f) anchorMinX = 0.965f;
                if (anchorMaxX > 0.965f) anchorMaxX = 0.965f;
            }
            else // Left
            {
                anchorMinX = screenHalfWidth - posX;
                anchorMaxX = screenHalfWidth - posX;

                if (anchorMinX < 0.035f) anchorMinX = 0.035f;
                if (anchorMaxX < 0.035f) anchorMaxX = 0.035f;
            }

            indicator.rectTransform.anchorMin = new Vector2(anchorMinX, anchorMinY);
            indicator.rectTransform.anchorMax = new Vector2(anchorMaxX, anchorMaxY);
        }
        else if (angleRU - 180 < targetAngle && -angleRU >= targetAngle) // LEFT 쪽에 있을 때
        {
            anchorMinX = 0.035f;
            anchorMaxX = 0.035f;

            float posY = (screenHalfWidth * Mathf.Abs(yPrime)) / -xPrime;

            if (yPrime > 0) // Up
            {
                anchorMinY = screenHalfWidth + posY;
                anchorMaxY = screenHalfWidth + posY;

                if (anchorMinY > 0.94f) anchorMinY = 0.94f;
                if (anchorMaxY > 0.94f) anchorMaxY = 0.94f;
            }
            else // Down
            {
                anchorMinY = screenHalfWidth - posY;
                anchorMaxY = screenHalfWidth - posY;

                if (anchorMinY < 0.04f) anchorMinY = 0.04f;
                if (anchorMaxY < 0.04f) anchorMaxY = 0.04f;
            }
            indicator.rectTransform.anchorMin = new Vector2(anchorMinX, anchorMinY);
            indicator.rectTransform.anchorMax = new Vector2(anchorMaxX, anchorMaxY);
        }

        indicator.rectTransform.anchoredPosition = new Vector3(0, 0);
        // 위에서 지정한 anchor로 이동
    }
}

