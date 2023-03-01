using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureInvenUI : InventoryUI
{
    protected override void Start()
    {
        base.Start();
        inventory.Refresh();
    }

    // 레시피별 ui를 띄워주기 위해 분리
    protected void InvenInit()
    {

    }
}
