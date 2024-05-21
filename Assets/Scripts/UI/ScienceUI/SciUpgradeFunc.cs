using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SciUpgradeFunc : MonoBehaviour
{
    Dictionary<ScienceBtn, float> upgradeSave = new Dictionary<ScienceBtn, float>();
    #region Singleton
    public static SciUpgradeFunc instance;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of recipeList found!");
            return;
        }
        instance = this;
    }
    #endregion

    public void CoroutineSet(ScienceBtn btn, float upgradeTime)
    {
        if (upgradeSave.ContainsKey(btn))
            return;
        StartCoroutine(UpgradeTimerCoroutine(btn, upgradeTime));
    }

    IEnumerator UpgradeTimerCoroutine(ScienceBtn btn, float upgradeTime)
    {
        float upgradeTimer = 0;
        upgradeSave.Add(btn, upgradeTimer);
        while (true)
        {
            upgradeTimer += Time.deltaTime;
            btn.upgradeImg.fillAmount = upgradeTimer / upgradeTime;
            if (btn.isCore)
            {
                btn.othCoreBtn.upgradeImg.fillAmount = upgradeTimer / upgradeTime;
            }
            upgradeSave[btn] = upgradeTimer;

            if (upgradeTimer >= upgradeTime)
            {
                btn.UpgradeFunc();
                upgradeSave.Remove(btn);
                yield break;
            }

            yield return null;
        }
    }

    public void LoadCoroutineSet(ScienceBtn btn, float upgradeTime, float upgradeTimer)
    {
        StartCoroutine(LoadUpgradeTimerCoroutine(btn, upgradeTime, upgradeTimer));
    }

    IEnumerator LoadUpgradeTimerCoroutine(ScienceBtn btn, float upgradeTime, float upgradeTimer)
    {
        upgradeSave.Add(btn, upgradeTimer);
        while (true)
        {
            upgradeTimer += Time.deltaTime;
            btn.upgradeImg.fillAmount = upgradeTimer / upgradeTime;
            if (btn.isCore)
            {
                btn.othCoreBtn.upgradeImg.fillAmount = upgradeTimer / upgradeTime;
            }
            upgradeSave[btn] = upgradeTimer;
            if (upgradeTimer >= upgradeTime)
            {
                btn.UpgradeFunc();
                upgradeSave.Remove(btn);
                yield break;
            }

            yield return null;
        }
    }

    public float UpgradeTimeReturn(ScienceBtn btn)
    {
        if (upgradeSave.ContainsKey(btn))
            return upgradeSave[btn];
        else
            return 0;
    }
}
