using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class TankCtrl : UnitAi
{
    bool playerOnTank;
    [SerializeField]
    Image reloadingBar;
    [SerializeField]
    Image reloadingBackBar;
    bool reloading;
    float reloadTimer;
    float reloadInterval;

    protected override void Update()
    {
        if (!IsServer)
            return;

        if (hp != maxHp && aIState != AIState.AI_Die)
        {
            selfHealTimer += Time.deltaTime;

            if (selfHealTimer >= selfHealInterval)
            {
                SelfHealingServerRpc();
                selfHealTimer = 0f;
            }
        }

        if (reloading)
        {
            reloadTimer += Time.deltaTime;
            reloadingBar.fillAmount = reloadTimer / reloadInterval;

            if (reloadTimer >= reloadInterval)
            {
                reloading = false;
                ReloadingUISet(false);
            }
        }
    }

    protected override void FixedUpdate() { }

    [ServerRpc(RequireOwnership = false)]
    public override void ClientConnectSyncServerRpc()
    {
        base.ClientConnectSyncServerRpc();
        if (playerOnTank)
        {
            TankOnClientRpc();
        }
    }

    public void PlayerTankOn()
    {
        TankOnServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    void TankOnServerRpc()
    {
        TankOnClientRpc();
    }

    [ClientRpc]
    void TankOnClientRpc()
    {
        playerOnTank = true;
        gameObject.SetActive(false);
    }

    public void PlayerTankOff(Vector3 pos)
    {
        TankOffServerRpc(pos);
    }

    [ServerRpc(RequireOwnership = false)]
    void TankOffServerRpc(Vector3 pos)
    {
        TankOffClientRpc(pos);
    }

    [ClientRpc]
    void TankOffClientRpc(Vector3 pos)
    {
        playerOnTank = false;
        transform.position = pos;
        gameObject.SetActive(true);
    }

    void ReloadingUISet(bool isOn)
    {
        if (isOn)
        {
            reloadingBar.enabled = true;
            reloadingBackBar.enabled = true;
        }
        else
        {
            reloadingBar.enabled = false;
            reloadingBackBar.enabled = false;
        }
    }

    public void TankDestory()
    {
        DieFuncServerRpc();
    }
}
