using UnityEngine;

public class Credits : MonoBehaviour
{
    [SerializeField] RectTransform rectTransform;
    [SerializeField] GameObject creditsObj;
    public float scrollSpeed;
    public float stopY;
    Vector2 startPosition;
    bool uiOpened;
    bool isFinished = false;
    float startDelay = 3f;
    float delayTimer;

    void Awake()
    {
        startPosition = rectTransform.anchoredPosition;
    }

    void Update()
    {
        if (creditsObj == null) return;

        if (!creditsObj.activeSelf)
        {
            if (uiOpened) uiOpened = false;
        }
        else
        {
            if (!uiOpened)
            {
                ResetToTop();
                uiOpened = true;
            }

            if (isFinished) return;

            if (delayTimer > 0f)
            {
                delayTimer -= Time.deltaTime;
                return;
            }

            Vector2 pos = rectTransform.anchoredPosition;
            pos.y += scrollSpeed * Time.deltaTime;

            if (pos.y >= stopY)
            {
                pos.y = stopY;
                isFinished = true;
            }

            rectTransform.anchoredPosition = pos;
        }
    }

    public void ResetToTop()
    {
        rectTransform.anchoredPosition = startPosition;
        delayTimer = startDelay;
        isFinished = false;
    }
}
