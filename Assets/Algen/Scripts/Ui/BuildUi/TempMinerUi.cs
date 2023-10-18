using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TempMinerUi : MonoBehaviour
{
    [SerializeField]
    Image image;
    [SerializeField]
    Text setCountText;

    RectTransform uiElement;
    Vector2 startPos;
    Vector2 endPos;
    Vector2 tempPos;
    bool isOpen = false;
    float moveDuration = 0.3f;

    float moveTimer;

    private IEnumerator moveCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        uiElement = GetComponent<RectTransform>();
        startPos = new Vector2(-70, 0);
        endPos = new Vector2(70, 0);

        moveTimer = 0.0f;
    }

    public void StartMoveUIElementCoroutine(bool show, int fullAmount, int amount)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        AmountTextSet(fullAmount, amount);
        moveCoroutine = MoveUIElement(show);
        StartCoroutine(moveCoroutine);
    }

    public void AmountTextSet(int fullAmount, int amount)
    {
        setCountText.text = amount + " / " + fullAmount;
    }

    public IEnumerator MoveUIElement(bool show)
    {
        moveTimer = 0.0f;
        tempPos = this.uiElement.anchoredPosition;

        if (show && !isOpen)
        {
            while (moveTimer < moveDuration)
            {
                float t = moveTimer / moveDuration;
                uiElement.anchoredPosition = Vector2.Lerp(tempPos, endPos, t);
                moveTimer += Time.deltaTime;

                yield return null;
            }
            isOpen = true;
            uiElement.anchoredPosition = endPos;
        }
        else if(!show)
        {
            isOpen = false;
            while (moveTimer < moveDuration)
            {
                float t = moveTimer / moveDuration;
                uiElement.anchoredPosition = Vector2.Lerp(tempPos, startPos, t);
                moveTimer += Time.deltaTime;

                yield return null;
            }
            uiElement.anchoredPosition = startPos;
        }

        moveCoroutine = null;
    }
}
