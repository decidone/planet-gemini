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
    ItemList itemList;

    private void Awake()
    {
        itemList = GameManager.instance.GetComponent<ItemList>();
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
            ScienceBtn scienceBtn = coreBtnObj.AddComponent<ScienceBtn>();
            scienceBtn.SetInfo("Core", coreLv, time, true);
        }
    }

    void SciTreeInst()
    {
        var data = ScienceInfoGet.instance.GetSciLevelData(sciClass, coreLv);

        for (int i = 0; i < data.Item1.Count; i++) 
        {
            GameObject iconUI = Instantiate(sciTreeIcon);
            iconUI.transform.SetParent(panel.transform, false);
            SciTreeIconCtrl sciTreeIconCtrl = iconUI.GetComponent<SciTreeIconCtrl>();
            Item itemData = itemList.FindData(data.Item1[i]);
            sciTreeIconCtrl.icon.sprite = itemData.icon;
            sciTreeIconCtrl.SetIcon(data.Item1[i], data.Item2[i], data.Item3[i]);   //이름, 레벨, 시간
        }
    }
}
