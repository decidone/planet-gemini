using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeConfirm : PopUpCtrl
{
    [SerializeField]
    GameObject itemImg;
    [SerializeField]
    GameObject itemInfoPanel;

    UpgradeBuild upgradeBuild;

    List<GameObject> icon = new List<GameObject>();

    protected override void Awake()
    {
        base.Awake();
        upgradeBuild = gameManager.GetComponent<UpgradeBuild>();
        pupUpContent = "If you run out of materials, only some buildings will be upgraded.";
        pupUpText.text = pupUpContent;
    }

    public void GetData(Dictionary<Item, int> enoughItemDic, Dictionary<Item, int> notEnoughItemDic)
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
        upgradeBuild.ConfirmEnd(true);
        CloseUI();
    }

    protected override void CancelBtnFunc()
    {
        upgradeBuild.ConfirmEnd(false);
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
