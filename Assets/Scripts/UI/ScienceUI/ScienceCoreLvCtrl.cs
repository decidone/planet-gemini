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
        if (coreLv == 1)
            LockBtnObj.SetActive(false);
        else
        {
            ScienceBtn scienceBtn = coreBtnObj.AddComponent<ScienceBtn>();
            scienceBtn.SetInfo("Core", coreLv, true);
        }
        sciClass = getSciClass;
        SciTreeInst();
    }

    void SciTreeInst()
    {
        Dictionary<string, int> getSciData = new Dictionary<string, int>(ScienceInfoGet.instance.GetSciLevelData(sciClass, coreLv));

        foreach (var sciData in getSciData)
        {
            GameObject iconUI = Instantiate(sciTreeIcon);
            iconUI.transform.SetParent(panel.transform, false);
            SciTreeIconCtrl sciTreeIconCtrl = iconUI.GetComponent<SciTreeIconCtrl>();
            Item itemData = itemList.FindData(sciData.Key);
            sciTreeIconCtrl.icon.sprite = itemData.icon;
            sciTreeIconCtrl.SetIcon(sciData.Key, sciData.Value);
        }
    }
}
