using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

// UTF-8 설정
public class ScienceCoreLvCtrl : MonoBehaviour
{
    public GameObject panel = null;
    public GameObject sciTreeIcon = null;
    public int coreLv = 0;
    [SerializeField]
    Text coreLvTx;
    List<SciTreeIconCtrl> sciTreeIcons = new List<SciTreeIconCtrl>();
    [SerializeField]
    GameObject coreBtnObj = null;
    [SerializeField]
    GameObject LockBtnObj = null;

    public void UISetting(int level)
    {
        coreLv = level + 1;
        coreLvTx.text = "Lv." + coreLv;
        if (coreLv == 1)
            LockBtnObj.SetActive(false);
        else
        {
            ScienceBtn scienceBtn = coreBtnObj.AddComponent<ScienceBtn>();
            scienceBtn.sciName = "Core";
            scienceBtn.level = coreLv;
            scienceBtn.isCore = true;
        }
    }
}
