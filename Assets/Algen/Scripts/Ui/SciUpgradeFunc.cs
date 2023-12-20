using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SciUpgradeFunc : MonoBehaviour
{
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
        StartCoroutine(UpgradeTimerCoroutine(btn, upgradeTime));
    }

    public IEnumerator UpgradeTimerCoroutine(ScienceBtn btn, float upgradeTime)
    {
        float upgradeTimer = 0;

        while (true)
        {
            upgradeTimer += Time.deltaTime;
            btn.upgradeImg.fillAmount = upgradeTimer / upgradeTime;

            if (upgradeTimer >= upgradeTime)
            {
                btn.UpgradeFunc();
                yield break;
            }

            yield return null;
        }
    }
}
