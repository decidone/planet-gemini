using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveConfirm : PopUpCtrl
{
    RemoveBuild removeBuild;

    protected override void Awake()
    {
        base.Awake();
        removeBuild = gameManager.GetComponent<RemoveBuild>();
        pupUpContent = "Do you really want to remove it?";
        pupUpText.text = pupUpContent;
    }

    public override void OkBtnFunc()
    {
        removeBuild.ConfirmEnd(true);
        CloseUI();
    }

    protected override void CancelBtnFunc()
    {
        removeBuild.ConfirmEnd(false);
        CloseUI();
    }

    public override void OpenUI()
    {
        base.OpenUI();
        removeBuild.isRemovePopUpOn = true;
    }

    public override void CloseUI()
    {
        base.CloseUI();
        removeBuild.isRemovePopUpOn = false;
    }
}
