using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WallCtrl : Structure
{
    void Start()
    {
        StrBuilt();
    }

    public override void NearStrBuilt()
    {
        // 건물을 지었을 때나 근처에 새로운 건물이 지어졌을 때 동작
        // 변경사항이 생기면 DelayNearStrBuiltCoroutine()에도 반영해야 함
        if (IsServer)
        {
            CheckPos();
            setModel.sprite = modelNum[level];
        }
        else
        {
            DelayNearStrBuilt();
        }
    }

    protected override IEnumerator DelayNearStrBuiltCoroutine()
    {
        // 동시 건설이나 그룹핑을 따로 예외처리 하는 경우가 아니면 NearStrBuilt()를 그대로 사용
        yield return new WaitForEndOfFrame();

        CheckPos();
        setModel.sprite = modelNum[level];
    }

    [ClientRpc]
    public override void UpgradeFuncClientRpc()
    {
        //base.UpgradeFuncClientRpc();
        UpgradeFunc();

        setModel.sprite = modelNum[level];
    }
}
