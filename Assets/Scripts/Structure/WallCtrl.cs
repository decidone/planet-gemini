using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WallCtrl : Structure
{
    [ClientRpc]
    public override void UpgradeFuncClientRpc()
    {
        base.UpgradeFuncClientRpc();
        setModel.sprite = modelNum[dirNum + level];
    }
}
