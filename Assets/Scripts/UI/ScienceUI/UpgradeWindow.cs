using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeWindow : MonoBehaviour
{
    [SerializeField]
    Button okBtn;
    [SerializeField]
    Button cancelBtn;
    ScienceBtn scienceBtn;

    private void Awake()
    {
        okBtn.onClick.AddListener(() => OkBtnFunc());
        cancelBtn.onClick.AddListener(() => CancelBtnFunc());
        CloseUI();
    }

    public void SetBtn(ScienceBtn btn)
    {
        scienceBtn = btn;
    }

    void OkBtnFunc()
    {
        ScienceManager.instance.UpgradeStart(scienceBtn);
        CloseUI();
    }

    void CancelBtnFunc()
    {
        CloseUI();
    }

    public void CloseUI()
    {
        scienceBtn = null;
        this.gameObject.SetActive(false);
    }
}
