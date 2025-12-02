using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WarningWindow : MonoBehaviour
{
    [SerializeField]
    GameObject imgPanel;
    Image imgPanelImg;
    [SerializeField]
    Text warningText;
    bool warningStart;
    float setTimer;
    float maxTimer;

    [SerializeField]
    bool testBtn = false;

    Color32 panelColor;
    Coroutine blinkCoroutine;

    #region Singleton
    public static WarningWindow instance;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        imgPanelImg = imgPanel.GetComponent<Image>();
        panelColor = imgPanelImg.color;
        maxTimer = 10;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale == 0)
        {
            return;
        }

        //if (warningStart)
        //{
        //    setTimer += Time.deltaTime;

        //    if (setTimer > maxTimer)
        //    {
        //        WarningState(false);
        //    }
        //}

        if (testBtn)
        {
            WarningState(true);
            testBtn = false;
        }
    }

    public void WarningTextSet(string text)
    {
        warningText.text = text;
        WarningState(true);
    }

    public void WarningTextSet(string text, bool isHostMap)
    {
        string mapselect;
        if (isHostMap)
            mapselect = "CASTOR Planet.";
        else
            mapselect = "POLLUX Planet.";

        warningText.text = text + " " + mapselect;
        WarningState(true);
    }

    void WarningState(bool start)
    {
        imgPanel.SetActive(start);
        warningStart = start;
        if (start)
        {
            if (blinkCoroutine != null)
                StopBlink();
            blinkCoroutine = StartCoroutine(BlinkImageCoroutine());
        }
        else
            StopBlink();
        //setTimer = 0;
    }

    void StopBlink()
    {
        StopCoroutine(blinkCoroutine);
        blinkCoroutine = null;
        Color32 col = panelColor;
        col.a = 70;
        imgPanelImg.color = col;
    }

    private IEnumerator BlinkImageCoroutine()
    {
        Color32 col = panelColor;
        col.a = 70;
        imgPanelImg.color = col;

        for (int i = 0; i < 3; i++) // 3번 점멸
        {
            // 70 -> 150 (Fade Out)
            yield return StartCoroutine(FadeAlpha(col, 70, 150, 0.7f));

            // 150 -> 70 (Fade In)
            yield return StartCoroutine(FadeAlpha(col, 150, 70, 0.7f));

            Debug.Log("blink " + (i + 1));
        }

        WarningState(false);
    }

    private IEnumerator FadeAlpha(Color32 baseColor, byte from, byte to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            byte a = (byte)Mathf.Lerp(from, to, t);

            Color32 col = baseColor;
            col.a = a;
            imgPanelImg.color = col;

            yield return null;
        }

        // 마지막 보정
        Color32 final = baseColor;
        final.a = to;
        imgPanelImg.color = final;
    }
}
