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
    public ScienceBtn scienceBtn;

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
        //for (int i = 0; i < data.Item1.Count; i++)
        //{
        //    GameObject iconUI = Instantiate(sciTreeIcon);
        //    iconUI.transform.SetParent(panel.transform, false);
        //    SciTreeIconCtrl sciTreeIconCtrl = iconUI.GetComponent<SciTreeIconCtrl>();
        //    Item itemData = itemList.FindDataGetLevel(data.Item1[i], data.Item2[i]);
        //    sciTreeIconCtrl.icon.sprite = itemData.icon;
        //    string name = InGameNameDataGet.instance.ReturnName(data.Item2[i], data.Item1[i]);
        //    sciTreeIconCtrl.SetIcon(data.Item1[i], data.Item2[i], data.Item3[i], data.Item4[i], name, data.Item5[i]);   //이름, 레벨, 코어레벨, 시간
        //}
    }
}
