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
    [SerializeField]
    Text text;
    bool isCoreWaring;

    private void Awake()
    {
        okBtn.onClick.AddListener(() => OkBtnFunc());
        cancelBtn.onClick.AddListener(() => CloseUI());
        CloseUI();
    }

    public void SetBtn(ScienceBtn btn)
    {
        scienceBtn = btn;
        isCoreWaring = false;
        //if (btn.isCore)
        //{
        //    text.text = "Upgrading the core will trigger a monster wave.";
        //}
        //else
        //{
            text.text = "Would you like to upgrade the science " + scienceBtn.gameName + "?";
        //}
    }

    public void CoreWaring(int needToUpgradeCount)
    {
        isCoreWaring = true;
        text.text = "To increase the core level, you must unlock a " + needToUpgradeCount + " of sciences from the previous core level.";
    }


    public void OkBtnFunc()
    {
        if (!isCoreWaring)
        {
            ScienceManager.instance.UpgradeStart(scienceBtn);
        }

        CloseUI();
    }

    public void CloseUI()
    {
        scienceBtn = null;
        this.gameObject.SetActive(false);
        GameManager.instance.onUIChangedCallback?.Invoke(this.gameObject);
        GameManager.instance.PopUpUISetting(false);
    }
}
