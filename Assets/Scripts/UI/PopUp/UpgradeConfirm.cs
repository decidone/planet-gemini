using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeConfirm : PopUpCtrl
{
    [SerializeField]
    GameObject itemImg;
    [SerializeField]
    GameObject itemInfoPanel;

    string openUIName;
    UpgradeBuild upgradeBuild;
    InfoUI infoUI;
    List<GameObject> icon = new List<GameObject>();

    protected override void Awake()
    {
        base.Awake();
        upgradeBuild = gameManager.GetComponent<UpgradeBuild>();
        infoUI = InfoUI.instance;
        pupUpContent = "If you run out of materials, you cannot upgrade.";
        pupUpText.text = pupUpContent;
    }

    public void GetData(Dictionary<Item, int> enoughItemDic, Dictionary<Item, int> notEnoughItemDic, string _openUIName)
    {
        ResetUi();

        if (notEnoughItemDic != null)
        {
            foreach (var kvp in notEnoughItemDic)
            {
                Item key = kvp.Key;
                int value = kvp.Value;
                GameObject itemSlot = Instantiate(itemImg);
                icon.Add(itemSlot);
                itemSlot.SetActive(true);
                itemSlot.transform.SetParent(itemInfoPanel.transform, false);
                itemSlot.GetComponent<BuildingImgCtrl>().AddItem(key, value, false);
            }
        }
        if (enoughItemDic != null)
        {
            foreach (var kvp in enoughItemDic)
            {
                Item key = kvp.Key;
                int value = kvp.Value;
                GameObject itemSlot = Instantiate(itemImg);
                icon.Add(itemSlot);
                itemSlot.SetActive(true);
                itemSlot.transform.SetParent(itemInfoPanel.transform, false);
                itemSlot.GetComponent<BuildingImgCtrl>().AddItem(key, value, true);
            }
        }
        openUIName = _openUIName;
        OpenUI();
    }

    void ResetUi()
    {
        if(icon.Count > 0)
        {
            foreach(GameObject UI in icon)
            {
                Destroy(UI);
            }
            icon.Clear();
        }
    }

    public override void OkBtnFunc()
    {
        if (openUIName == "UpgradeBuild")
        {
            upgradeBuild.ConfirmEnd(true);
        }
        else if (openUIName == "InfoUI")
        {
            infoUI.ConfirmEnd(true);
        }

        CloseUI();
    }

    protected override void CancelBtnFunc()
    {
        if (openUIName == "UpgradeBuild")
        {
            upgradeBuild.ConfirmEnd(false);
        }
        else if (openUIName == "InfoUI")
        {
            infoUI.ConfirmEnd(false);
        }

        CloseUI();
    }

    public override void OpenUI()
    {
        base.OpenUI();
    }

    public override void CloseUI()
    {
        ResetUi();
        base.CloseUI();
    }
}
