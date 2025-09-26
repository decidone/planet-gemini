using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

// UTF-8 설정
public class ScienceCoreLvCtrl : MonoBehaviour
{
    public GameObject panel;
    public GameObject sciTreeIcon;
    string sciClass;
    int coreLv;
    [SerializeField]
    Text coreLvTx;
    [SerializeField]
    GameObject coreBtnObj;
    [SerializeField]
    GameObject LockBtnObj;
    Image lockBtnImg;
    ItemList itemList;
    public ScienceBtn scienceBtn;
    Coroutine blinkCoroutine;

    private void Awake()
    {
        itemList = ItemList.instance;
    }

    private void Start()
    {
        lockBtnImg = LockBtnObj.GetComponent<Image>();
    }

    public void UISetting(int level, string getSciClass)
    {
        coreLv = level + 1;
        coreLvTx.text = "Lv." + coreLv;
        sciClass = getSciClass;
        SciTreeInst();
        float time = ScienceInfoGet.instance.CoreUpgradeTime(coreLv);
        if (coreLv == 1)
            LockBtnObj.SetActive(false);
        else
        {
            scienceBtn = coreBtnObj.AddComponent<ScienceBtn>();
            scienceBtn.SetInfo("Core", coreLv, coreLv, time, true, "Core", false);
        }
    }

    void SciTreeInst()
    {
        var data = ScienceInfoGet.instance.GetSciLevelData(coreLv);
        foreach (var scienceData in data)
        {
            GameObject iconUI = Instantiate(sciTreeIcon);
            iconUI.transform.SetParent(panel.transform, false);
            SciTreeIconCtrl sciTreeIconCtrl = iconUI.GetComponent<SciTreeIconCtrl>();
            Item itemData = itemList.FindDataGetLevel(scienceData.Value.Item1, scienceData.Value.Item2);
            sciTreeIconCtrl.icon.sprite = itemData.icon;
            string name = InGameNameDataGet.instance.ReturnName(scienceData.Value.Item2, scienceData.Value.Item1);
            sciTreeIconCtrl.SetIcon(scienceData.Value.Item1, scienceData.Value.Item2, scienceData.Value.Item3, scienceData.Value.Item4, name, scienceData.Value.Item5);   //이름, 레벨, 코어레벨, 시간
        }
    }

    public void StartBlink()
    {
        if (!lockBtnImg) return;
        if(blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);
        blinkCoroutine = StartCoroutine(BlinkImageCoroutine());
    }

    private IEnumerator BlinkImageCoroutine()
    {
        Color32 col = lockBtnImg.color;

        for (int i = 0; i < 3; i++) // 3번 점멸
        {
            // 100 -> 0 (Fade Out)
            yield return StartCoroutine(FadeAlpha(col, 100, 0, 0.15f));

            // 0 -> 100 (Fade In)
            yield return StartCoroutine(FadeAlpha(col, 0, 100, 0.15f));
        }
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
            lockBtnImg.color = col;

            yield return null;
        }

        // 마지막 보정
        Color32 final = baseColor;
        final.a = to;
        lockBtnImg.color = final;
    }
}
