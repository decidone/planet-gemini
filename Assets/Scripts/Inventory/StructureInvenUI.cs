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

    // �����Ǻ� ui�� ����ֱ� ���� �и�
    protected void InvenInit()
    {

    }
}
